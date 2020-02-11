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
        
        public void Update(RacetimeUser user, DateTime raceStartingTime)
        {
            usernameLabel.Text = user.Name;
            if(user.Place>0)
            {
                placementLabel.Text = user.PlaceOrdinal;
                switch(user.Place)
                {
                    case 1: placementLabel.ForeColor = Color.FromArgb(246,218,22); break;
                    case 2: placementLabel.ForeColor = Color.FromArgb(180, 180, 180); break;
                    case 3: placementLabel.ForeColor = Color.FromArgb(209, 123, 40); break;
                    default: placementLabel.ForeColor = Color.White; break;
                }

            }
            else
            {
                placementLabel.Text = "";
            }


            liveStatusImage.Image = null;
            if (user.Status == UserStatus.Forfeit || user.Status == UserStatus.Disqualified)
                liveStatusImage.Image = Properties.Resources.f;
            else if (user.IsLive || user.StreamOverride)
            {
                liveStatusImage.Image = Properties.Resources.live;
                if (user.Status == UserStatus.Ready || user.Status == UserStatus.Racing || user.Status == UserStatus.Finished)
                    liveStatusImage.Image = Properties.Resources.live_and_ready;
            }
            else if (user.TwitchChannel != null && !user.IsLive)
                liveStatusImage.Image = Properties.Resources.not_live;
            

            var finishtime = user.FinishedAt - raceStartingTime;
          
            if(user.Status == UserStatus.Disqualified || user.Status == UserStatus.Forfeit)
            {
                timeLabel.Text = "- - : - - : - -";
            }
            else if (user.HasFinished)
            {
                timeLabel.Text = string.Format("{0:hh\\:mm\\:ss}", finishtime);
            }
            else
                timeLabel.Text = "";


        }
    }
}
