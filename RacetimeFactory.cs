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

        public string XMLURL => "https://steto-scope.github.io/LiveSplit.Racetime/update.racetime.xml";

        public string UpdateURL => "https://github.com/steto-scope/LiveSplit.Racetime/releases/download/";

        public Version Version => new Version(0, 7);

        
    }
}
