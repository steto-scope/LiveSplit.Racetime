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
    public partial class UserListControl : UserControl
    {
        public UserListControl()
        {
            InitializeComponent();
        }
        
        public void Update(Race race)
        {
            userPanel.SuspendLayout();

            var ent = race.Entrants.ToList();
            

            for(int i = userPanel.RowCount-1; i < ent.Count; i++)
            {
                userPanel.RowStyles.Add(new RowStyle(SizeType.Absolute,18));
            }

            if(ent.Count > 0 && ent[0].HasFinished)
            {
                userPanel.ColumnStyles[0].Width = 35;
            }
            else
            {
                userPanel.ColumnStyles[0].Width = 0;
            }

            for (int i = 0; i < ent.Count; i++)
            {
                Label lname;
                Label lplace;
                PictureBox pstatus;
                Label ltime;

                if (userPanel.Controls.ContainsKey($"{i}_placement"))
                {
                    lname = userPanel.Controls[$"{i}_name"] as Label;
                    lplace = userPanel.Controls[$"{i}_placement"] as Label;
                    pstatus = userPanel.Controls[$"{i}_status"] as PictureBox;
                    ltime = userPanel.Controls[$"{i}_time"] as Label;
                }
                else
                {
                    lname = new Label() { Name = $"{i}_name", ForeColor = Color.White };
                    lplace = new Label() { Name = $"{i}_placement", ForeColor = Color.White};
                    pstatus = new PictureBox() { Name =$"{i}_status", SizeMode = PictureBoxSizeMode.CenterImage, Width=18, Height=16, Dock = DockStyle.Top/*, Width = 14*/ };
                    ltime = new Label() { Name = $"{i}_time", ForeColor = Color.White };
                    
                    userPanel.Controls.Add(lplace, 0, i);
                    userPanel.Controls.Add(pstatus, 1, i);
                    userPanel.Controls.Add(lname, 2, i);
                    userPanel.Controls.Add(ltime, 3, i);
                }

                lname.Text = ent[i].Name;

                if (ent[i].Place > 0)
                {
                    lplace.Text = ent[i].PlaceOrdinal;
                    switch (ent[i].Place)
                    {
                        case 1: lplace.ForeColor = Color.FromArgb(246, 218, 22); break;
                        case 2: lplace.ForeColor = Color.FromArgb(180, 180, 180); break;
                        case 3: lplace.ForeColor = Color.FromArgb(209, 123, 40); break;
                        default: lplace.ForeColor = Color.White; break;
                    }

                }
                else
                {
                    lplace.Text = "";
                }


                pstatus.Image = null;
                if (ent[i].Status == UserStatus.Forfeit || ent[i].Status == UserStatus.Disqualified)
                    pstatus.Image = Properties.Resources.f;
                else if (ent[i].IsLive || ent[i].StreamOverride)
                {
                    pstatus.Image = Properties.Resources.live;
                    if (ent[i].Status == UserStatus.Ready || ent[i].Status == UserStatus.Racing || ent[i].Status == UserStatus.Finished)
                        pstatus.Image = Properties.Resources.live_and_ready;
                }
                else if (ent[i].TwitchChannel != null && !ent[i].IsLive)
                    pstatus.Image = Properties.Resources.not_live;

                var finishtime = ent[i].FinishedAt - race.StartedAt;

                if (ent[i].Status == UserStatus.Disqualified || ent[i].Status == UserStatus.Forfeit)
                {
                    ltime.Text = "- - : - - : - -";
                }
                else if (ent[i].HasFinished)
                {
                    ltime.Text = string.Format("{0:hh\\:mm\\:ss}", finishtime);
                }
                else
                    ltime.Text = "";
            }

            int n = ent.Count;
            while(userPanel.Controls.ContainsKey($"{n}_placement"))
            {
                userPanel.Controls.RemoveByKey($"{n}_name");
                userPanel.Controls.RemoveByKey($"{n}_placement");
                userPanel.Controls.RemoveByKey($"{n}_time");
                userPanel.Controls.RemoveByKey($"{n}_status");
                n++;
            }

            userPanel.ResumeLayout();
        }
    }
}
