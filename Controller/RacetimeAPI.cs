using LiveSplit.Options;
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


namespace LiveSplit.Racetime.Controller
{
    public class RacetimeAPI
    {
        private static readonly Uri BaseUri = new Uri("http://192.168.178.70:8000/");
        
        private string racesEndpoint => "races/data";

        

        public RacetimeChannel Channel { get; set; }


        protected Uri GetUri(string subUri)
        {
            return new Uri(BaseUri, subUri);
        }

        
        public IEnumerable<Race> GetRaces()
        {
            var races = JSON.FromUri(new Uri(BaseUri.AbsoluteUri + racesEndpoint)).races;
            
            foreach(var r in races)
            {
                yield return Race.Deserialize(r);
            }
            yield break;
        }


    }
}
