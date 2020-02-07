using LiveSplit.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Model
{
    public abstract class ChatMessage : RTModelBase
    {
        public abstract MessageType Type { get; }

        public virtual string Message
        {
            get
            {
                return Data.message;
            }
        }
        public virtual RacetimeUser User
        {
            get
            {
                try
                {
                    return RTModelBase.Create<RacetimeUser>(Data.user);
                }
                catch
                {
                    return null;
                }
            }
        }
        public DateTime Posted
        {
            get
            {
                try
                {
                    if (Data.posted_at == null)
                        return DateTime.MaxValue;
                    return DateTime.Parse(Data.posted_at);
                }
                catch
                {
                    return DateTime.MaxValue;
                }
            }
        }
        public bool Highlight
        {
            get
            {
                try
                {
                    return Data.highlight;
                }
                catch
                {
                    return false;
                }
               
            }
        }
        public bool IsSystem
        {
            get
            {
                try
                {
                    return Data.is_system;
                }
                catch
                {
                    return false;
                }
                
            }
        }
        
        public static IEnumerable<ChatMessage> Parse(dynamic m)
        {
            Console.WriteLine(m.GetType().ToString() + m.ToString());
            switch (m.type)
            {
                case "error":
                    yield return RTModelBase.Create<ErrorMessage>(m);
                    break;
                case "race.data":
                    yield return RTModelBase.Create<RaceMessage>(m.race);
                    break;
                case "chat.message":
                    if(m.message.is_system)
                        yield return RTModelBase.Create<RaceBotMessage>(m.message);
                    else
                        yield return RTModelBase.Create<UserMessage>(m.message);
                    break;
                case "livesplit":
                    yield return RTModelBase.Create<LiveSplitMessage>(m.message);
                    break;
            }
            yield break;
        }

    }

    public class LiveSplitMessage : ChatMessage
    {
        public override MessageType Type => MessageType.LiveSplit;

        public override RacetimeUser User
        {
            get
            {
                return RacetimeUser.LiveSplit;
            }
        }

        public static LiveSplitMessage Create(string msg, bool important)
        {
            var dataroot = new
            {
                message = msg,
                user = RacetimeUser.LiveSplit,
                posted_at = DateTime.Now,
                highlight = important,
                is_system = true                   
            };
            return Create<LiveSplitMessage>(dataroot);
        }
    }
    public class RaceBotMessage : ChatMessage
    {
        public override MessageType Type => MessageType.RaceBot;

        public override RacetimeUser User
        {
            get
            {
                return RacetimeUser.RaceBot;
            }
        }
    }
    public class UserMessage : ChatMessage
    {
        public override MessageType Type => MessageType.User;
    }
    public class ErrorMessage : ChatMessage
    {
        public override MessageType Type => MessageType.Error;

        public override RacetimeUser User
        {
            get
            {
                return RacetimeUser.RaceBot;
            }
        }

        public override string Message
        {
            get
            {
                try
                {
                    string msg = "";
                    foreach(var s in Data.errors)
                        msg += s + " ";
                    return msg;
                }
                catch
                {
                    return "Error in the error message";
                }
            }
                
        }
    }
    public class RaceMessage : ChatMessage
    {
        public override MessageType Type => MessageType.Race;

        public Race Race
        {
            get
            {
                return RTModelBase.Create<Race>(Data);
            }
        }
    }
}
