using LiveSplit.Model;
using LiveSplit.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LiveSplit.Racetime.Model
{
    public class Race : RTModelBase, IRaceInfo
    {
        public static dynamic EntrantToUserConverter(dynamic e)
        {

            return new
            {
                id = e.user.id,
                name = e.user.name,
                full_name = e.user.full_name,
                flair = e.user.flair,
                twitch_name = e.user.twitch_name,
                twitch_channel = e.user.twitch_channel,
                discriminator = e.user.discriminator,
                status = e.status,
                place = e.place,
                place_ordinal = e.place_ordinal,
                finish_time = e.finish_time,
                finished_at = e.finished_at,
                start_delay = e.start_delay,
                stream_live = e.stream_live,
                comment = e.comment,
                stream_override = e.stream_override
            };
        }
        public bool AllowNonEntrantChat
        {
            get
            {
                return false;
            }
        }
        public bool AllowMidraceChat
        {
            get
            {
                return Data.allow_midrace_chat;
            }
        }
        public bool AllowComments
        {
            get
            {
                return Data.allow_comments;
            }
        }
        public string ChannelName
        {
            get
            {
                return Id.Substring(0,Id.IndexOf('/'));
            }
        }
        public string Name
        {
            get
            {
                return Data.name;
            }
        }
        public string Goal
        {
            get
            {
                try
                {
                    return Data.goal.name;
                }
                catch
                {
                    return null;
                }
            }
        }
        public string Info
        {
            get
            {
                try
                {
                    return Data.info;
                }
                catch
                {
                    return null;
                }
            }
        }
        public TimeSpan StartDelay
        {
            get
            {
                try
                {
                    TimeSpan ts = XmlConvert.ToTimeSpan(Data.start_delay);
                    return ts;
                }
                catch
                {
                    return TimeSpan.Zero;
                }
            }
        }
        public int NumEntrants
        {
            get
            {
                return Data.entrants_count;
            }
        }
        public IEnumerable<RacetimeUser> Entrants
        {
            get
            {
                foreach(var e in Data.entrants)
                {
                    yield return RTModelBase.Create<RacetimeUser>(EntrantToUserConverter(e));
                }
            }
        }
        public string GameSlug
        {
            get
            {
                return Id.Substring(Id.IndexOf('/')+1);
            }
        }
        public RaceState State
        {
            get
            {
                switch(Data.status.value)
                {
                    case "open": return RaceState.Open;
                    case "pending": return RaceState.Starting;
                    case "in_progress": return RaceState.Started;
                    case "finished": return RaceState.Ended;
                    case "cancelled": return RaceState.Cancelled;
                    default: return RaceState.Unknown;
                }
            }
        }
        public DateTime StartedAt
        {
            get
            {
                try
                {
                    if (Data.started_at == null)
                        return DateTime.MaxValue;
                    return DateTime.Parse(Data.started_at).ToUniversalTime();
                }
                catch
                {
                    return DateTime.MaxValue;
                }
            }
        }
       
        
        public DateTime OpenedAt
        {
            get
            {
                try
                {
                    if (Data.opened_at == null)
                        return DateTime.MaxValue;
                    return DateTime.Parse(Data.opened_at);
                }
                catch
                {
                    return DateTime.MaxValue;
                }
            }
        }
        public RacetimeUser OpenedBy
        {
            get
            {
                return RTModelBase.Create<RacetimeUser>(Data.opened_by);
            }
        }

        public int Finishes => Entrants.Count(x => x.Status == UserStatus.Finished);
        public int Forfeits => Entrants.Count(x => x.Status == UserStatus.Forfeit || x.Status == UserStatus.Disqualified);

        public string GameId =>  Data.category.slug;

        public string GameName => Data.category.name;

        public string Id => Data.name;

        public IEnumerable<string> LiveStreams => Entrants.Where(x => x.Status != UserStatus.Forfeit && x.Status != UserStatus.Disqualified && !x.HasFinished).Select(x => x.TwitchName);

        public int Starttime => StartedAt == DateTime.MaxValue ? 0 : (int)(StartedAt - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        int IRaceInfo.State => State == RaceState.Open ? 1 : (State == RaceState.Started ? 3 : 42);


        public bool IsParticipant(string username)
        {
            return true;//Entrants.Any(x => x.Name.ToLower() == username?.ToLower());
        }
    }
}
