using DarkUI.Forms;
using LiveSplit.Racetime.Controller;
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

        internal ChannelForm()
        {
            InitializeComponent();
        }

        public ChannelForm(RacetimeChannel channel, bool alwaysOnTop = false)
        {
            Channel = channel;
            Channel.ChannelJoined += Channel_ChannelJoined;
            Channel.StateChanged += Channel_StateChanged;
            Channel.UserListRefreshed += Channel_UserListRefreshed;
            Channel.GoalChanged += Channel_GoalChanged;
            Channel.ChatUpdate += Channel_ChatUpdate;
            Channel.Disconnected += Channel_Disconnected;

            InitializeComponent();
            TopMost = alwaysOnTop;
            Show();
        }

        private void Channel_Disconnected(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Channel_ChatUpdate(object sender, string value)
        {
            throw new NotImplementedException();
        }

        private void Channel_GoalChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Channel_UserListRefreshed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Channel_StateChanged(object sender, Model.RaceState value)
        {
            throw new NotImplementedException();
        }

        private void Channel_ChannelJoined(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ChannelWindow_Load(object sender, EventArgs e)
        {

        }

    }       
}
