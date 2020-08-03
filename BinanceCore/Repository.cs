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
                var ieshortinfo = context.BinanceInfoShort.Where(e => (e.Time>time1 && e.Time<time2 && e.Symbol == symbol)).ToList();
                List<BinanceInfo> result = new List<BinanceInfo>();
                foreach (var item in ieshortinfo)
                {
                    var info = new BinanceInfo(item.Time, item.Symbol, item.TradeQuantity, item.RatePrice);
                    result.Add(info);
                }

                return result;
            }
        }
    }
}
