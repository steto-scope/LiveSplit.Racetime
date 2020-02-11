using LiveSplit.Options;
using LiveSplit.Racetime.Controller;
using LiveSplit.Racetime.Model;
using LiveSplit.Web;
using LiveSplit.Web.SRL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
    public class RacetimeAPI : IUpdateable
    {
        protected static readonly Uri BaseUri = new Uri("http://192.168.178.70:8000/");
        protected static string racesEndpoint => "races/data";

        public event EventHandler RacetimeRacesRefreshed;
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
        }

        public IEnumerable<Race> Races { get; set; }

        public AuthenticatorBase Authenticator { get; set; }

        public string UpdateName => "Racetime Integration";

        public string XMLURL => throw new NotImplementedException();

        public string UpdateURL => throw new NotImplementedException();

        public Version Version => new Version(0, 7);

        protected Uri GetUri(string subUri)
        {
            return new Uri(BaseUri, subUri);
        }

        public void RefreshRacesListAsync()
        {
            Task.Factory.StartNew(() => RefreshRacesList());
        }

        protected void RefreshRacesList()
        {
            Races = GetRaces().ToArray();
            RacetimeRacesRefreshed?.Invoke(null, new EventArgs());
        }


        protected IEnumerable<Race> GetRaces()
        {
            var races = JSON.FromUri(new Uri(BaseUri.AbsoluteUri + racesEndpoint)).races;
        
            foreach (var r in races)
            {
                yield return RTModelBase.Create<Race>(r);
            }
            yield break;
        }


    }
}
