using LiveSplit.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Model
{
    public class RacetimeUser : RTModelBase
    {
        private int nameChcecksum = -1;
        public int Class
        {
            get
            {
                if (nameChcecksum == -1)
                    nameChcecksum = Name.Sum(x => (int)x);
                return nameChcecksum;
            }
        }
        public override bool Equals(object obj)
        {
            return Name.ToLower() == ((RacetimeUser)obj)?.Name?.ToLower();
        }
        public string Name
        {
            get
            {
                return Data.name ?? "";
            }
        }
        public string TwitchChannel
        {
            get
            {
                return Data.twitch_channel;
            }
        }
        public string TwitchName
        {
            get
            {
                return Data.twitch_name;
            }
        }
        public UserRole Role
        {
            get
            {
                UserRole r = UserRole.Regular;

                if (Data.user.flair == null)
                    return UserRole.Unknown;

                string[] flairs = Data.user.flair.ToString().Split(' ');
                foreach (string f in flairs)
                {
                    switch (f)
                    {
                        case "staff": r |= UserRole.Staff; break;
                        case "moderator": r |= UserRole.Moderator; break;
                        case "monitor": r |= UserRole.Monitor; break;
                        case "bot": r |= UserRole.Bot; break;
                        case "system": r |= UserRole.System; break;
                        case "anonymous": r |= UserRole.Anonymous; break;
                    } 
                }
                return r;
            }
        }
        public UserStatus Status
        {
            get
            {
                UserStatus s = UserStatus.Unknown;
                if (Data.status == null)
                    return UserStatus.Unknown;

                switch (Data.status.value)
                {
                    case "not_ready": s = UserStatus.NotReady; break;
                    case "ready": s = UserStatus.Ready; break;
                    default:s = UserStatus.Unknown; break;
                }
                return s;
            }
        }
        
        public int FinishTime
        {
            get
            {
                try
                {
                    return Data.finish_time;
                }
                catch
                {
                    return 0;
                }
            }
        }
        public int Place
        {
            get
            {
                try
                {
                    return Data.place;
                }
                catch
                {
                    return int.MaxValue;
                }
            }
        }
        public string PlaceOrdinal
        {
            get
            {
                try
                {
                    return Data.place_ordinal;
                }
                catch
                {
                    return null;
                }
            }
        }
        public string Comment
        {
            get
            {
                try
                {
                    return Data.comment;
                }
                catch
                {
                    return null;
                }
            }
        }
        public bool IsLive
        {
            get
            {
                try
                {
                    return Data.stream_live || Data.stream_override;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static RacetimeUser RaceBot = CreateBot("RaceBot", "bot staff moderator monitor");
        public static RacetimeUser LiveSplit = CreateBot("LiveSplit", "system staff moderator monitor");
        public static RacetimeUser Anonymous = CreateBot("Anonymous", "anonymous");

        
        public static RacetimeUser CreateBot(string botname, string flairs)
        {
            var dataroot = new
            {
                    name = botname,
                    id = botname,
                    flair = flairs,
            };
            return Create<RacetimeUser>(dataroot);
        }
    }
}
