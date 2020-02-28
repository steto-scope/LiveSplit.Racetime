using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.Model.Input;
using LiveSplit.Racetime.Model;
using LiveSplit.Web;

namespace LiveSplit.Racetime.Controller
{
    public class RacetimeChannel
    {

        public const int bufferSize = 20480;
        public const int maxBufferSize = 2097152;
        public readonly int[] reconnectDelays = {0, 5, 5, 10, 10, 10, 15};

        public string FullWebRoot => string.Format("{0}://{1}/", Properties.Resources.PROTOCOL_REST, Properties.Resources.DOMAIN);
        public string FullSocketRoot => string.Format("{0}://{1}/", Properties.Resources.PROTOCOL_WEBSOCKET, Properties.Resources.DOMAIN);

        public Race Race { get; set; }
        public UserStatus PersonalStatus
        {
            get
            {
                var u = Race?.Entrants?.FirstOrDefault(x => x.Name.ToLower() == RacetimeAPI.Instance.Authenticator.Identity?.Name.ToLower());
                if (u == null)
                    return UserStatus.Unknown;
                return u.Status;
            }
        }
        protected ITimerModel Model { get; set; }

        private ClientWebSocket ws;
        protected List<ChatMessage> log = new List<ChatMessage>();
        public int ConnectionError { get; set; }
        public bool IsConnected { get; set; }



        CancellationTokenSource websocket_cts;
        CancellationTokenSource reconnect_cts;

        public RacetimeChannel(LiveSplitState state, ITimerModel model)
        {

            reconnect_cts = new CancellationTokenSource();

            Model = model;

            state.OnSplit += State_OnSplit;
            state.OnUndoSplit += State_OnUndoSplit;
            state.OnReset += State_OnReset;
            model.OnPause += Model_OnPause;
        }

        private void Model_OnPause(object sender, EventArgs e)
        {
            Model.Pause();
        }

        private async void Reconnect()
        {
            if (ConnectionError>=0 && Race != null && !RacetimeAPI.Instance.Authenticator.IsAuthorizing)
            {
                Connect(Race.Id);
            }
        }

        private async Task<bool> ReceiveAndProcess()
        {
            WebSocketReceiveResult result;
            string msg = "";
            byte[] buf = new byte[bufferSize];

            try
            {
                int maxBufferSize = RacetimeChannel.maxBufferSize;
                int read = 0;
                int free = buf.Length;


                do
                {
                    if (free < 1)
                    {
                        var newSize = buf.Length + (bufferSize);
                        if (newSize > maxBufferSize)
                        {
                            throw new InternalBufferOverflowException();
                        }
                        var newBuf = new byte[newSize];
                        Array.Copy(buf, 0, newBuf, 0, read);
                        buf = newBuf;
                        free = buf.Length - read;
                    }
                    result = await ws.ReceiveAsync(new ArraySegment<byte>(buf, read, free), websocket_cts.Token);
                    if (websocket_cts.IsCancellationRequested)
                        return false;
                    read += result.Count;
                    free -= result.Count;
                }
                while (!result.EndOfMessage);


                msg = Encoding.UTF8.GetString(buf, 0, read);
                RawMessageReceived?.Invoke(this, msg);
            }
            catch (InternalBufferOverflowException)
            {
                //flush socket
                while (!(result = await ws.ReceiveAsync(new ArraySegment<byte>(buf, 0, buf.Length), websocket_cts.Token)).EndOfMessage)
                    ;

                SendSystemMessage("Content too large to load");
                return false;
            }
            catch
            {
                return false;
            }

            RawMessageReceived?.Invoke(this, msg);

            IEnumerable<ChatMessage> chatmessages = Parse(JSON.FromString(msg));


            ChatMessage racemessage = chatmessages.FirstOrDefault(x => x.Type == MessageType.Race);
            if (racemessage != null)
            {
                UpdateRaceData((RaceMessage)racemessage);
            }

            var errormsg = chatmessages.FirstOrDefault(x => x.Type == MessageType.Error)?.Message;
            if (errormsg != null && string.Join("", errormsg).Contains("Permission denied"))
            {                
                ForceReload();
                return true;
            }
            else if (errormsg != null)
            {
                StateChanged?.Invoke(this, Race.State);
                UserListRefreshed?.Invoke(this, new EventArgs());
            }
            MessageReceived?.Invoke(this, chatmessages);
            return true;
        }


        public async Task RunAsync(string id)
        {
start:
            if (IsConnected)
            {
                return;
            }
            websocket_cts = new CancellationTokenSource();
            var Authenticator = RacetimeAPI.Instance.Authenticator;

            using (ws = new ClientWebSocket())
            {
                IsConnected = true;

                if (await Authenticator.Authorize())
                {
                    SendSystemMessage($"Authorization successful. Hello, {Authenticator.Identity?.Name}");
                    Authorized?.Invoke(this, null);
                }
                else
                {
                    SendSystemMessage(Authenticator.Error, true);
                    AuthenticationFailed?.Invoke(this, new EventArgs());
                    ConnectionError++;
                    goto cleanup;
                }
                //opening the socket
                ws.Options.SetRequestHeader("Authorization", $"Bearer {Authenticator.AccessToken}");
                try
                {
                    await ws.ConnectAsync(new Uri(FullSocketRoot + "ws/o/race/" + id), websocket_cts.Token);
                }
                catch (WebSocketException wex)
                {
                    ConnectionError++;
                    goto cleanup;
                }

                //initial command to sync LiveSplit 
                if (ws.State == WebSocketState.Open)
                {

                    ChannelJoined?.Invoke(this, new EventArgs());
                    SendSystemMessage($"Joined Channel '{id}'");
                    try
                    {
                        ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes("{ \"action\":\"getrace\" }"));
                        ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                        await ReceiveAndProcess();

                        if (Race.StartedAt != DateTime.MaxValue)
                            Model.CurrentState.Run.Offset = DateTime.UtcNow - Race.StartedAt;
                        else
                            Model.CurrentState.Run.Offset = -Race.StartDelay;
                    }
                    catch (Exception ex)
                    {

                        SendSystemMessage("Unable to obtain Race information. Try reloading");
                        //Authenticator.AccessToken = null;
                        //Authenticator.RefreshToken = null;

                        goto cleanup;
                    }
                    try
                    {
                        var rf = new StandardComparisonGeneratorsFactory();

                        if (ConnectionError>=0) //don't load after every reconnect
                        {
                            SendSystemMessage("Loading chat history...");
                            ArraySegment<byte> otherBytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes("{ \"action\":\"gethistory\" }"));
                            ws.SendAsync(otherBytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                            await ReceiveAndProcess();

                        }
                    }
                    catch
                    {
                        SendSystemMessage("Unable to load chat history");
                    }
                }

                ConnectionError = -1;

                while (ws.State == WebSocketState.Open && !websocket_cts.IsCancellationRequested)
                {

                    try
                    {
                        await ReceiveAndProcess();

                    }
                    catch (Exception ex)
                    {
                    }
                }


                switch (ws.State)
                {
                    case WebSocketState.CloseSent:
                    case WebSocketState.CloseReceived:
                    case WebSocketState.Closed:
                        ConnectionError = -1;
                        break;
                    default:
                    case WebSocketState.Aborted:
                        if(!websocket_cts.IsCancellationRequested)
                            ConnectionError++;

                        break;
                }
            }

        cleanup:
            IsConnected = false;

            if (ConnectionError >= 0)
            {
                SendSystemMessage($"Auto-reconnect in {reconnectDelays[Math.Min(reconnectDelays.Length-1,ConnectionError)]}s...");
                await Task.Delay(reconnectDelays[Math.Min(reconnectDelays.Length - 1, ConnectionError)] * 1000);
                goto start;
            }
            else
                SendSystemMessage("Disconnect");

            websocket_cts.Dispose();
            
            Disconnected?.Invoke(this, new EventArgs());

        }

        public async void ForceReload()
        {
            if (IsConnected)
            {
                ArraySegment<byte> otherBytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes("{ \"action\":\"gethistory\" }"));
                ws.SendAsync(otherBytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                await ReceiveAndProcess();
            }
        }



        private void UpdateRaceData(RaceMessage msg)
        {

            if (Race == null)
            {
                Race = msg.Race;
                RaceChanged?.Invoke(this, new EventArgs());
                GoalChanged?.Invoke(this, new EventArgs());
                UserListRefreshed?.Invoke(this, new EventArgs());
                return;
            }



            switch (msg.Race?.State)
            {
                case RaceState.Starting:
                    Model.CurrentState.Run.Offset = -msg.Race.StartDelay;
                    Model.Start();
                    break;
                case RaceState.Cancelled:
                    Model.Reset();
                    break;
            }

            if (RacetimeAPI.Instance.Authenticator.Identity == null)
                return;


            if ((Race.State == RaceState.Open || Race.State == RaceState.OpenInviteOnly) && Model.CurrentState.Run.Offset < TimeSpan.Zero && (msg.Race.StartDelay != Race.StartDelay))
            {
                Model.CurrentState.Run.Offset = -msg.Race.StartDelay;
            }

            Race = msg.Race ?? Race;

            var newIdentity = msg.Race.Entrants?.FirstOrDefault(x => x.Name.ToLower() == RacetimeAPI.Instance.Authenticator.Identity.Name?.ToLower());
            switch (newIdentity?.Status)
            {
                case UserStatus.Racing:
                    Model.CurrentState.Run.Offset = DateTime.UtcNow - msg.Race.StartedAt;
                    if (Model.CurrentState.CurrentPhase == TimerPhase.Ended)
                        Model.UndoSplit();
                    if (Model.CurrentState.CurrentPhase == TimerPhase.Paused)
                        Model.Pause();
                    if (Model.CurrentState.CurrentPhase == TimerPhase.NotRunning)
                        Model.Start();

                    break;
                case UserStatus.Disqualified:
                    Model.Reset();
                    break;
                case UserStatus.Finished:
                    Model.Split();
                    break;
                case UserStatus.Forfeit:
                    Model.Reset();
                    break;
            }


            
            RaceChanged?.Invoke(this, new EventArgs());
            StateChanged?.Invoke(this, Race.State);
            UserListRefreshed?.Invoke(this, new EventArgs());
            GoalChanged?.Invoke(this, new EventArgs());

        }

        public IEnumerable<ChatMessage> Parse(dynamic m)
        {
            switch (m.type)
            {
                case "error":
                    yield return RTModelBase.Create<ErrorMessage>(m);
                    break;
                case "race.data":
                    yield return RTModelBase.Create<RaceMessage>(m.race);
                    break;
                case "chat.message":
                    if (m.message.is_system)
                        yield return RTModelBase.Create<RaceBotMessage>(m.message);
                    else
                        yield return RTModelBase.Create<UserMessage>(m.message);
                    break;
                case "chat.history":
                    RequestOutputReset?.Invoke(this, new EventArgs());
                    foreach (var msg in m.messages)
                    {
                        if (msg.user == null)
                            yield return RTModelBase.Create<RaceBotMessage>(msg);
                        else
                            yield return RTModelBase.Create<UserMessage>(msg);
                    }
                    break;
                case "livesplit":
                    yield return RTModelBase.Create<LiveSplitMessage>(m.message);
                    break;
            }
            yield break;
        }



        private void State_OnReset(object sender, TimerPhase value)
        {
            if(PersonalStatus == UserStatus.Racing)
                SendChannelMessage(".forfeit");
        }

        private void State_OnUndoSplit(object sender, EventArgs e)
        {
            if (Model.CurrentState.CurrentSplitIndex == Model.CurrentState.Run.Count - 1)
            {
                 if(PersonalStatus != UserStatus.Racing)
                    SendChannelMessage(".undone");               
            }
        }

        private void State_OnSplit(object sender, EventArgs e)
        {
            if (Model.CurrentState.CurrentSplitIndex >= Model.CurrentState.Run.Count)
                SendChannelMessage(".done");
        }

        public event EventHandler ChannelJoined;
        public event EventHandler Disconnected;
        public event EventHandler GoalChanged;
        public event EventHandler RaceChanged;
        public event EventHandler Kicked;
        public event EventHandler AuthenticationFailed;
        public event EventHandlerT<string> RawMessageReceived;
        public event EventHandlerT<RaceState> StateChanged;
        public event EventHandler UserListRefreshed;
        public event EventHandlerT<IEnumerable<ChatMessage>> MessageReceived;
        public event EventHandler RequestOutputReset;
        public event EventHandler Authorized;




        public async void Connect(string id)
        {
            try
            {
                await RacetimeAPI.Instance.Authenticator.Authorize();
                await RunAsync(id.Split('/')[1]);
            }
            catch (Exception ex)
            {
                IsConnected = false;
                Connect(id);
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
                websocket_cts.Cancel();
            reconnect_cts.Cancel();

            Model.Reset();
            Model.CurrentState.Run.Offset = TimeSpan.Zero;
            Model.OnPause -= Model_OnPause;
            Model.OnSplit -= State_OnSplit;
            Model.OnReset -= State_OnReset;
            Model.OnUndoSplit -= State_OnUndoSplit;


        }

        public void Forfeit()
        {
            if (PersonalStatus == UserStatus.Racing)
            {
                SendChannelMessage(".forfeit");
            }
        }

        public void RemoveRaceComparisons()
        {

        }

        private Regex cmdRegex = new Regex(@"^\.([a-z]+)\s*?(.+)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool TryCreateCommand(ref string message)
        {
            Match m = cmdRegex.Match(message);
            if (m.Success)
            {
                if (m.Groups.Count == 2 && m.Groups[1].Value.Trim().Length > 0)
                {
                    message = "{ \"action\": \"" + m.Groups[1].Value.ToLower().Trim() + "\" }";
                }
                else if (m.Groups.Count == 3)
                {
                    message = "{ \"action\": \"" + m.Groups[1].Value.ToLower().Trim() + "\", \"data\":{ \"" + m.Groups[1].Value.ToLower().Trim() + "\":\"" + m.Groups[2].Value.Trim() + "\" } }";
                }
                else
                {
                    return false;
                }

                return true;
            }
            return false;
        }

        public async void SendChannelMessage(string message)
        {
            message = message.Trim();
            message = message.Replace("\"", "\\\"");
            string data = TryCreateCommand(ref message) ? message : "{ \"action\": \"message\", \"data\": { \"message\":\"" + message + "\", \"guid\":\"" + Guid.NewGuid().ToString() + "\" } }";
            RawMessageReceived?.Invoke(this, data);

            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data));

            if (IsConnected)
            {
                try
                {
                    await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, websocket_cts.Token);
                }
                catch
                {

                }
            }
        }

        public void SendSystemMessage(string message, bool important = false)
        {
            var msg = new ChatMessage[] { LiveSplitMessage.Create(message, important) };
            MessageReceived?.Invoke(this, msg);
            RawMessageReceived?.Invoke(this, msg.First().Posted.ToString());
        }

        public void Ready() => SendChannelMessage(".ready");
        public void Quit() => SendChannelMessage(".quit");
        public void Enter() => SendChannelMessage(".enter");
        public void Unready() => SendChannelMessage(".unready");
        public void Done() => Model.Split();
        public void Undone() => SendChannelMessage(".undone");
    }
}
