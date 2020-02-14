using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.Racetime.Controller;
using LiveSplit.Racetime.Model;
using LiveSplit.Racetime.View;
using LiveSplit.UI.Components;
using LiveSplit.Web;
using LiveSplit.Web.SRL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UpdateManager;

/*
 
 Races List:
 
 {
  "count": "5552",
  "games": [
    {
      "id": 1,
      "name": "Super Mario 64",
      "abbrev": "sm64",
      "popularity": 974,
      "popularityrank": 1
    },
*/


namespace LiveSplit.Racetime
{
    public class RacetimeAPI : RaceProviderAPI
    {
        protected static readonly Uri BaseUri = new Uri("http://192.168.178.70:8000/");
        protected static string racesEndpoint => "races/data";

        

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
            var form = new ChannelForm(channel, id);
        }

        public void Create(ITimerModel model)
        {
            Process.Start(GetUri("/").AbsoluteUri);
        }

        public IEnumerable<Race> Races { get; set; }

        public AuthenticatorBase Authenticator { get; set; }

        public override string ProviderName => "Racetime";

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
