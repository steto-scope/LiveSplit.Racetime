using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveSplit.Racetime.Model;

namespace LiveSplit.Racetime.View
{
    public partial class RacerControl : UserControl
    {
        protected RacetimeUser User { get; set; }

        public RacerControl()
        {
            InitializeComponent();
        }
        
        public void UpdateUser(RacetimeUser user)
        {
            usernameLabel.Text = user.Name;
            if(user.Place>0)
            {
                placementLabel.Text = user.PlaceOrdinal;
                placementLabel.Visible = true;
            }
            else
            {
                placementLabel.Visible = false;
            }


            liveStatusImage.Image = null;
            if (user.IsLive)
                liveStatusImage.Image = Properties.Resources.live;
            else if(user.TwitchChannel != null)
                liveStatusImage.Image = Properties.Resources.not_live;
            if (user.Status == UserStatus.Ready)
                liveStatusImage.Image = Properties.Resources.live_and_ready;

            timeLabel.Text = (user.FinishTime > 0) ? string.Format(string.Format("{0:hh\\:mm\\:ss}", TimeSpan.FromSeconds(user.FinishTime))) : "";

        }
    }
}
