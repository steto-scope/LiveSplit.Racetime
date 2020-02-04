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
        public const string restProto = "http";
        public const string wsProto = "ws";
        public const string serverDomain = "192.168.178.70";
        public const int serverPort = 8000;

        public string FullWebRoot => string.Format("{0}://{1}:{2}/", restProto, serverDomain, serverPort);
        public string FullSocketRoot => string.Format("{0}://{1}:{2}/", wsProto, serverDomain, serverPort);
        public Uri SocketUri => Race == null ? null : new Uri(FullSocketRoot + "ws/race/" + Race.ID);

        public Race Race { get; set; }
        protected ITimerModel Model { get; set; }
        private RacetimeAuthenticator Authenticator { get; set; }
        private ClientWebSocket ws;

        CancellationTokenSource cts;

        public RacetimeChannel(LiveSplitState state, ITimerModel model)
        {
            Authenticator = new RacetimeAuthenticator();
            this.Model = model;

            state.OnSplit += State_OnSplit;
            state.OnUndoSplit += State_OnUndoSplit;
            state.OnReset += State_OnReset;
        }

        public async Task RunAsync()
        {
            cts = new CancellationTokenSource();
            ws = new ClientWebSocket();
            await Authenticator.Authorize();
           await Authenticator.RequestAccessToken();

            if(Authenticator.AccessToken == null)
            {
                AuthenticationFailed?.Invoke(this, new EventArgs());
                return;
            }

            

            ws.Options.SetRequestHeader("Authorization", $"Bearer {Authenticator.AccessToken}");
            
            await ws.ConnectAsync(SocketUri, cts.Token);
            GoalChanged?.Invoke(this, null);

            
            //initial command to sync LiveSplit 
            if (ws.State == WebSocketState.Open)
            {
                ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes("{ \"action\":\"getrace\" }"));
                ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            

            while (ws.State == WebSocketState.Open && !cts.IsCancellationRequested)
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
                    WebSocketReceiveResult result = await ws.ReceiveAsync(bytesReceived, cts.Token);
                    string msg = Encoding.UTF8.GetString(bytesReceived.Array, 0, result.Count);
                    RawMessageReceived?.Invoke(this, msg);

                    var chatmessage = ChatMessage.Deserialize((msg), Race?.Entrants);
                    if(chatmessage != null)
                        MessageReceived?.Invoke(this, chatmessage);
                    
                    
                }
                catch(Exception ex)
                {

                }
            }

            switch(ws.State)
            {
                case WebSocketState.CloseReceived:
                    MessageReceived?.Invoke(this, ChatMessage.Deserialize("{ \"type\":\"error\", \"errors\":[\"Server closed the cconnection\"] }", Race?.Entrants));
                    break;
                case WebSocketState.Aborted:
                    MessageReceived?.Invoke(this, ChatMessage.Deserialize("{ \"type\":\"error\", \"errors\":[\"Connection aborted\"] }", Race?.Entrants));
                    break;
                case WebSocketState.Closed:
                    MessageReceived?.Invoke(this, ChatMessage.Deserialize("{ \"type\":\"error\", \"errors\":[\"Connection closed\"] }", Race?.Entrants));
                    break;
            }
            
        }

        
        /*
        private SRLIRCRights DetermineRights(string flair)
        {
            switch(flair)
            {
                case "moderator":
                case "staff":
                    return SRLIRCRights.Operator;
                case ""
            }
        }/*
        *//*
        private Tuple<string, SRLIRCUser, string> GenerateChatMessage(string msg)
        {
            string user;
            string time;
            dynamic obj = JSON.FromString(msg);

            switch(obj?.type)
            {
                case "chat.message":
                    user = obj.message?.user?.name?.ToString();
                    time = DateTime.Parse(obj.message?.posted_at?.ToString()).ToString("HH:mm ");
                    return new Tuple<string, SRLIRCUser, string>(Race.ID, user==null ? SRLIRCUser.RaceBot : new SRLIRCUser(user), obj.message.message);
                case "error":
                    return new Tuple<string, SRLIRCUser, string>(Race.ID, SRLIRCUser.System, obj.errors[0]);
                case "race.data":
                    UpdateRaceData(JSON.FromString(msg).race);
                    
                    
                    
                    break;
            }

            return null;
        }*/

            /*
        private void UpdateRaceData(dynamic msg)
        {
            Race.Goal = msg.goal?.name ?? "No Goal set";
            GoalChanged?.Invoke(this, new EventArgs());

            Race.Entrants.Clear();
            foreach(var e in msg.entrants)
            {
                Race.Entrants.Add(SRLIRCUser.Deserialize(e));
            }
            UserListRefreshed?.Invoke(this, new EventArgs());

            switch(msg.status.value)
            {
                case "invitational":
                case "pending":
                case "open":
                default:
                    Race.State = RaceState.NotInRace;
                    break;
                case "in_progress":
                    Race.State = RaceState.RaceStarted;
                    break;
                case "finished":
                case "cancelled":
                    Race.State = RaceState.RaceEnded;
                    break;
            }
            StateChanged?.Invoke(this, Race.State);

        }

        */

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
        public event EventHandlerT<string> ChatUpdate;
        protected event EventHandlerT<IEnumerable<ChatMessage>> MessageReceived;




        public async void Connect()
        {
            await RunAsync();
        }

        public void Disconnect()
        {
            cts.Cancel();
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
                var command = message.Substring(1, message.IndexOf(' ') - 1).TrimEnd().ToLower();
                var parameter = message.Substring(message.IndexOf(' ')).TrimStart();
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
                await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, cts.Token);
            }
        }
        

    }
}
