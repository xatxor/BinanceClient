using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Binance.Net;

namespace BinanceClient
{
    public class Unloader
    {
        public void GetTradesAndRates(Binance.Net.BinanceClient client, string symbol, ref List<BinanceInfo> info, DateTime start, DateTime end)
        { 
            var aggTrades = client.GetAggregatedTrades(symbol, startTime: start, endTime: end, limit: 1000);
            if (aggTrades.Data != null)
                foreach (var t in aggTrades.Data)
                {
                    info.Add(new BinanceInfo(t.TradeTime, t.Quantity, t.Price));
                }
            else
                throw new Exception(aggTrades.Error.Message);
        }

    }
}
