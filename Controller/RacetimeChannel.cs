using System;
using System.Collections.Generic;
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
        


        CancellationTokenSource websocket_cts;
        CancellationTokenSource reconnect_cts;

        public RacetimeChannel(LiveSplitState state, ITimerModel model)
        {
            reconnect_cts = new CancellationTokenSource();
            RunPeriodically(() => Reconnect(), new TimeSpan(0, 0, 3), reconnect_cts.Token);

            Authenticator = new RacetimeAuthenticator();
            this.Model = model;
            
            state.OnSplit += State_OnSplit;
            state.OnUndoSplit += State_OnUndoSplit;
            state.OnReset += State_OnReset;
        }

        private void Reconnect()
        {
            if(ConnectionError && Race!=null)
                Connect(Race.ID);
        }

        private async Task<bool> ReceiveAndProcess()
        {
            ArraySegment<byte> bytesReceived = new ArraySegment<byte>(new byte[1024 * 100]);
            string msg = "";
            try
            {
                WebSocketReceiveResult result = await ws.ReceiveAsync(bytesReceived, websocket_cts.Token);
                if (result == null)
                    return false;

                msg = Encoding.UTF8.GetString(bytesReceived.Array, 0, result.Count);
                RawMessageReceived?.Invoke(this, msg);
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
            websocket_cts = new CancellationTokenSource();
            ws = new ClientWebSocket();

            //authorize user
            if (Authenticator.AccessToken != null)
                goto connect;


            if(await Authenticator.Authorize() && await Authenticator.RequestAccessToken() && Authenticator.AccessToken != null)
            {
                Authenticator.RequestUserInfo();
                SendSystemMessage($"Authorization successful. Hello, {Authenticator.Identity?.Name}");
                
            }
            else
            {
                AuthenticationFailed?.Invoke(this, new EventArgs());
                return;
            }
            Authenticator.Finalize();

connect:
            //opening the socket
            ws.Options.SetRequestHeader("Authorization", $"Bearer {Authenticator.AccessToken}");
            try
            {
                await ws.ConnectAsync(new Uri(FullSocketRoot + "ws/o/race/" + id), websocket_cts.Token);
            }
            catch(WebSocketException wex)
            {
                Console.WriteLine(wex.Message);
                Console.WriteLine(wex.InnerException.Message);
                Console.WriteLine(wex.StackTrace);
                //SendSystemMessage(wex.Message);
            }

            //initial command to sync LiveSplit 
            if (ws.State == WebSocketState.Open)
            {
                ConnectionError = false;
                ChannelJoined?.Invoke(this, new EventArgs());
                SendSystemMessage($"Joined Channel '{id}'");
                ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes("{ \"action\":\"getrace\" }"));
                ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                await ReceiveAndProcess();
                SendSystemMessage("Loading chat history...");
                ArraySegment<byte> otherBytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes("{ \"action\":\"gethistory\" }"));
                ws.SendAsync(otherBytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                await ReceiveAndProcess();
            }

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
                catch(Exception ex)
                {
                    //Console.WriteLine(ex.InnerException.Message);
                }
            }
            

            switch (ws.State)
            {
                case WebSocketState.CloseReceived:
                    SendSystemMessage("Disconnect");
                    ConnectionError = false;
                    Disconnected?.Invoke(this, new EventArgs());
                    break;
                default:
                case WebSocketState.Aborted:
                    SendSystemMessage("Connection lost");
                    SendSystemMessage("Reconnecting...");
                    ConnectionError = true;

                    break;
                case WebSocketState.Closed:
                    SendSystemMessage("Connection closed");
                    SendSystemMessage("Reconnecting...");
                    ConnectionError = true;

                    break;
            }


        }

        public async void ForceReload()
        {
            ArraySegment<byte> otherBytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes("{ \"action\":\"gethistory\" }"));
            ws.SendAsync(otherBytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
            await ReceiveAndProcess();
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
                        Model.CurrentState.Run.Offset = new TimeSpan(0,0,-10);//msg.Race.StartDelay;
                        Console.WriteLine(msg.Race.StartDelay);
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
                    Model.CurrentState.Run.Offset = DateTime.Now - msg.Race.StartedAt;
                    Model.UndoSplit();
                    Model.Start();
                    break;
                case UserStatus.Disqualified:
                    Model.Reset();
                    break;
                case UserStatus.Finished:                    
                    Model.Split();
                    Model.CurrentState.Run.Offset = newIdentity.FinishTime;
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
            
        }

        private void State_OnSplit(object sender, EventArgs e)
        {
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
           
            websocket_cts.Cancel();
            reconnect_cts.Cancel();
            ws.Dispose();
            Console.WriteLine("Disconnect");
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

            if (ws.State == WebSocketState.Open)
            {               
                await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, websocket_cts.Token);
            }
        }

        public void SendSystemMessage(string message, bool important = false)
        {
            MessageReceived?.Invoke(this,  new ChatMessage[] { LiveSplitMessage.Create(message, important) });
        }

        public void Ready() => SendChannelMessage(".ready");
        public void Unready() => SendChannelMessage(".unready");
    }
}
