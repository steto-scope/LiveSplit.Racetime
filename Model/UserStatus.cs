using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Model
{
    public enum UserStatus
    {
        Unknown,
        NotInRace,
        NotReady,
        Ready,
        Finished,
        Disqualified,
        Forfeit,
        Racing
    }
}
