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
        public RacetimeUser User { get; set; }
        public DateTime Posted { get; set; }
        public bool Highlight { get; set; }

        protected ChatMessage(MessageType type)
        {
            Type = type;
        }

        public static ChatMessage CreateUser(string msg, RacetimeUser user, DateTime? posted)
        {
            if (!string.IsNullOrEmpty(msg) && user != null)
                return new ChatMessage(MessageType.User) { Message = msg, User = user, Posted = posted.HasValue ? posted.Value : DateTime.Now });
            return null;
        }
        public static ChatMessage CreateError(string msg, DateTime? posted)
        {
            if (!string.IsNullOrEmpty(msg))
                return new ChatMessage(MessageType.Error) { Message = msg, User = RacetimeUser.RaceBot, Posted = posted.HasValue ? posted.Value : DateTime.Now };
            return null;
        }

        public static ChatMessage CreateRace(Race race, DateTime? posted)
        {
            if (race != null)
                return new ChatMessage(MessageType.Error) { Race = race, User = RacetimeUser.RaceBot, Posted = posted.HasValue ? posted.Value : DateTime.Now };
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
                    yield return CreateUser(m.message.message, userlist.FirstOrDefault(u => u.Id == m.message.user.id) ?? RacetimeUser.Anonymous, null);
                    break;
            }
            yield break;
        }

    }
}
