using DarkUI.Forms;
using LiveSplit.Options;
using LiveSplit.Racetime.Controller;
using LiveSplit.Racetime.Model;
using LiveSplit.Web;
using LiveSplit.Web.Share;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.Racetime.View
{
    public partial class ChannelForm : DarkForm
    {
        public RacetimeChannel Channel { get; set; }
        public string ChannelID { get; set; }
        private static Regex urlPattern = new Regex(@"\b((ftp|https?):\/\/[-\w]+(\.\w[-\w]*)+|(?:[a-z0-9](?:[-a-z0-9]*[a-z0-9])?\.)+(?: com\b|edu\b|biz\b|gov\b|in(?:t|fo)\b|mil\b|net\b|org\b|[a-z][a-z]\b))(\:\d+)?(\/[^.!,?;""'<>()\[\]{}\s\x7F-\xFF]*(?:[.!,?]+[^.!,?;""'<>()\[\]{}\s\x7F-\xFF]+)*)?", RegexOptions.Compiled| RegexOptions.IgnoreCase);
        
        private bool formClosing;
        //private int reconnectTries = 0;
        //private int maxReconnectTries = 5;

        internal ChannelForm()
        {
            InitializeComponent();
        }

        private static readonly Color[] ColorList = new Color[]
        {
            Color.FromArgb(219,160,170),
            Color.FromArgb(229,161,204),
            Color.FromArgb(221,161,229),
            Color.FromArgb(201,161,229),
            Color.FromArgb(172,161,229),
            Color.FromArgb(161,177,229),
            Color.FromArgb(161,196,229),
            Color.FromArgb(161,221,229),
            Color.FromArgb(161,229,206),
            Color.FromArgb(161,229,172),
            Color.FromArgb(183,229,161),
            Color.FromArgb(204,229,161),
            Color.FromArgb(227,229,161),
            Color.FromArgb(229,204,161),
            Color.FromArgb(229,183,161)
        }; 

        public ChannelForm(RacetimeChannel channel, string channelId, bool alwaysOnTop = true)
        {
            ChannelID = channelId;
            Channel = channel;
            Channel.ChannelJoined += Channel_ChannelJoined;
            Channel.StateChanged += Channel_StateChanged;
            Channel.UserListRefreshed += Channel_UserListRefreshed;
            Channel.GoalChanged += Channel_GoalChanged;
            Channel.MessageReceived += Channel_ChatUpdate;
            Channel.RawMessageReceived += Channel_RawMessageReceived;
            Channel.RequestOutputReset += Channel_RequestOutputReset;
            Channel.Disconnected += Channel_Disconnected;
            

            InitializeComponent();
            DownloadAllEmotes();
            TopMost = alwaysOnTop;
            Show();
            Text = channelId.Substring(channelId.IndexOf('/')+1);
            SetInitialState();
            actionButton.Enabled = false;
            Channel.Connect(channelId);
            infoLabel.LinkClicked += (ss, args) => { if (urlPattern.IsMatch(infoLabel.Text)) Process.Start(infoLabel.Text.Substring(args.Link.Start,args.Link.Length)); };
        }

        protected void DownloadAllEmotes()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    this.InvokeIfRequired(TwitchEmoteResolver.DownloadTwitchEmotesList);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            });
        }

        public void InvokeIfRequired(Action action)
        {
            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }

        private void Channel_Disconnected(object sender, EventArgs e)
        {
            if(!IsDisposed)
            {
                SetInitialState();
                forceReloadButton.Enabled = false;
                actionButton.Enabled = true;
            }
        }

        private void Channel_RequestOutputReset(object sender, EventArgs e)
        {
            chatBox.Clear();
        }

        private void Channel_RawMessageReceived(object sender, string value)
        {
            Console.WriteLine(value);
        }

        private void Channel_ChatUpdate(object sender, IEnumerable<ChatMessage> chatMessages)
        {
            if (formClosing)
                return;            

            foreach(ChatMessage m in chatMessages)
            {
                if (m.Type == MessageType.Race)
                    continue;
                if (Channel.Race?.State == RaceState.Started && hideFinishesCheckBox.Checked && m is RaceBotMessage && ((RaceBotMessage)m).IsFinishingMessage)
                    continue;
                if (Channel.Race?.State == RaceState.Started && hideChatCheckBox.Checked && ((m.User.Role & (UserRole.ChannelCreator|UserRole.Monitor|UserRole.Bot|UserRole.Staff|UserRole.System))==0))
                    continue;

                chatBox.AppendText("\n");
                chatBox.AppendText(m.Posted.ToString("HH:mm"), Color.Silver, Color.White);
                chatBox.AppendText("  ");

                Color col = Color.White;
                RacetimeUser u = RacetimeUser.Anonymous;
                bool hideUsername = m.User == null || m.User == RacetimeUser.LiveSplit || (m.User == RacetimeUser.RaceBot && !m.IsSystem);

                if (m.User == RacetimeUser.RaceBot)
                    col = Color.FromArgb(255, 50, 50);
                else
                {
                    col = ColorList[Math.Abs(m.User.Class) % ColorList.Length];
                }
                
                if(!hideUsername)
                    chatBox.AppendText(m.User == RacetimeUser.RaceBot ? "  "+m.User.Name+"  " : m.User.Name, col, Color.White, false, m.User == RacetimeUser.RaceBot);

                chatBox.AppendText("  ");

                string[] words = m.Message.Split(' ');
                bool firstWord = true;

                if(m.Highlight)
                {//255 94 94, 2 198 34, 255 50 50, 155 178 0
                    chatBox.SelectionColor = PickHighlightColor(m);
                }

                foreach(var word in words)
                {
                    if (TwitchEmoteResolver.IsEmote(word))
                    {
                        chatBox.AppendText((firstWord ? "" : " "));
                        var image = TwitchEmoteResolver.Resolve(word);
                        var whiteImage = new Bitmap(image.Width, image.Height);
                        var g = Graphics.FromImage(whiteImage);
                        g.FillRectangle(Brushes.White, 0, 0, image.Width, image.Height);
                        g.DrawImage(image, 0, 0, image.Width, image.Height);
                        Clipboard.SetDataObject(whiteImage);
                        chatBox.ReadOnly = false;
                        chatBox.Paste();
                        chatBox.ReadOnly = true;
                    }
                    else
                        chatBox.AppendText((firstWord ? "" : " ")+word);

                    firstWord = false;
                }

                if(m.Highlight)
                {
                    chatBox.SelectionColor = chatBox.ForeColor;
                }

                chatBox.SelectionStart = chatBox.Text.Length;
                chatBox.ScrollToCaret();
            }
           
        }

        private Color PickHighlightColor(ChatMessage m)
        {
            if (m is ErrorMessage)
                return Color.FromArgb(255, 94, 94);
            if(m is RaceBotMessage || m is LiveSplitMessage)
            {
                if (m.Message.Contains("finish") || m.Message.Contains("begun"))
                    return Color.FromArgb(2, 198, 34);
                else if (m.Message.Contains("cancel") || m.Message.Contains("forfeit") || m.Message.Contains("disqual"))
                    return Color.FromArgb(255, 94, 94);
                else if(m.Message.Contains("begin"))
                    return Color.FromArgb(155, 178, 0);

            }

            return Color.White;
        }

        private void Channel_GoalChanged(object sender, EventArgs e)
        {
            goalLabel.Text = Channel.Race.Goal ?? "";
            
            var s = Channel.Race.Info;
            Console.WriteLine(s);
            infoLabel.Text = s;
            //infoLabel.Text = s;

            MatchCollection mc = urlPattern.Matches(s);


            infoLabel.Links.Clear();
            foreach (Match m in mc)
            {

                infoLabel.Links.Add(m.Index, m.Length);
            }
            
        }

        private void Channel_UserListRefreshed(object sender, EventArgs e)
        {
            if ((hideFinishesCheckBox.Checked || hideChatCheckBox.Checked) && Channel.PersonalStatus== UserStatus.Racing)
                return;

            userlist.Clear();
            foreach(RacetimeUser u in Channel.Race.Entrants)
            {
                userlist.AddUser(u,Channel.Race.StartedAt);
            }
        }

        private void SetInitialState()
        {
            actionButton.Enabled = false;
            actionButton.Text = "(Re)Connect";
            readyCheckBox.Enabled = false;
            readyCheckBox.Checked = false;
            forceReloadButton.Enabled = false;
            saveLogButton.Enabled = false;
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
                    readyCheckBox.Enabled = false;
                    readyCheckBox.Checked = false;
                    forceReloadButton.Enabled = false;
                    saveLogButton.Enabled = false;
                    break;
                case UserStatus.NotReady:
                    actionButton.Enabled = true;
                    actionButton.Text = "Quit Race";
                    readyCheckBox.Enabled = true;
                    readyCheckBox.Checked = false;
                    forceReloadButton.Enabled = true;
                    saveLogButton.Enabled = true;
                    break;
                case UserStatus.Ready:
                    actionButton.Enabled = true;
                    actionButton.Text = "Quit Race";
                    readyCheckBox.Enabled = true;
                    readyCheckBox.Checked = true;
                    forceReloadButton.Enabled = true;
                    saveLogButton.Enabled = true;
                    break;
                case UserStatus.Racing:
                    actionButton.Enabled = true;
                    actionButton.Text = "Forfeit Race";
                    readyCheckBox.Enabled = false;
                    readyCheckBox.Checked = true;
                    forceReloadButton.Enabled = true;
                    saveLogButton.Enabled = true;
                    break;
                case UserStatus.Finished:
                case UserStatus.Forfeit:
                    if(hideFinishesCheckBox.Checked)
                    {
                        Channel_UserListRefreshed(sender,null);
                    }
                    actionButton.Enabled = true;
                    actionButton.Text = "Undone";
                    readyCheckBox.Enabled = false;
                    readyCheckBox.Checked = true;
                    forceReloadButton.Enabled = true;
                    saveLogButton.Enabled = true;
                    break;
                case UserStatus.Disqualified:
                    actionButton.Enabled = false;
                    if (hideFinishesCheckBox.Checked)
                    {
                        Channel_UserListRefreshed(sender, null);
                    }

                    forceReloadButton.Enabled = false;
                    saveLogButton.Enabled = true;
                    break;
                default:
                    actionButton.Enabled = false;
                    actionButton.Text = "";
                    readyCheckBox.Enabled = false;
                    readyCheckBox.Checked = false;
                    forceReloadButton.Enabled = false;
                    saveLogButton.Enabled = false;
                    break;
            }
            readyCheckBox.CheckedChanged += readyCheckBox_CheckedChanged;
            
            inputBox.Enabled = (!(!Channel.Race.AllowNonEntrantChat && Channel.PersonalStatus == UserStatus.NotInRace) &&
                                !(Channel.Race.State == RaceState.Started && !Channel.Race.AllowMidraceChat)) || 
                                !(Channel.PersonalStatus == UserStatus.Disqualified);


        }

        private void actionButton_Click(object sender, EventArgs e)
        {
            DialogResult r = DialogResult.None;
            actionButton.Enabled = false;
            if(!Channel.IsConnected)
            {
                Channel.Connect(ChannelID);
                return;
            }
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
                    r = MessageBox.Show("Are you sure that you want forfeit this race?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (r == DialogResult.Yes)
                        Channel.SendChannelMessage(".forfeit");
                    else
                        Channel_StateChanged(sender, Channel.Race.State);
                    break;               
                
                case UserStatus.Finished:
                    r = MessageBox.Show("You are already done. Are you sure?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (r == DialogResult.Yes)
                        Channel.SendChannelMessage(".undone");
                    else
                        Channel_StateChanged(sender, Channel.Race.State);
                    break;
                case UserStatus.Forfeit:
                    Channel.SendChannelMessage(".undone");
                    break;
            }
        }

        private void Channel_ChannelJoined(object sender, EventArgs e)
        {
            forceReloadButton.Enabled = true;
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
            if(Channel.Race?.State == RaceState.Started && Channel.PersonalStatus == UserStatus.Racing)
            {
                DialogResult r = MessageBox.Show("Do you want to FORFEIT before closing the window?", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if(r == DialogResult.Yes)
                {
                    Channel.Forfeit();                   
                }
                else if(r == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            formClosing = true;
            Channel.ChannelJoined -= Channel_ChannelJoined;
            Channel.StateChanged -= Channel_StateChanged;
            Channel.UserListRefreshed -= Channel_UserListRefreshed;
            Channel.GoalChanged -= Channel_GoalChanged;
            Channel.MessageReceived -= Channel_ChatUpdate;

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
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Log Files (*.log)|*.*";
            DialogResult res = sfd.ShowDialog();
            if(res == DialogResult.OK)
            {
                try
                {
                    WebRequest wr = WebRequest.Create($"{Channel.FullWebRoot}{Channel.Race.ID}/log");
                    using (WebResponse sp = wr.GetResponse())
                    {
                        StreamReader sr = new StreamReader(sp.GetResponseStream());
                        string content = sr.ReadToEnd();

                        File.WriteAllText(sfd.FileName, content);
                    }
                }
                catch(UnauthorizedAccessException)
                {
                    MessageBox.Show("No write permissions", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch(IOException)
                {
                    MessageBox.Show("I/O Error. Can not write to disk", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);                
                }
                catch(Exception)
                {
                    MessageBox.Show("Could not save Logfile. Reason unknown", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
        }

        private void forceReloadButton_Click(object sender, EventArgs e)
        {
            Channel.ForceReload();
        }
    }       
}
