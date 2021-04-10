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
using System.Runtime.InteropServices;
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
        private static Regex urlPattern = new Regex(@"\b((ftp|https?):\/\/[-\w]+(\.\w[-\w]*)+|(?:[a-z0-9](?:[-a-z0-9]*[a-z0-9])?\.)+(?: com\b|edu\b|biz\b|gov\b|in(?:t|fo)\b|mil\b|net\b|org\b|[a-z][a-z]\b))(\:\d+)?(\/[^.!,?;""'<>()\[\]{}\s\x7F-\xFF]*(?:[.!,?]+[^.!,?;""'<>()\[\]{}\s\x7F-\xFF]+)*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private List<string> rtf_chatmessages = new List<string>();

        private bool formClosing;

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
            Channel.DeletedMessage += Channel_DeletedMessage;
            Channel.PurgedMessage += Channel_PurgedMessage;
            Channel.Disconnected += Channel_Disconnected;
            Channel.RaceChanged += Channel_RaceChanged;
            Channel.Authorized += Channel_Authorized;
            InitializeComponent();
            DownloadAllEmotes();
            TopMost = alwaysOnTop;
            Show();
            Text = "Connecting to " + channelId.Substring(channelId.IndexOf('/') + 1);
            SetInitialState();
            actionButton.Enabled = false;
            Channel.Connect(channelId);
            infoLabel.LinkClicked += (ss, args) => { if (urlPattern.IsMatch(infoLabel.Text)) Process.Start(infoLabel.Text.Substring(args.Link.Start, args.Link.Length)); };
            hideFinishesCheckBox.Checked = Channel.Settings.HideResults;
            HideCaret(chatBox.Handle);
        }
        private void chatBox_MouseUp(object sender, MouseEventArgs e)
        {
            HideCaret(chatBox.Handle);
        }

        private void chatBox_MouseDown(object sender, MouseEventArgs e)
        {
            HideCaret(chatBox.Handle);
        }
        [DllImport("user32.dll")]
        private static extern int HideCaret(IntPtr hwnd);
        private void Channel_RaceChanged(object sender, EventArgs e)
        {
            Text = $"{Channel.Race.Goal} [{Channel.Race.GameName}] - {Channel.Race.ChannelName}";
        }

        private void Channel_Authorized(object sender, EventArgs e)
        {
            Focus();
        }

        protected void DownloadAllEmotes()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    InvokeIfRequired(TwitchEmoteResolver.DownloadTwitchEmotesList);
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
            if (!IsDisposed)
            {
                Text = "Disconnected";
                SetInitialState();
                forceReloadButton.Enabled = false;
                actionButton.Enabled = true;
            }
        }

        private void Channel_RequestOutputReset(object sender, EventArgs e)
        {
            chatBox.Clear();
            rtf_chatmessages.Clear();
        }

        private void Channel_DeletedMessage(object sender, dynamic value)
        {
            try
            {
                int index = rtf_chatmessages.FindIndex(x => x == value.id);
                string line = chatBox.Lines[index + 1];
                chatBox.Select(chatBox.GetFirstCharIndexFromLine(index + 1), line.Length);
                Point pos = chatBox.GetPositionFromCharIndex(chatBox.SelectionStart);
                foreach (var con in chatBox.Controls.OfType<Button>())
                {
                    if (con.Location.Y == pos.Y + chatBox.Top)
                    {
                        chatBox.BeginInvoke((Action)(() => chatBox.Controls.Remove(con)));
                    }
                }
                chatBox.SelectedText = $"{value.deleted_by.name} deleted a message from {value.user.name}";
                chatBox.SelectionStart = chatBox.Text.Length;
                chatBox.ScrollToCaret();
            }
            catch (Exception ex) { }
            HideCaret(chatBox.Handle);
        }

        private void Channel_PurgedMessage(object sender, dynamic value)
        {
            List<int> DeleteLines = new List<int>();

            try
            {
                int counter = 0;
                foreach (var line in chatBox.Lines)
                {
                    string user = line.Trim().Split(new char[0], StringSplitOptions.RemoveEmptyEntries).Skip(1).FirstOrDefault();
                    if (user == value.user.name)
                    {
                        DeleteLines.Add(counter);
                    }
                    counter++;
                }
                DeleteLines.Reverse();
                var lines = chatBox.Lines;
                foreach (var index in DeleteLines)
                {
                    string line = chatBox.Lines[index];
                    chatBox.Select(chatBox.GetFirstCharIndexFromLine(index), line.Length);
                    Point pos = chatBox.GetPositionFromCharIndex(chatBox.SelectionStart);
                    foreach (var con in chatBox.Controls.OfType<Button>())
                    {
                        if (con.Text == "purge")
                        {
                            Console.WriteLine(con.Location.Y + " " + pos.Y + " " + chatBox.Top);
                        }
                        if (con.Location.Y == pos.Y + chatBox.Top)
                        {
                            Console.WriteLine(con.Location.Y + " " + pos.Y);
                            chatBox.BeginInvoke((Action)(() => chatBox.Controls.Remove(con)));
                        }
                    }
                    chatBox.DeselectAll();
                }
                foreach (var index in DeleteLines)
                {
                    string line = chatBox.Lines[index];
                    chatBox.Select(chatBox.GetFirstCharIndexFromLine(index), line.Length);
                    chatBox.SelectedText = $"{value.purged_by.name} purged all messages from {value.user.name}";
                    chatBox.DeselectAll();
                }

                chatBox.SelectionStart = chatBox.Text.Length;
                chatBox.ScrollToCaret();
            }
            catch (Exception ex) { }
            HideCaret(chatBox.Handle);

        }

        private void Channel_RawMessageReceived(object sender, string value)
        {
        }
        private void Purge_Clicked(object sender, EventArgs e)
        {
            Button cb = (sender as Button);
            int charindex = chatBox.GetCharIndexFromPosition(cb.Location);
            int index = chatBox.GetLineFromCharIndex(charindex) - 1;
            string id = rtf_chatmessages[index];
            var request = (HttpWebRequest)WebRequest.Create(new Uri(Channel.FullWebRoot + "o/" + Channel.Race.Id + $"/monitor/purge/{id}"));
            request.Method = "POST";
            request.Headers.Add("Authorization", "Bearer " + RacetimeAPI.Instance.Authenticator.AccessToken);
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
            }
            HideCaret(chatBox.Handle);
        }
        private void Delete_Clicked(object sender, EventArgs e)
        {

            Button cb = (sender as Button);
            int charindex = chatBox.GetCharIndexFromPosition(cb.Location);
            int index = chatBox.GetLineFromCharIndex(charindex) - 1;
            string id = rtf_chatmessages[index];
            var request = (HttpWebRequest)WebRequest.Create(new Uri(Channel.FullWebRoot + "o/" + Channel.Race.Id + $"/monitor/delete/{id}"));
            request.Method = "POST";
            request.Headers.Add("Authorization", "Bearer " + RacetimeAPI.Instance.Authenticator.AccessToken);
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
            }
            HideCaret(chatBox.Handle);
        }
        private void Channel_ChatUpdate(object sender, IEnumerable<ChatMessage> chatMessages)
        {
            if (formClosing)
                return;

            foreach (ChatMessage m in chatMessages)
            {
                if (m.Type == MessageType.Race)
                    continue;
                if (Channel.Race?.State == RaceState.Started && hideFinishesCheckBox.Checked && m is SystemMessage && ((SystemMessage)m).IsFinishingMessage)
                    continue;
                if (Channel.Race?.State == RaceState.Started && hideChatCheckBox.Checked && ((m.User.Role & (UserRole.ChannelCreator | UserRole.Monitor | UserRole.Bot | UserRole.Staff | UserRole.System)) == 0))
                    continue;
                chatBox.AppendText("\n");
                if (Channel.Monitors.Contains(Channel.Username) && (m.User != RacetimeUser.LiveSplit && m.User != RacetimeUser.System))
                {
                    var delete = new Button
                    {
                        Size = new System.Drawing.Size(16, 18),
                        Text = "trash",
                        BackgroundImage = LiveSplit.Racetime.Properties.Resources.delete,
                        BackgroundImageLayout = ImageLayout.Stretch,
                        BackColor = Color.Transparent,
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand,
                    };
                    delete.Click += new EventHandler(Delete_Clicked);
                    delete.FlatAppearance.BorderSize = 0;
                    Point pos = chatBox.GetPositionFromCharIndex(chatBox.SelectionStart);
                    chatBox.BeginInvoke((Action)(() => chatBox.Controls.Add(delete)));

                    delete.Location = new Point(chatBox.Left, pos.Y + chatBox.Top);
                    var purge = new Button
                    {
                        Size = new System.Drawing.Size(16, 16),
                        Text = "purge",
                        BackgroundImage = LiveSplit.Racetime.Properties.Resources.purge,
                        BackgroundImageLayout = ImageLayout.Stretch,
                        BackColor = Color.Transparent,
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand
                    };
                    purge.Click += new EventHandler(Purge_Clicked);
                    purge.FlatAppearance.BorderSize = 0;
                    pos = chatBox.GetPositionFromCharIndex(chatBox.SelectionStart);
                    chatBox.BeginInvoke((Action)(() => chatBox.Controls.Add(purge)));

                    purge.Location = new Point(chatBox.Left + 16, pos.Y + chatBox.Top);
                    chatBox.AppendText("            ");
                }
                chatBox.AppendText(m.Posted.ToString("HH:mm"), Color.Silver, Color.White);
                chatBox.AppendText("  ");

                Color col = Color.White;
                RacetimeUser u = RacetimeUser.Anonymous;
                bool hideUsername = m.User == RacetimeUser.LiveSplit || m.User == RacetimeUser.System;

                if (m.User == RacetimeUser.Bot)
                    col = Color.FromArgb(115, 100, 255);
                else
                {
                    col = ColorList[Math.Abs(m.User.Class) % ColorList.Length];
                }

                if (!hideUsername)
                    chatBox.AppendText(m is BotMessage ? "[" + (((BotMessage)m).BotName) + "]" : m.User.Name, col, Color.White, false, m.User == RacetimeUser.System);
                else if (m.User == RacetimeUser.System)
                    chatBox.AppendText("» ");


                chatBox.AppendText("  ");

                string[] words = m.Message.Split(' ');
                bool firstWord = true;

                if (m.Highlight)
                {
                    chatBox.SelectionColor = PickHighlightColor(m);
                }
                else if (m.User == RacetimeUser.Bot)
                {
                    chatBox.SelectionColor = Color.FromArgb(170, 170, 255);
                }

                foreach (var word in words)
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
                        chatBox.AppendText((firstWord ? "" : " ") + word);

                    firstWord = false;
                }
                if (m.Highlight)
                {
                    chatBox.SelectionColor = chatBox.ForeColor;
                }
                rtf_chatmessages.Add(m.Id);


                chatBox.SelectionStart = chatBox.Text.Length;
                chatBox.ScrollToCaret();
                HideCaret(chatBox.Handle);
            }

        }

        private Color PickHighlightColor(ChatMessage m)
        {
            if (m is ErrorMessage)
                return Color.FromArgb(255, 94, 94);
            if (m is SystemMessage || m is LiveSplitMessage)
            {
                if (m.Message.Contains("finish") || m.Message.Contains("begun"))
                    return Color.FromArgb(2, 198, 34);
                else if (m.Message.Contains("cancel") || m.Message.Contains("forfeit") || m.Message.Contains("disqual"))
                    return Color.FromArgb(255, 94, 94);
                else if (m.Message.Contains("begin"))
                    return Color.FromArgb(155, 178, 0);

            }

            return Color.White;
        }

        private void Channel_GoalChanged(object sender, EventArgs e)
        {
            goalLabel.Text = Channel.Race.Goal ?? "";

            var s = Channel.Race.Info;
            infoLabel.Text = s;

            MatchCollection mc = urlPattern.Matches(s);

            infoLabel.Links.Clear();
            foreach (Match m in mc)
            {
                infoLabel.Links.Add(m.Index, m.Length);
            }

        }

        private void Channel_UserListRefreshed(object sender, EventArgs e)
        {
            userlist.Update(Channel.Race);
        }

        private void SetInitialState()
        {
            actionButton.Enabled = false;
            actionButton.Text = "(Re)Connect";
            readyCheckBox.Enabled = false;
            readyCheckBox.Checked = false;
            forceReloadButton.Enabled = false;
            saveLogButton.Enabled = false;
            doneButton.Enabled = false;
            doneButton.Visible = false;
        }

        private void Channel_StateChanged(object sender, RaceState value)
        {
            if (IsDisposed)
                return;

            readyCheckBox.CheckedChanged -= readyCheckBox_CheckedChanged;
            hideFinishesCheckBox.Font = new Font(hideFinishesCheckBox.Font, Channel.PersonalStatus == UserStatus.Racing && Channel.Race.State == RaceState.Started && hideFinishesCheckBox.Checked ? FontStyle.Italic : FontStyle.Regular);
            switch (Channel.PersonalStatus)
            {
                case UserStatus.Unknown:
                    if (value == RaceState.Started)
                    {
                        actionButton.Enabled = false;
                        actionButton.Text = "Enter Race";
                        readyCheckBox.Enabled = false;
                        readyCheckBox.Checked = false;
                        forceReloadButton.Enabled = true;
                        saveLogButton.Enabled = true;
                        doneButton.Enabled = false;
                        doneButton.Visible = false;
                        hideFinishesCheckBox.Enabled = false;
                    }
                    else
                    {
                        actionButton.Enabled = true;
                        actionButton.Text = "Enter Race";
                        readyCheckBox.Enabled = false;
                        readyCheckBox.Checked = false;
                        forceReloadButton.Enabled = false;
                        saveLogButton.Enabled = false;
                        doneButton.Enabled = false;
                        doneButton.Visible = false;
                        hideFinishesCheckBox.Enabled = false;
                    }
                    break;
                case UserStatus.NotReady:
                    actionButton.Enabled = true;
                    actionButton.Text = "Quit Race";
                    readyCheckBox.Enabled = true;
                    readyCheckBox.Checked = false;
                    forceReloadButton.Enabled = true;
                    saveLogButton.Enabled = true;
                    doneButton.Enabled = false;
                    doneButton.Visible = false;
                    hideFinishesCheckBox.Enabled = true;
                    break;
                case UserStatus.Ready:
                    actionButton.Enabled = true;
                    actionButton.Text = "Quit Race";
                    readyCheckBox.Enabled = true;
                    readyCheckBox.Checked = true;
                    forceReloadButton.Enabled = true;
                    saveLogButton.Enabled = true;
                    doneButton.Enabled = false;
                    hideFinishesCheckBox.Enabled = true;
                    doneButton.Visible = false;
                    break;
                case UserStatus.Racing:
                    actionButton.Enabled = true;
                    actionButton.Text = "Forfeit Race";
                    readyCheckBox.Enabled = false;
                    hideFinishesCheckBox.Enabled = true;
                    readyCheckBox.Checked = true;
                    forceReloadButton.Enabled = true;
                    saveLogButton.Enabled = true;
                    doneButton.Enabled = true;
                    doneButton.Text = "Done";
                    doneButton.Visible = true;
                    if (hideFinishesCheckBox.Checked)
                        userlist.Visible = false;
                    break;
                case UserStatus.Finished:
                case UserStatus.Forfeit:
                    if (hideFinishesCheckBox.Checked)
                    {
                        Channel_UserListRefreshed(sender, null);
                    }
                    actionButton.Enabled = true;
                    actionButton.Text = "Undone";
                    hideFinishesCheckBox.Enabled = true;
                    readyCheckBox.Enabled = false;
                    readyCheckBox.Checked = true;
                    forceReloadButton.Enabled = true;
                    saveLogButton.Enabled = true;
                    doneButton.Enabled = true;
                    doneButton.Visible = true;
                    doneButton.Text = "Set Comment";
                    userlist.Visible = true;
                    break;
                case UserStatus.Disqualified:
                    actionButton.Enabled = false;
                    if (hideFinishesCheckBox.Checked)
                    {
                        Channel_UserListRefreshed(sender, null);
                    }

                    forceReloadButton.Enabled = false;
                    saveLogButton.Enabled = true;
                    doneButton.Text = "Disqualified";
                    hideFinishesCheckBox.Enabled = true;
                    doneButton.Visible = true;
                    doneButton.Enabled = false;
                    userlist.Visible = true;
                    break;
                default:
                    actionButton.Enabled = false;
                    actionButton.Text = "";
                    readyCheckBox.Enabled = false;
                    readyCheckBox.Checked = false;
                    forceReloadButton.Enabled = false;
                    hideFinishesCheckBox.Enabled = true;
                    saveLogButton.Enabled = false;
                    doneButton.Enabled = false;
                    doneButton.Visible = false;
                    break;
            }

            if (value == RaceState.Ended || value == RaceState.Cancelled)
            {
                actionButton.Enabled = false;
                actionButton.Text = "Race ended";
                readyCheckBox.Enabled = false;
                readyCheckBox.Checked = false;
                forceReloadButton.Enabled = true;
                hideFinishesCheckBox.Enabled = true;
                saveLogButton.Enabled = true;
                doneButton.Enabled = false;
                doneButton.Visible = false;
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
            if (!Channel.IsConnected)
            {
                Channel.Connect(ChannelID);
                return;
            }
            switch (Channel.PersonalStatus)
            {
                default:
                case UserStatus.NotInRace:
                    if (Channel.Invited == true)
                    {
                        Channel.Accept();
                    }
                    else
                    {
                        Channel.Enter();
                    }
                    break;
                case UserStatus.NotReady:
                case UserStatus.Ready:
                    Channel.Quit();
                    break;
                case UserStatus.Racing:
                    r = MessageBox.Show("Are you sure that you want forfeit this race?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (r == DialogResult.Yes)
                        Channel.Forfeit();
                    else
                        Channel_StateChanged(sender, Channel.Race.State);
                    break;

                case UserStatus.Finished:
                    Channel.Undone();
                    break;
                case UserStatus.Forfeit:
                    Channel.Undone();
                    break;
            }
        }

        private void Channel_ChannelJoined(object sender, EventArgs e)
        {
            forceReloadButton.Enabled = true;
        }

        private void ChannelWindow_Load(object sender, EventArgs e)
        {
            inputBox.Select();
        }

        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Return || e.KeyData == Keys.Enter)
            {
                if (!string.IsNullOrEmpty(inputBox.Text))
                {
                    if (inputBox.Text.Trim() == ".done")
                        Channel.Done();
                    else
                        Channel.SendChannelMessage(inputBox.Text);

                    inputBox.Text = "";
                    inputBox.SelectionStart = 0;
                    inputBox.SelectionLength = 0;
                }
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void readyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == this || sender == readyCheckBox)
            {
                if (readyCheckBox.Checked)
                    Channel.Ready();
                else
                    Channel.Unready();
            }
        }

        private void ChannelForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //RacetimeAPI.Instance.Authenticator.StopPendingAuthRequest();
            Channel.Settings.HideResults = hideFinishesCheckBox.Checked;

            if (Channel.Race?.State == RaceState.Started && Channel.PersonalStatus == UserStatus.Racing)
            {
                DialogResult r = MessageBox.Show("Do you want to FORFEIT before closing the window?", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (r == DialogResult.Yes)
                {
                    Channel.Forfeit();
                }
                else if (r == DialogResult.Cancel)
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
            Channel.Authorized -= Channel_Authorized;
            Channel.RaceChanged -= Channel_RaceChanged;

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
            sfd.DefaultExt = "log";
            sfd.Filter = "Log Files (*.log)|*.*";
            DialogResult res = sfd.ShowDialog();
            if (res == DialogResult.OK)
            {
                try
                {
                    WebRequest wr = WebRequest.Create($"{Channel.FullWebRoot}{Channel.Race.Id}/log");
                    using (WebResponse sp = wr.GetResponse())
                    {
                        StreamReader sr = new StreamReader(sp.GetResponseStream());
                        string content = sr.ReadToEnd();

                        File.WriteAllText(sfd.FileName, content);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("No write permissions", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (IOException)
                {
                    MessageBox.Show("I/O Error. Can not write to disk", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception)
                {
                    MessageBox.Show("Could not save Logfile. Reason unknown", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void forceReloadButton_Click(object sender, EventArgs e)
        {
            Channel.ForceReload();
        }

        private void chatBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private void doneButton_Click(object sender, EventArgs e)
        {
            if (Channel.Race?.State == RaceState.Started && Channel.PersonalStatus == UserStatus.Racing)
            {
                Channel.Done();
            }
            if (Channel.PersonalStatus == UserStatus.Forfeit || Channel.PersonalStatus == UserStatus.Finished)
            {
                inputBox.Text = ".comment ";
                inputBox.Focus();
                inputBox.SelectionStart = inputBox.Text.Length;
            }
        }

        private void hideFinishesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            userlist.Visible = !hideFinishesCheckBox.Checked;
            if (Channel.PersonalStatus != UserStatus.Racing)
            {
                userlist.Visible = true;
            }
            else
            {
                hideFinishesCheckBox.Font = new Font(hideFinishesCheckBox.Font, Channel.Race.State == RaceState.Started && hideFinishesCheckBox.Checked ? FontStyle.Italic : FontStyle.Regular);
            }
        }


    }
}
