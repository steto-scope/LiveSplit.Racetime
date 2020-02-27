using LiveSplit.Model;
using LiveSplit.Racetime.Controller;
using LiveSplit.Racetime.Model;
using LiveSplit.Racetime.View;
using LiveSplit.UI.Components;
using LiveSplit.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace LiveSplit.Racetime
{
    public class RacetimeAPI : RaceProviderAPI
    {
        protected static readonly Uri BaseUri = new Uri($"{Properties.Resources.PROTOCOL_REST}://{Properties.Resources.DOMAIN}/");
        protected static string racesEndpoint => Properties.Resources.ENDPOINT_RACES;

        private static RacetimeAPI _instance;
        public static RacetimeAPI Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RacetimeAPI();
                return _instance;
            }
        }


        public RacetimeAPI()
        {
            Authenticator = new RacetimeAuthenticator(new RTAuthentificationSettings());
            JoinRace = Join;
            CreateRace = Create;
        }

        public void Join(ITimerModel model, string id)
        {

            var channel = new RacetimeChannel(model.CurrentState, model);
            var form = new ChannelForm(channel, id, model.CurrentState.LayoutSettings.AlwaysOnTop);
        }

        public void Warn()
        {
            
        }

        public void Create(ITimerModel model)
        {
            Process.Start(GetUri(Properties.Resources.CREATE_RACE_ADDRESS).AbsoluteUri);
        }

        public IEnumerable<Race> Races { get; set; }

        internal RacetimeAuthenticator Authenticator { get; set; }

        public override string ProviderName => "racetime.gg";

        public override string Username => Authenticator.Identity?.Name;

        protected Uri GetUri(string subUri)
        {
            return new Uri(BaseUri, subUri);
        }

        public override void RefreshRacesListAsync()
        {
            Task.Factory.StartNew(() => RefreshRacesList());
        }

        protected void RefreshRacesList()
        {
            Races = GetRacesFromServer().ToArray();
            RacesRefreshedCallback?.Invoke(this);
        }

        
        protected IEnumerable<Race> GetRacesFromServer()
        {
            var races = JSON.FromUri(new Uri(BaseUri.AbsoluteUri + racesEndpoint)).races;
        
            foreach (var r in races)
            {
                var fulldata = JSON.FromUri(new Uri(BaseUri.AbsoluteUri + r.name + "/data"));
                Race raceObj = RTModelBase.Create<Race>(fulldata);
                yield return raceObj;
            }
            yield break;
        }

        
        public override IEnumerable<IRaceInfo> GetRaces()
        {
            return Races;
        }

        public override Image GetGameImage(string id)
        {
            return null;
        }
    }
}
