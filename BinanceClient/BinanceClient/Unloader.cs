using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Binance.Net;
using Binance.Net.Objects.Spot.MarketData;

namespace BinanceClient
{
    public class Unloader
    {
        public IEnumerable<BinanceAggregatedTrade> GetTradesAndRates(Binance.Net.BinanceClient client, string symbol, DateTime start, DateTime end)
        {
            var aggTrades = client.GetAggregatedTrades(symbol, startTime: start, endTime: end, limit: 1000);
            if (aggTrades.Data == null)
                throw new Exception(aggTrades.Error.Message);
            return aggTrades.Data;
        }
    }
}
