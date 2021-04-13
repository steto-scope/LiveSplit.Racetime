using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.Racetime
{
    public partial class RacetimeSettingsControl : UserControl
    {
        private RacetimeSettings settings;

        public RacetimeSettings Settings
        {
            get { return settings; }
            set { settings = value; RacetimeSettingsControl_VisibleChanged(this, null); }
        }



        public RacetimeSettingsControl()
        {
            InitializeComponent();
        }

        private void RacetimeSettingsControl_VisibleChanged(object sender, EventArgs e)
        {
        }
    }
}
