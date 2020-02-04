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

        public bool IsReady { get; set; }
        public int FinishTime { get; set; }
        public int Place { get; set; }
        public string PlaceOrdinal { get; set; }
        public string Comment { get; set; }
        public StreamState StreamState { get; set; }

        public static RacetimeUser RaceBot = Create(null, "RaceBot", UserRole.Bot);
        public static RacetimeUser LiveSplit = Create(null, "LiveSplit", UserRole.System);
        public static RacetimeUser Anonymous = Create(null, "Anonymous", UserRole.Anonymous);

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

            RacetimeUser user = DeserializeUser(e);
            if (user != null)
            {
                user.IsReady = e.status.value != "not_ready";
                user.FinishTime = e.finish_time;
                user.Place = e.place;
                user.PlaceOrdinal = e.place_ordinal;
                user.Comment = e.comment;
                user.StreamState = e.stream_override ? StreamState.Override : (e.stream_live ? StreamState.Live : StreamState.NotLive);
                return user;
            }
            return null;
        }

        public static RacetimeUser DeserializeUser(dynamic u)
        {
            if (u?.user == null)
                return null;

            RacetimeUser user = RacetimeUser.Create(u.user.id, u.user.name);
            if(user != null)
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

                return user;
            }

            return null;
        }
    }
}
