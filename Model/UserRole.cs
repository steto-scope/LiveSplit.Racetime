using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Model
{
    public enum UserRole : int
    {
        Unknown = 0,
        Anonymous = 1,
        Regular = 2,
        ChannelCreator = 4,
        Monitor = 8,
        Moderator = 16,
        Staff = 32,
        Bot = 64,
        System = 128
    }
}