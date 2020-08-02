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
                var result = context.BinanceInfo.Where(e => (e.Time>time1 && e.Time<time2 && e.Symbol == symbol)).ToList();
                return result;
            }
        }
    }
}
