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
                var u = Race?.Entrants?.FirstOrDefault(x => x.Name == Authenticator.Identity?.Name);
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
        


        CancellationTokenSource wscts;
        CancellationTokenSource rccts;

        public RacetimeChannel(LiveSplitState state, ITimerModel model)
        {
            rccts = new CancellationTokenSource();
            RunPeriodically(() => Reconnect(), new TimeSpan(0, 0, 3), rccts.Token);

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


        public async Task RunAsync(string id)
        {
            wscts = new CancellationTokenSource();
            ws = new ClientWebSocket();

            //authorize user
            if (Authenticator.AccessToken != null)
                goto connect;
            
            await Authenticator.Authorize();
            await Authenticator.RequestAccessToken();
            

            if(Authenticator.AccessToken == null)
            {
                AuthenticationFailed?.Invoke(this, new EventArgs());
                return;
            }
            else
            {
                Authenticator.RequestUserInfo();
                SendSystemMessage($"Authorization successful. Hello, {Authenticator.Identity?.Name}");
            }

connect:
            //opening the socket
            ws.Options.SetRequestHeader("Authorization", $"Bearer {Authenticator.AccessToken}");
            try
            {
                await ws.ConnectAsync(new Uri(FullSocketRoot + "ws/race/" + id), wscts.Token);
            }
            catch(WebSocketException wex)
            {
                //SendSystemMessage(wex.Message);
                goto cleanup;
            }

            //initial command to sync LiveSplit 
            if (ws.State == WebSocketState.Open)
            {
                ConnectionError = false;
                ChannelJoined?.Invoke(this, new EventArgs());
                SendSystemMessage($"Joined Channel '{id}'");
                ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes("{ \"action\":\"getrace\" }"));
                ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
            }



            while (ws.State == WebSocketState.Open && !wscts.IsCancellationRequested)
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
                    ArraySegment<byte> bytesReceived = new ArraySegment<byte>(new byte[1024 * 10]);
                    WebSocketReceiveResult result = await ws.ReceiveAsync(bytesReceived, wscts.Token);
                    if (result == null)
                        continue;
                    string msg = Encoding.UTF8.GetString(bytesReceived.Array, 0, result.Count);
                    RawMessageReceived?.Invoke(this, msg);

                    Console.WriteLine(msg);
                    
                    IEnumerable<ChatMessage> chatmessages = ChatMessage.Deserialize(JSON.FromString(msg), Race?.Entrants);
                   
                    MessageReceived?.Invoke(this, chatmessages);
                    ChatMessage racemessage = chatmessages.FirstOrDefault(x => x.Type == MessageType.Race);
                    
                    if(racemessage!= null)
                    {
                        UpdateRaceData(racemessage);
                    }
                    
                    
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            

            switch (ws.State)
            {
                case WebSocketState.CloseReceived:
                    SendSystemMessage("Server closed the connection");
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

cleanup:
            ws.Dispose();
            wscts.Dispose();

        }

        
            
        private void UpdateRaceData(ChatMessage msg)
        {
            if (Race == null)
            {
                Race = msg.Race;
                GoalChanged?.Invoke(this, new EventArgs());
                UserListRefreshed?.Invoke(this, new EventArgs());
                return;
            }

            if(Race.Goal != msg.Race.Goal)
            {
                Race.Goal = msg.Race.Goal ?? "No Goal set";
                GoalChanged?.Invoke(this, new EventArgs());
            }         

            Race.Entrants.Clear();
            foreach(var e in msg.Race.Entrants)
            {
                Race.Entrants.Add(e);
            }
            UserListRefreshed?.Invoke(this, new EventArgs());

            Race.State = msg.Race.State;
            StateChanged?.Invoke(this, Race.State);

        }

        

        private void State_OnReset(object sender, TimerPhase value)
        {
            Console.WriteLine("Timer Reset");
        }

        private void State_OnUndoSplit(object sender, EventArgs e)
        {
            Console.WriteLine("Undo Split");
        }

        private void State_OnSplit(object sender, EventArgs e)
        {
            Console.WriteLine("Split");
        }

        public event EventHandler ChannelJoined;
        public event EventHandler Disconnected;
        public event EventHandler GoalChanged;
        public event EventHandler Kicked;
        public event EventHandler AuthenticationFailed;
        protected event EventHandlerT<string> RawMessageReceived;
        public event EventHandlerT<Model.RaceState> StateChanged;
        public event EventHandler UserListRefreshed;
        public event EventHandlerT<IEnumerable<ChatMessage>> MessageReceived;




        public async void Connect(string id)
        {
            await RunAsync(id.Split('/')[1]);
        }

        public void Disconnect()
        {
            wscts.Cancel();
            rccts.Cancel();
            Console.WriteLine("Disconnect");
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
                await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, wscts.Token);
            }
        }

        public void SendSystemMessage(string message)
        {
            MessageReceived?.Invoke(this, ChatMessage.Deserialize(JSON.FromString("{ \"type\":\"livesplit\", \"message\":\""+message+"\" }"), Race?.Entrants));
        }

        public void Ready() => SendChannelMessage(".ready");
        public void Unready() => SendChannelMessage(".unready");
    }
}
