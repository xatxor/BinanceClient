using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceClient
{
    public class BinanceInfo
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string Symbol { get; set; }
        public int TradeQuantity { get; set; }
        public decimal RatePrice { get; set; }

        public BinanceInfo(DateTime time, string symbol, int tradeQuantity, decimal ratePrice)
        {
            Symbol = symbol;
            Time = time;
            TradeQuantity = tradeQuantity;
            RatePrice = ratePrice;
        }
    }
}
