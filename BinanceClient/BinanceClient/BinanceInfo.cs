using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceClient
{
    public class BinanceInfo
    {
        public DateTime Time { get; set; }
        public decimal TradeQuantity { get; set; }
        public decimal RatePrice { get; set; }

        public BinanceInfo(DateTime time, decimal tradeQuantity, decimal ratePrice)
        {
            Time = time;
            TradeQuantity = tradeQuantity;
            RatePrice = ratePrice;
        }
    }
}
