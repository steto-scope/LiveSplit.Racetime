using DarkUI.Forms;
using LiveSplit.Racetime.Controller;
using LiveSplit.Racetime.Model;
using LiveSplit.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.Racetime.View
{
    public partial class ChannelForm : DarkForm
    {
        public RacetimeChannel Channel { get; set; }
        
        private bool formClosing;
        //private int reconnectTries = 0;
        //private int maxReconnectTries = 5;

        internal ChannelForm()
        {
            InitializeComponent();
        }

        private static readonly Color[] ColorList = new Color[]
        {
            Color.Aqua,
            Color.Aquamarine,
            Color.Cornsilk,
            Color.Crimson
        };

        public ChannelForm(RacetimeChannel channel, string channelId, bool alwaysOnTop = false)
        {
            Channel = channel;
            Channel.ChannelJoined += Channel_ChannelJoined;
            Channel.StateChanged += Channel_StateChanged;
            Channel.UserListRefreshed += Channel_UserListRefreshed;
            Channel.GoalChanged += Channel_GoalChanged;
            Channel.MessageReceived += Channel_ChatUpdate;
            Channel.Disconnected += Channel_Disconnected;
            Channel.RawMessageReceived += Channel_RawMessageReceived;
            Channel.RequestOutputReset += Channel_RequestOutputReset;

            InitializeComponent();
            TopMost = alwaysOnTop;
            Show();
            Text = channelId.Substring(channelId.IndexOf('/')+1);
            Channel.Connect(channelId);
        }

        private void Channel_RequestOutputReset(object sender, EventArgs e)
        {
            chatBox.Clear();
        }

        private void Channel_RawMessageReceived(object sender, string value)
        {
            Console.WriteLine(value);
        }

        private  async void Channel_Disconnected(object sender, EventArgs e)
        {
            
        }

        private void Channel_ChatUpdate(object sender, IEnumerable<ChatMessage> chatMessages)
        {
            if (formClosing)
                return;

            

            foreach(ChatMessage m in chatMessages)
            {
                Console.WriteLine(m);
                if (m.Type == MessageType.Race)
                    continue;

                chatBox.AppendText("\n");
                chatBox.AppendText(m.Posted.ToString("HH:mm"), Color.ForestGreen);
                chatBox.AppendText("  ");

                Color col;
                if (m.User == null || m.User == RacetimeUser.RaceBot)
                    col = Color.White;
                else
                {
                    col = ColorList[Math.Abs(m.User.Class) % ColorList.Length];
                }
                if (m.IsSystem)
                    col = Color.Red;

                if(m.User!=null && m.User != RacetimeUser.LiveSplit)
                    chatBox.AppendText(m.User.Name, col, true);
                chatBox.AppendText("  ");
                chatBox.AppendText(m.Message);
                chatBox.SelectionStart = chatBox.Text.Length;
                chatBox.ScrollToCaret();
            }
           
        }

        private void Channel_GoalChanged(object sender, EventArgs e)
        {
            
        }

        private void Channel_UserListRefreshed(object sender, EventArgs e)
        {
            userlist.Clear();
            foreach(RacetimeUser u in Channel.Race.Entrants)
            {
                userlist.AddUser(u);
            }
        }

        private void Channel_StateChanged(object sender, RaceState value)
        {
            if (IsDisposed)
                return;

            readyCheckBox.CheckedChanged -= readyCheckBox_CheckedChanged;
            switch(Channel.PersonalStatus)
            {
                case UserStatus.Unknown:
                    actionButton.Enabled = true;
                    actionButton.Text = "Enter Race";
                    readyCheckBox.Enabled = true;
                    readyCheckBox.Checked = false;
                    break;
                case UserStatus.NotReady:
                    actionButton.Enabled = true;
                    actionButton.Text = "Quit Race";
                    readyCheckBox.Enabled = true;
                    readyCheckBox.Checked = false;
                    break;
                case UserStatus.Ready:
                    actionButton.Enabled = true;
                    actionButton.Text = "Quit Race";
                    readyCheckBox.Enabled = true;
                    readyCheckBox.Checked = true;
                    break;
                case UserStatus.Racing:
                    actionButton.Enabled = true;
                    actionButton.Text = "Forfeit Race";
                    readyCheckBox.Enabled = false;
                    readyCheckBox.Checked = true;
                    break;
                case UserStatus.Finished:
                    actionButton.Enabled = true;
                    actionButton.Text = "Undone";
                    readyCheckBox.Enabled = false;
                    readyCheckBox.Checked = true;
                    break;
                case UserStatus.Disqualified:
                case UserStatus.Forfeit:
                default:
                    actionButton.Enabled = false;
                    actionButton.Text = "";
                    readyCheckBox.Enabled = false;
                    readyCheckBox.Checked = false;
                    break;
            }
            readyCheckBox.CheckedChanged += readyCheckBox_CheckedChanged;

            inputBox.Enabled = !(!Channel.Race.AllowNonEntrantChat && Channel.PersonalStatus == UserStatus.NotInRace) &&
                                !(Channel.Race.State == RaceState.Started && !Channel.Race.AllowMidraceChat);


        }

        private void actionButton_Click(object sender, EventArgs e)
        {
            actionButton.Enabled = false;
            switch(Channel.PersonalStatus)
            {
                default:
                case UserStatus.NotInRace:
                    Channel.SendChannelMessage(".enter");
                    break;
                case UserStatus.NotReady:
                case UserStatus.Ready:
                    Channel.SendChannelMessage(".quit");
                    break;
                /*case UserStatus.Ready:
                    Channel.SendChannelMessage(".quit");
                    break;*/
                case UserStatus.Racing:
                    Channel.SendChannelMessage(".forfeit");
                    break;
                case UserStatus.Finished:
                    Channel.SendChannelMessage(".undone");
                    break;
            }
        }

        private void Channel_ChannelJoined(object sender, EventArgs e)
        {
            
        }

        private void ChannelWindow_Load(object sender, EventArgs e)
        {

        }

        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if((e.KeyData == Keys.Return || e.KeyData == Keys.Enter) && !string.IsNullOrEmpty(inputBox.Text))
            {
                Channel.SendChannelMessage(inputBox.Text);
                inputBox.Text = "";
                inputBox.SelectionStart = 0;
                inputBox.SelectionLength = 0;
            }
        }

        private void readyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if(sender == this || sender == readyCheckBox)
            {
                if (readyCheckBox.Checked)
                    Channel.Ready();
                else
                    Channel.Unready();               
            }
        }

        private void ChannelForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            formClosing = true;
            Channel.ChannelJoined -= Channel_ChannelJoined;
            Channel.StateChanged -= Channel_StateChanged;
            Channel.UserListRefreshed -= Channel_UserListRefreshed;
            Channel.GoalChanged -= Channel_GoalChanged;
            Channel.MessageReceived -= Channel_ChatUpdate;
            Channel.Disconnected -= Channel_Disconnected;

            Channel.Disconnect();
        }

        private void ChannelForm_FormClosed(object sender, FormClosedEventArgs e)
        {
           
        }

        private void readyCheckBox_Click(object sender, EventArgs e)
        {
            readyCheckBox.Enabled = false;
        }

        private void saveLogButton_Click(object sender, EventArgs e)
        {
            string s = inputBox.Text;
            Channel.TryCreateCommand(ref s);
            Channel.SendChannelMessage(s);
        }
    }       
}
