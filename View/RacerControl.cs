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
                //placementLabel.Width = 10;
                //placementLabel.Visible = true;
                switch(user.Place)
                {
                    case 1: placementLabel.ForeColor = Color.Gold; break;
                    case 2: placementLabel.ForeColor = Color.FromArgb(100, 100, 100); break;
                    case 3: placementLabel.ForeColor = Color.FromArgb(100, 65, 0); break;
                    default: placementLabel.ForeColor = Color.White; break;
                }

            }
            else
            {
                placementLabel.Text = "";
                //placementLabel.Visible = false;
            }


            liveStatusImage.Image = null;
            if (user.Status == UserStatus.Forfeit || user.Status == UserStatus.Disqualified)
                liveStatusImage.Image = Properties.Resources.flag;
            else if (user.IsLive || user.StreamOverride)
            {
                liveStatusImage.Image = Properties.Resources.live;
                if (user.Status == UserStatus.Ready || user.Status == UserStatus.Racing || user.Status == UserStatus.Finished)
                    liveStatusImage.Image = Properties.Resources.live_and_ready;
            }
            else if (user.TwitchChannel != null && !user.IsLive)
                liveStatusImage.Image = Properties.Resources.not_live;
            

            /*if (user.StreamOverride)
                liveStatusImage.Image = Properties.Resources.live_and_ready;*/

            //Console.WriteLine(user.FinishTime);
            var finishtime = user.FinishedAt - raceStartingTime;
            /*Console.WriteLine(user.FinishedAt);
            Console.WriteLine(raceStartingTime);
            Console.WriteLine(finishtime);*/
            timeLabel.Text = (user.HasFinished) ? string.Format(string.Format("{0:hh\\:mm\\:ss}", finishtime)) : "";

        }
    }
}
