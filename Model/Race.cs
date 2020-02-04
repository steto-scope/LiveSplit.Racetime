using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Model
{
    public class Race
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string Goal { get; set; }
        public string Info { get; set; }
        public string Image { get; set; }
        public int NumEntrants { get; set; }
        public List<RacetimeUser> Entrants { get; set; }
        public string GameSlug { get; set; }
        public RaceState State { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime OpenedAt { get; set; }
        public RacetimeUser OpenedBy { get; set; }

        protected Race(string gameSlug)
        {
            NumEntrants = 0;
            Entrants = new List<RacetimeUser>();
            GameSlug = gameSlug;
        }

        public override bool Equals(object obj)
        {
            return ID == ((Race)obj).ID;
        }

        public static Race Create(string gameSlug)
        {
            if (!string.IsNullOrEmpty(gameSlug))
                return new Race(gameSlug);
            return null;
        }

        public static Race Deserialize(dynamic r)
        {
            Race race = Race.Create(r?.name);
            if(race != null)
            {
                race.Name = race.ID.Split('/')[1];
                race.GameSlug = race.ID.Split('/')[0];

                switch(r.status.value)
                {
                    case "open": race.State = RaceState.Open; break;
                    default: race.State = RaceState.Unknown; break;
                }
                race.Goal = r.goal.name;
                race.Info = r.info;
                race.NumEntrants = r.entrants_count;
                race.OpenedAt = r.opened_at;
                race.OpenedBy = RacetimeUser.DeserializeUser(r.opened_by);
                return race;
            }
            return null;
        }
    }
}
