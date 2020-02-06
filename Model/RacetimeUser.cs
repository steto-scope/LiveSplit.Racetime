using LiveSplit.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Model
{
    public class RacetimeUser
    {
        public string Id { get; protected set; }
        public string Name { get; protected set; }
        public string TwitchChannel { get; set; }
        public string TwitchName { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        
        public int FinishTime { get; set; }
        public int Place { get; set; }
        public string PlaceOrdinal { get; set; }
        public string Comment { get; set; }
        public bool IsLive { get; set; }

        public static RacetimeUser RaceBot = Create("RaceBot", "RaceBot", UserRole.Bot);
        public static RacetimeUser LiveSplit = Create("LiveSplit", "LiveSplit", UserRole.System);
        public static RacetimeUser Anonymous = Create("Anonymous", "Anonymous", UserRole.Anonymous);

        private RacetimeUser(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public static RacetimeUser Create(string id, string name, UserRole role = UserRole.Unknown)
        {
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name))
                return new RacetimeUser(id, name) { Role = role };
            return null;
        }

        public static RacetimeUser DeserializeEntrant(dynamic e)
        {
            if (e?.user == null)
                return null;

            RacetimeUser user = null;

            try
            {
                user = DeserializeUser(e);
            }
            catch
            {
                return user;
            }

            try
            {
                
                user.FinishTime = e.finish_time;
                user.Place = e.place;
                user.PlaceOrdinal = e.place_ordinal;
                user.Comment = e.comment;
                user.IsLive = e.stream_live;
                return user;
            }
            catch { }

            return user;
        }
          
        public static RacetimeUser DeserializeUser(dynamic u)
        {
            if (u?.user == null)
                return null;
            RacetimeUser user = null;
            try
            {
                 user = RacetimeUser.Create(u.user.id, u.user.name);
            }
            catch
            {
                return user;
            }

            try
            {
                user.TwitchChannel = u.user.twitch_channel;
                user.TwitchName = u.user.twitch_name;

                string[] flairs = u.user.flair?.ToString().Split(' ');
                user.Role = UserRole.Unknown;
                foreach(string f in flairs)
                {
                    switch(f)
                    {
                        case "staff": user.Role |= UserRole.Staff; break;
                        case "moderator": user.Role |= UserRole.Moderator; break;
                        case "monitor": user.Role |= UserRole.Monitor; break;
                    }
                }

           
                switch (u.status?.value)
                {
                    case "not_ready": user.Status = UserStatus.NotReady; break;
                    case "ready": user.Status = UserStatus.Ready; break;
                    default: user.Status = UserStatus.Unknown; break;
                }
            }
            catch { }

            return user;
        }
    }
}
