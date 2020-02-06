using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Model
{
    public class Race
    {
        public bool AllowNonEntrantChat { get; set; }
        public bool AllowMidraceChat { get; set; }
        public bool AllowComments { get; set; }
        public string ID { get; set; }
        public string ChannelName { get; set; }
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
        public TimeSpan Elapsed
        {
            get
            {
                if (!StartedAt.HasValue)
                    return TimeSpan.Zero;

                TimeSpan elapsed = TimeStamp.CurrentDateTime - StartedAt.Value;
                if (elapsed < TimeSpan.Zero)
                    elapsed = TimeSpan.Zero;
                return elapsed;
            }
        }
        public int NumFinishes
        {
            get
            {
                return Entrants.Count(x => x.Status == UserStatus.Finished);
            }
        }
        public int NumDropOuts
        {
            get
            {
                return Entrants.Count(x => x.Status == UserStatus.Forfeit || x.Status == UserStatus.Disqualified);
            }
        }
        public DateTime OpenedAt { get; set; }
        public RacetimeUser OpenedBy { get; set; }

        protected Race(string id)
        {
            ID = id;
            NumEntrants = 0;
            Entrants = new List<RacetimeUser>();
            GameSlug = id.Split('/')[0];
            ChannelName = id.Split('/')[1];            
        }

        public override bool Equals(object obj)
        {
            return ID == ((Race)obj).ID;
        }

        public static Race Create(string id)
        {
            if (!string.IsNullOrEmpty(id))
                return new Race(id);
            return null;
        }

        public static Race Deserialize(dynamic r)
        {
            Race race = null;
            try
            {
                race = Race.Create(r?.name);
            }
            catch
            {
                return race;
            }

            try
            {
                race.AllowNonEntrantChat = true;
                race.AllowMidraceChat = r.allow_midrace_chat == null ? false : r.allow_midrace_chat == true;
                race.AllowComments = r.allow_comments == null ? false : r.allow_comments == true;
                race.Goal = r.goal.name;
                race.Info = r.info;
                race.NumEntrants = r.entrants_count;
                race.OpenedAt = DateTime.Parse(r.opened_at);
                race.OpenedBy = RacetimeUser.DeserializeUser(r.opened_by);

                switch (r.status.value)
                {
                    case "open": race.State = RaceState.Open; break;
                    default: race.State = RaceState.Unknown; break;
                }

                race.Entrants.Clear();
                foreach (var ent in r.entrants)
                {
                    var e = RacetimeUser.DeserializeEntrant(ent);
                    if (e != null)
                        race.Entrants.Add(e);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return race;            
        }
    }
}
