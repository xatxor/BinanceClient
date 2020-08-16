using BinanceCore.Controls;
using BinanceCore.Services;

namespace BinanceCore.Entities
{
    class Project : AppSettings<Project>
    {
        public FractalDefinition[] fractals;
        public int interval;
        public string symbol;
        public decimal FailRise;
        public decimal FailFall;
        public decimal WinRise;
        public decimal WinFall;
        public decimal BasePrice;
        public Mode LastMode;
        /// <summary>
        /// Binance key
        /// </summary>
        public string Key;
        /// <summary>
        /// Binance secret
        /// </summary>
        public string Secret;
        /// <summary>
        /// Telegtram bot token
        /// </summary>
        public string Token;
        /// <summary>
        /// Telegram master id
        /// </summary>
        public long Master;

        public decimal StopBalance;
    }
}
