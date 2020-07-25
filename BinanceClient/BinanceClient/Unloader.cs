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
        public void GetTradesAndRates(Binance.Net.BinanceClient client, string symbol, ref List<Tuple<DateTime, decimal>> trades, ref List<Tuple<DateTime, decimal>> rates, DateTime start, DateTime end)
        { 
            var aggTrades = client.GetAggregatedTrades(symbol, startTime: start, endTime: end, limit: 1000);
            foreach (var t in aggTrades.Data)
            {
                trades.Add(new Tuple<DateTime, decimal>(t.TradeTime,t.Quantity));
                rates.Add(new Tuple<DateTime, decimal>(t.TradeTime, t.Price));
            }
        }

    }
}
