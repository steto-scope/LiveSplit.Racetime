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
                //placementLabel.Width = 10;
                //placementLabel.Visible = true;

            }
            else
            {
                placementLabel.Text = "";
                //placementLabel.Visible = false;
            }


            liveStatusImage.Image = null;
            if (user.IsLive || user.StreamOverride)
            {
                liveStatusImage.Image = Properties.Resources.live;
                if(user.Status == UserStatus.Ready || user.Status == UserStatus.Racing || user.Status == UserStatus.Finished)
                    liveStatusImage.Image = Properties.Resources.live_and_ready;
            }
            else if(user.TwitchChannel != null)
                liveStatusImage.Image = Properties.Resources.not_live;

            /*if (user.StreamOverride)
                liveStatusImage.Image = Properties.Resources.live_and_ready;*/

            Console.WriteLine(user.FinishTime);
            timeLabel.Text = (user.FinishTime > TimeSpan.Zero) ? string.Format(string.Format("{0:hh\\:mm\\:ss}", user.FinishTime)) : "";

        }
    }
}
