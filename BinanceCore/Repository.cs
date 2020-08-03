using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinanceCore
{
    class Repository
    {
        public IEnumerable<BinanceInfo> GetRangeOfElementsByTime(DateTime time1, DateTime time2, string symbol, bool shortData)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                if (shortData)
                    return context.BinanceInfoShort.Where(e => (e.Time > time1 && e.Time < time2 && e.Symbol == symbol))
                            .Select(bis =>
                                new BinanceInfo(bis.Time, bis.Symbol, bis.TradeQuantity, bis.RatePrice, bis.Id)).ToArray();
                else
                    return context.BinanceInfo.Where(e => (e.Time > time1 && e.Time < time2 && e.Symbol == symbol)).ToArray();
            }
        }
    }
}
