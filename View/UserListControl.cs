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

        public void AddUser(RacetimeUser u, DateTime raceStartingTime)
        {
            if (u != null)
            {
                RacerControl rc = new RacerControl();
                rc.Update(u, raceStartingTime);
                userPanel.Controls.Add(rc);
            }
        }

        public void Clear()
        {
            userPanel.Controls.Clear();
        }

    }
}
