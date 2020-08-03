using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceCore
{
    public class BinanceInfoShort
    {
        public long Id { get; set; }
        public DateTime Time { get; set; }
        public string Symbol { get; set; }
        public decimal TradeQuantity { get; set; }
        public decimal RatePrice { get; set; }

        public BinanceInfoShort(DateTime time, string symbol, decimal tradeQuantity, decimal ratePrice, long id)
        {
            Id = id;
            Time = time;
            Symbol = symbol;
            TradeQuantity = tradeQuantity;
            RatePrice = ratePrice;
        }
    }
}
