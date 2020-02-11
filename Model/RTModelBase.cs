using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Model
{
    public abstract class RTModelBase
    {
        public DateTime Received { get; set; }

        public static T Create<T>(dynamic dataroot) where T : RTModelBase, new()
        {
            if (dataroot == null)
                return null;

            T item = new T();
            item.Received = DateTime.Now;
            item.Data = dataroot;

            return item;
        }

        public dynamic Data { get; set; }

        public RTModelBase()
        {

        }

    }
}
