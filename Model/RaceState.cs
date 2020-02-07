using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Model
{
    public enum RaceState
    {
        Unknown,
        Open,
        OpenInviteOnly,
        Ready,
        Starting,
        Started,
        Ended,
        Cancelled
    }
}
