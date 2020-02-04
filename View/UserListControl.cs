using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.Racetime.View
{
    public partial class UserListControl : UserControl
    {
        public UserListControl()
        {
            InitializeComponent();
            /*SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;*/
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
            userPanel.Controls.Add(new RacerControl());
        }

    }
}
