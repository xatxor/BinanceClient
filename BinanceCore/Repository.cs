using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinanceCore
{
    class Repository
    {
        public IEnumerable<BinanceInfo> GetRangeOfElementsByTime(DateTime time1, DateTime time2, string symbol)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                var result = context.BinanceInfo.ToList().Where(e => (e.Time.CompareTo(time1) > 0) && (e.Time.CompareTo(time2) < 0) && (e.Symbol == symbol));
                return result;
            }
        }
    }
}
