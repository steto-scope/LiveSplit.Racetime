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
                return GetPersonalStatus(Race);
            }
        }
                
        protected ITimerModel Model { get; set; }

        private ClientWebSocket ws;
        protected List<ChatMessage> log = new List<ChatMessage>();
        public int ConnectionError { get; set; }
        public bool IsConnected { get; set; }
        public RacetimeSettings Settings { get; set; }



        CancellationTokenSource websocket_cts;
        CancellationTokenSource reconnect_cts;

        public RacetimeChannel(LiveSplitState state, ITimerModel model, RacetimeSettings settings)
        {
            Settings = settings;
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

        protected UserStatus GetPersonalStatus(Race race)
        {
            var u = race?.Entrants?.FirstOrDefault(x => x.Name.ToLower() == RacetimeAPI.Instance.Authenticator.Identity?.Name.ToLower());
            if (u == null)
                return UserStatus.Unknown;
            return u.Status;
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
                    result = await ws?.ReceiveAsync(new ArraySegment<byte>(buf, read, free), websocket_cts?.Token ?? CancellationToken.None);
                    if (websocket_cts?.IsCancellationRequested ?? true)
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
                while (!(result = await ws?.ReceiveAsync(new ArraySegment<byte>(buf, 0, buf.Length), websocket_cts?.Token ?? CancellationToken.None)).EndOfMessage)
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

                AuthResult r = await Authenticator.Authorize();
                switch(r)
                {
                    case AuthResult.Success:
                        SendSystemMessage($"Authorization successful. Hello, {Authenticator.Identity?.Name}");
                        Authorized?.Invoke(this, null);
                        break;
                    case AuthResult.Failure:
                        SendSystemMessage(Authenticator.Error, true);
                        AuthenticationFailed?.Invoke(this, new EventArgs());
                        ConnectionError++;
                        goto cleanup;
                    case AuthResult.Cancelled:
                        SendSystemMessage($"Authorization declined by user.");
                        ConnectionError = -1;
                        goto cleanup;
                    case AuthResult.Pending:
                        Authenticator.StopPendingAuthRequest();
                        IsConnected = false;
                        goto start;
                    case AuthResult.Stale:
                        goto cleanup_silent;

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

                    }
                    catch (Exception ex)
                    {
                        SendSystemMessage("Unable to obtain Race information. Try reloading");
                        goto cleanup;
                    }
                    try
                    {
                        var rf = new StandardComparisonGeneratorsFactory();

                        if (ConnectionError>=0 && Settings.LoadChatHistory) //don't load after every reconnect
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
                        if(!(websocket_cts?.IsCancellationRequested ?? true))
                            ConnectionError++;

                        break;
                }
            }
            ws = null;

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

        cleanup_silent:
            websocket_cts?.Dispose();
            websocket_cts = null;
            Disconnected?.Invoke(this, new EventArgs());
        }

        public async void ForceReload()
        {
            if (IsConnected && ws != null)
            {
                ArraySegment<byte> otherBytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes("{ \"action\":\"gethistory\" }"));
                ws.SendAsync(otherBytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                await ReceiveAndProcess();
            }
        }



        private void UpdateRaceData(RaceMessage msg)
        {
            //safety check, this shouldn't happen
            if (RacetimeAPI.Instance.Authenticator.Identity == null)
                return;

            //ignore double tap prevention when updating race data
            var m = Model;
            if (m is DoubleTapPrevention)
                m = ((DoubleTapPrevention)Model).InternalModel;


            RaceState r = Race?.State ?? RaceState.Unknown;
            RaceState nr = msg.Race?.State ?? RaceState.Unknown;
            UserStatus u = GetPersonalStatus(Race);
            UserStatus nu = GetPersonalStatus(msg.Race);

            if (msg.Race != null)
                Race = msg.Race;

            //update only neccessary if the state of the player and/or the race has changed
            if ((r != nr) || (u != nu))
            {
                //we are (now) part of the race
                if (nu != UserStatus.NotInRace && nu != UserStatus.Unknown)
                {
                    //the race is starting
                    if ((r == RaceState.Open || r == RaceState.OpenInviteOnly) && nr == RaceState.Starting)
                    {
                        m.CurrentState.Run.Offset = DateTime.UtcNow - msg.Race.StartedAt;
                        m.Reset();
                        m.Start();
                    }

                    //the race is already running and we're not finished, sync the timer
                    if(nr == RaceState.Started && nu == UserStatus.Racing)
                    {
                        m.CurrentState.Run.Offset = DateTime.UtcNow - msg.Race.StartedAt;
                        if (m.CurrentState.CurrentPhase == TimerPhase.Ended)
                            m.UndoSplit();
                        if (m.CurrentState.CurrentPhase == TimerPhase.Paused)
                            m.Pause();
                        if (m.CurrentState.CurrentPhase == TimerPhase.NotRunning)
                            m.Start();
                    }

                    if(u != nu && nu == UserStatus.Finished)
                    {
                        m.Split();
                    }

                    if (u != nu && nu == UserStatus.Forfeit)
                    {
                        m.Reset();
                    }
                    if (u != nu && nu == UserStatus.Disqualified)
                    {
                        m.Reset();
                    }

                    if (r != nr && nr == RaceState.Cancelled)
                    {
                        m.Reset();
                    }                    
                }                      
            }

            StateChanged?.Invoke(this, nr);
            RaceChanged?.Invoke(this, new EventArgs());
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
                    if (m.message.is_system != null && m.message.is_system)
                        yield return RTModelBase.Create<SystemMessage>(m.message);
                    else
                    {
                        bool isBot = false;
                        try
                        {
                            isBot = m.message.is_bot;
                        }
                        catch
                        {
                            isBot = false;
                        }
                        if(isBot)
                            yield return RTModelBase.Create<BotMessage>(m.message);
                        else
                            yield return RTModelBase.Create<UserMessage>(m.message);
                    }
                    break;
                case "chat.history":
                    RequestOutputReset?.Invoke(this, new EventArgs());
                    foreach (var msg in m.messages)
                    {
                        if (msg.is_system != null && msg.is_system)
                            yield return RTModelBase.Create<SystemMessage>(msg);
                        else
                        {
                            bool isBot = false;
                            try
                            {
                                isBot = msg.is_bot;
                            }
                            catch
                            {
                                isBot = false;
                            }
                            if (isBot)
                                yield return RTModelBase.Create<BotMessage>(msg);
                            else
                                yield return RTModelBase.Create<UserMessage>(msg);
                        }
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
            if (Model.CurrentState.CurrentSplitIndex >= Model.CurrentState.Run.Count && PersonalStatus == UserStatus.Racing)
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
            {                                
                websocket_cts?.Cancel();
                websocket_cts = null;
            }
            reconnect_cts?.Cancel();
            reconnect_cts = null;

            Model.OnPause -= Model_OnPause;
            Model.OnSplit -= State_OnSplit;
            Model.OnReset -= State_OnReset;
            Model.OnUndoSplit -= State_OnUndoSplit;


        }

        public void Forfeit()
        {
            if (PersonalStatus == UserStatus.Racing)
            {
                Model.Reset();
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

            if (IsConnected && ws != null)
            {
                try
                {
                    if(websocket_cts != null)
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
