using LiveSplit.Model;
using LiveSplit.Racetime;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpdateManager;

[assembly: ComponentFactory(typeof(RacetimeFactory))]

namespace LiveSplit.Racetime
{
    public class RacetimeFactory : IRaceProviderFactory
    {
        public RaceProviderAPI Create(ITimerModel model)
        {
            return RacetimeAPI.Instance;
        }
        
        public string UpdateName => "Racetime Integration";

        public string XMLURL => Properties.Resources.UPDATE_DEFINITION;

        public string UpdateURL => Properties.Resources.UPDATE_DATA;

        public Version Version => Version.Parse(Properties.Resources.PLUGIN_VERSION);

        
    }
}
