using CefSharp;
using DarkUI.Forms;
using LiveSplit.Racetime.Controller;
using LiveSplit.Racetime.Model;
using System;
using System.Windows.Forms;

namespace LiveSplit.Racetime.View
{
    public partial class ChannelForm : DarkForm
    {
        public RacetimeChannel Channel { get; set; }

        public ChannelForm(RacetimeChannel channel, string channelId, bool alwaysOnTop = true)
        {
            Channel = channel;
            Channel.Disconnected += Channel_Disconnected;
            Channel.RaceChanged += Channel_RaceChanged;
            Channel.Authorized += Channel_Authorized;
            InitializeComponent();
            TopMost = alwaysOnTop;
            Show();
            Text = "Connecting to " + channelId.Substring(channelId.IndexOf('/') + 1);
            Channel.Connect(channelId);
            chatBox.LifeSpanHandler = new ChatBoxLifeSpanHandler();
        }

        private void Channel_RaceChanged(object sender, EventArgs e)
        {
            Text = $"{Channel.Race.Goal} [{Channel.Race.GameName}] - {Channel.Race.ChannelName}";
            chatBox.Load(Channel.FullWebRoot + Channel.Race.Id + "/livesplit");

        }

        private void Channel_Authorized(object sender, EventArgs e)
        {
            Focus();
        }

        private void Channel_Disconnected(object sender, EventArgs e)
        {
            if (!IsDisposed)
            {
                Text = "Disconnected";
            }
        }

        private void ChannelForm_FormClosing(object sender, FormClosingEventArgs e)
        {
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
            Channel.Authorized -= Channel_Authorized;
            Channel.RaceChanged -= Channel_RaceChanged;
            Channel.Disconnect();
        }

    }
    public class ChatBoxLifeSpanHandler : ILifeSpanHandler
    {
        public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            System.Diagnostics.Process.Start(targetUrl);
            newBrowser = null;
            return true;
        }
        public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser) { return true; }
        public void OnBeforeClose(IWebBrowser browser) { }
        public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser) { }
        public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser) { }
    }
}
