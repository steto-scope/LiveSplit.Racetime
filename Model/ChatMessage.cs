using LiveSplit.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                try
                {
                    return Data.message_plain;
                }
                catch
                {
                    return Data.message;
                }
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
                        return Received;
                    return DateTime.Parse(Data.posted_at);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(Data.posted_at);

                    return DateTime.MaxValue;
                }
            }
        }
        public virtual bool Highlight
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
                posted_at = DateTime.Now.ToString(),
                highlight = important,
                is_system = true                   
            };
            return Create<LiveSplitMessage>(dataroot);
        }
    }
    public class SystemMessage : ChatMessage
    {
        public override MessageType Type => MessageType.System;

        public override RacetimeUser User
        {
            get
            {
                return RacetimeUser.System;
            }
        }

        public bool IsFinishingMessage
        {
            get
            {
                return Regex.IsMatch(Message, "(finish|forfeit|comment|done)", RegexOptions.IgnoreCase);
            }
        }
    }
    public class BotMessage : ChatMessage
    {
        public override MessageType Type => MessageType.Bot;

        public string BotName
        {
            get
            {
                try
                {
                    return Data.bot;
                }
                catch
                {
                    return null;
                }
            }
        }

        public override RacetimeUser User
        {
            get
            {
                return RacetimeUser.Bot;
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

        public override bool Highlight => true;

        public override RacetimeUser User
        {
            get
            {
                return RacetimeUser.System;
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
