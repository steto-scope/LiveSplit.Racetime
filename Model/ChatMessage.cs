using LiveSplit.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Model
{
    public class ChatMessage
    {
        public MessageType Type;
        public Race Race { get; set; }
        public string Message { get; set; }
        public string User { get; set; }
        public DateTime Posted { get; set; }
        public bool Highlight { get; set; }
        public bool IsSystem { get; set; }

        protected ChatMessage(MessageType type)
        {
            Type = type;
        }

        public static ChatMessage CreateUser(string msg, string user, bool isSystem, bool isHighlight, DateTime? posted)
        {
            if (user == null)
                user = RacetimeUser.RaceBot.Name;
            if (!string.IsNullOrEmpty(msg) && user != null)
                return new ChatMessage(MessageType.User) { Message = msg, User = user, Posted = posted.HasValue ? posted.Value : DateTime.Now, Highlight=isHighlight, IsSystem=isSystem };
            return null;
        }
        public static ChatMessage CreateError(string msg, DateTime? posted)
        {
            if (!string.IsNullOrEmpty(msg))
                return new ChatMessage(MessageType.Error) { Message = msg, User = RacetimeUser.RaceBot.Name, Posted = posted.HasValue ? posted.Value : DateTime.Now };
            return null;
        }

        public static ChatMessage CreateRace(Race race, DateTime? posted)
        {
            if (race != null)
                return new ChatMessage(MessageType.Race) { Race = race, User =null, Posted = posted.HasValue ? posted.Value : DateTime.Now };
            return null;
        }

        public static ChatMessage CreateSystem(string msg, DateTime? posted)
        {
            if (!string.IsNullOrEmpty(msg))
                return new ChatMessage(MessageType.System) { Message = msg, User = null, Posted = posted.HasValue ? posted.Value : DateTime.Now };
            return null;
        }

        public static IEnumerable<ChatMessage> Deserialize(dynamic m, IEnumerable<RacetimeUser> userlist)
        {
            switch (m.type)
            {
                case "error":
                    foreach (var msg in m.errors)
                        yield return CreateError(msg, null);
                    break;
                case "race.data":
                    yield return CreateRace(Race.Deserialize(m.race), null);
                    break;
                case "chat.message":
                    //RacetimeUser y = ;
                    //var user = RacetimeUser.Anonymous;
                    //if (userlist != null)
                    //   user = userlist?.FirstOrDefault(u => u.Id == m.message.user.id);
                    yield return CreateUser(m.message.message, m.message.user?.name, m.is_system == null ? false : m.is_system, m.highlight == null? false:m.highlight, null);
                    break;
                case "livesplit":
                    yield return CreateSystem(m.message, null);
                    break;
            }
            yield break;
        }

    }
}
