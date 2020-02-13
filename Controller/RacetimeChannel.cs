using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiveSplit.Model;
using LiveSplit.Model.Input;
using LiveSplit.Options;
using LiveSplit.Racetime.Model;
using LiveSplit.Web;
using LiveSplit.Web.SRL;

namespace LiveSplit.Racetime.Controller
{
    public class RacetimeChannel
    {
        public static async Task RunPeriodically(Action action, TimeSpan period, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(period, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                    action();
            }
        }

        public const string restProto = "http";
        public const string wsProto = "ws";
        public const string serverDomain = "192.168.178.70";
        public const int bufferSize = 20480;
        public const int maxBufferSize = 2097152;
        public const int serverPort = 8000;

        public string FullWebRoot => string.Format("{0}://{1}:{2}/", restProto, serverDomain, serverPort);
        public string FullSocketRoot => string.Format("{0}://{1}:{2}/", wsProto, serverDomain, serverPort);

        public Race Race { get; set; }
        public UserStatus PersonalStatus
        {
            get
            {
                //Console.WriteLine(Race.Entrants.First().Name);
                //Console.WriteLine(Authenticator.Identity?.Name.ToLower());
                var u = Race?.Entrants?.FirstOrDefault(x => x.Name.ToLower() == Authenticator.Identity?.Name.ToLower());
                if (u == null)
                    return UserStatus.Unknown;
                return u.Status;
            }
        }
        protected ITimerModel Model { get; set; }
        private RacetimeAuthenticator Authenticator { get; set; }
        private ClientWebSocket ws;
        protected List<ChatMessage> log = new List<ChatMessage>();
        public bool ConnectionError { get; set; }
        public bool IsConnected { get; set; }
        


        CancellationTokenSource websocket_cts;
        CancellationTokenSource reconnect_cts;

        public RacetimeChannel(LiveSplitState state, ITimerModel model)
        {
            Authenticator = new RacetimeAuthenticator(new RTAuthentificationSettings());
            reconnect_cts = new CancellationTokenSource();
            RunPeriodically(() => Reconnect(), new TimeSpan(0, 0, 10), reconnect_cts.Token);

           
            this.Model = model;
            
            state.OnSplit += State_OnSplit;
            state.OnUndoSplit += State_OnUndoSplit;
            state.OnReset += State_OnReset;
        }

        private void Reconnect()
        {
            if(ConnectionError && Race!=null)
                Connect(Race.Id);
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
                    read += result.Count;
                    free -= result.Count;
                }
                while (!result.EndOfMessage);
                

                msg = Encoding.UTF8.GetString(buf, 0, read);
                RawMessageReceived?.Invoke(this, msg);
            }
            catch(InternalBufferOverflowException)
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


            IEnumerable<ChatMessage> chatmessages = Parse(JSON.FromString(msg));


            ChatMessage racemessage = chatmessages.FirstOrDefault(x => x.Type == MessageType.Race);
            if (racemessage != null)
            {
                UpdateRaceData((RaceMessage)racemessage);
            }
            MessageReceived?.Invoke(this, chatmessages);
            return true;
        }


        public async Task RunAsync(string id)
        {
            if(IsConnected)
            {
                SendSystemMessage("WebSocket is already open");
                return;
            }
            websocket_cts = new CancellationTokenSource();
            

            using (ws = new ClientWebSocket())
            {
                IsConnected = true;

                //authorize user if needed
                if (Authenticator.AccessToken == null)
                {
                    if (await Authenticator.Authorize())
                    {
                        Console.WriteLine(Authenticator.Identity);
                        SendSystemMessage($"Authorization successful. Hello, {Authenticator.Identity?.Name}");
                    }
                    else
                    {
                        SendSystemMessage(Authenticator.Error, true);
                        AuthenticationFailed?.Invoke(this, new EventArgs());
                        goto cleanup;
                    }
                }
                //opening the socket
                ws.Options.SetRequestHeader("Authorization", $"Bearer {Authenticator.AccessToken}");
                try
                {
                    await ws.ConnectAsync(new Uri(FullSocketRoot + "ws/o/race/" + id), websocket_cts.Token);
                }
                catch (WebSocketException wex)
                {
                    Console.WriteLine(wex.Message);
                    Console.WriteLine(wex.InnerException.Message);
                    Console.WriteLine(wex.StackTrace);
                    //SendSystemMessage(wex.Message);
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
                    catch
                    {
                        SendSystemMessage("Unable to obtain Race information. Try reloading");
                        goto cleanup;
                    }
                    try
                    {
                        if (!ConnectionError) //don't load after every reconnect
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

                ConnectionError = false;

                while (ws.State == WebSocketState.Open && !websocket_cts.IsCancellationRequested)
                {

                    try
                    {
                        /* Console.Write("Input message ('exit' to exit): ");
                        string msg = Console.ReadLine();
                        if (msg == "exit")
                        {
                            break;
                        }*/
                        //ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
                        //await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                        await ReceiveAndProcess();

                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(ex.InnerException.Message);
                    }
                }


                switch (ws.State)
                {
                    case WebSocketState.CloseSent:
                    case WebSocketState.CloseReceived:
                    case WebSocketState.Closed:
                        ConnectionError = false;                       
                        break;
                    default:
                    case WebSocketState.Aborted:
                        ConnectionError = true;

                        break;
                    /*
                        SendSystemMessage("Connection closed");
                        SendSystemMessage("Reconnecting...");
                        ConnectionError = true;

                        break;*/
                }
            }

cleanup:
            SendSystemMessage("Disconnect");
            if(ConnectionError)
                 SendSystemMessage("Reconnecting...");
            websocket_cts.Dispose();
            IsConnected = false;
            Disconnected?.Invoke(this, new EventArgs());            

        }

        public async void ForceReload()
        {            
            if(IsConnected)
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
                GoalChanged?.Invoke(this, new EventArgs());
                UserListRefreshed?.Invoke(this, new EventArgs());
                return;
            }




            if (msg.Race.State != Race.State)
            {
                switch (msg.Race.State)
                {
                    case Racetime.Model.RaceState.Starting:
                        Model.CurrentState.SetGameTime(TimeSpan.Zero);
                        Model.CurrentState.Run.Offset = DateTime.UtcNow - msg.Race.StartedAt;// - msg.Race.StartDelay;//-msg.Race.StartDelay;// new TimeSpan(0,0,-10);//msg.Race.StartDelay;
                        Console.WriteLine("Start Delay: {0}, Date: {1}, Posted: {2}", msg.Race.StartDelay, msg.Race.StartedAt, msg.Posted);
                        Model.Start();
                        break;
                    /*case Racetime.Model.RaceState.Started:
                        Model.CurrentState.SetGameTime(DateTime.Now - msg.Race.StartedAt);
                        Model.Start();
                        break;*/
                    case Racetime.Model.RaceState.Cancelled:
                        Model.Reset();
                        break;
                }
            }

            var newIdentity = msg.Race.Entrants?.FirstOrDefault(x => x.Name.ToLower() == Authenticator.Identity.Name.ToLower());
            //Console.WriteLine(string.Join(" ",msg.Race.Entrants?.Select(x=>x.Name)));
            switch(newIdentity?.Status)
            {
                case UserStatus.Racing:
                    Model.CurrentState.Run.Offset = DateTime.UtcNow - msg.Race.StartedAt;
                    
                    Model.UndoSplit();
                    Model.Start();
                    break;
                case UserStatus.Disqualified:
                    Model.Reset();
                    break;
                case UserStatus.Finished:                    
                    Model.Split();
                    Model.CurrentState.Run.Offset = newIdentity.FinishedAt - msg.Race.StartedAt;
                    break;
                case UserStatus.Forfeit:
                    Model.Reset();
                    break;
            }

            Race = msg.Race;
            StateChanged?.Invoke(this, Race.State);
            UserListRefreshed?.Invoke(this, new EventArgs());
            GoalChanged?.Invoke(this, new EventArgs());

        }

        public IEnumerable<ChatMessage> Parse(dynamic m)
        {
           // Console.WriteLine(m.GetType().ToString() + m.ToString());
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
                        //Console.WriteLine(msg);
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
           
        }

        private void State_OnUndoSplit(object sender, EventArgs e)
        {
            //todo: dosent work yet
            if (Model.CurrentState.CurrentSplitIndex >= Model.CurrentState.Run.Count) 
                SendChannelMessage(".undone");
        }

        private void State_OnSplit(object sender, EventArgs e)
        {
            if(Model.CurrentState.CurrentSplitIndex >= Model.CurrentState.Run.Count)
                SendChannelMessage(".done");
        }

        public event EventHandler ChannelJoined;
        public event EventHandler Disconnected;
        public event EventHandler GoalChanged;
        public event EventHandler Kicked;
        public event EventHandler AuthenticationFailed;
        public event EventHandlerT<string> RawMessageReceived;
        public event EventHandlerT<Model.RaceState> StateChanged;
        public event EventHandler UserListRefreshed;
        public event EventHandlerT<IEnumerable<ChatMessage>> MessageReceived;
        public event EventHandler RequestOutputReset;




        public async void Connect(string id)
        {
            await RunAsync(id.Split('/')[1]);
        }

        public void Disconnect()
        {
           if(IsConnected)
                websocket_cts.Cancel();
            reconnect_cts.Cancel();
        }

        public void Forfeit()
        {
            SendChannelMessage(".forfeit");
            Model.Reset();
        }
        
        public void RemoveRaceComparisons()
        {
            Console.WriteLine("Remove Race Comparisons");
        }

        public bool TryCreateCommand(ref string message)
        {
            if (message.StartsWith("."))
            {
                int end = 1;
                end = message.IndexOf(' ') <= 0 ? message.Length - 1 : message.IndexOf(' ') - 1;

                var command = message.Substring(1, end).TrimEnd().ToLower();
                //var parameter = message.Substring(message.IndexOf(' ')).TrimStart();
                message = "{ \"action\": \""+command+"\" }";
                return true;
            }
            return false;
        }

        public async void SendChannelMessage(string message)
        {
            message = message.Trim();
            string data = TryCreateCommand(ref message) ? message : "{ \"action\": \"message\", \"data\": { \"message\":\"" + message + "\", \"guid\":\"" + Guid.NewGuid().ToString() + "\" } }";
            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data));

            if (IsConnected)
            {               
                await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, websocket_cts.Token);
            }
        }

        public void SendSystemMessage(string message, bool important = false)
        {
            var msg = new ChatMessage[] { LiveSplitMessage.Create(message, important) };
            MessageReceived?.Invoke(this, msg );
            RawMessageReceived?.Invoke(this, msg.First().Posted.ToString());
        }

        public void Ready() => SendChannelMessage(".ready");
        public void Unready() => SendChannelMessage(".unready");
    }
}
