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
        public string Key;
        public string Dbpath;
        public string Token;
        public long Master;
    }
}
