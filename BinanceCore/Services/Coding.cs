using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BinanceCore.Services
{
    public class Candle
    {
        public decimal inV;
        public decimal outV;
        public decimal minV;
        public decimal maxV;
        public DateTime start;
        public TimeSpan len;
    }
    public static class Coding
    {
        public static string MakeCode(DateTime end, TimeSpan len, IEnumerable<BinanceInfo> BinanceInfo)
        {
            var start = end.Subtract(len);

            var vals = BinanceInfo.Select(hh => hh.RatePrice);
            var max = vals.Count() > 0 ? vals.Max() : 0;
            var min = vals.Count() > 0 ? vals.Min() : 0;
            var d = max - min;
            if (d == 0)
            {
                d = max / 2;
                min = max - d * (decimal).03;
                max = max + d * (decimal).03;
            }
            if (min < 0) min = 0;

            #region Настройка шагов графика на варианты длины 1, 7 и 30 дней по TotalDays
            var stepLen = new TimeSpan(0, 15, 0);
            int parts = 24 * 4;
            if (len.TotalDays == 7)
            {
                stepLen = new TimeSpan(2, 0, 0);
                parts = 7 * 12;
            }
            if (len.TotalDays == 30)
            {
                stepLen = new TimeSpan(6, 0, 0);
                parts = 30 * 4;
            }
            #endregion

            var pos = start;
            var priceAtPos = BinanceInfo.Count() > 0 ? BinanceInfo.First().RatePrice : 0;

            LatestCode= MakeCode(end, BinanceInfo, stepLen, parts, pos, ref priceAtPos);
            return LatestCode;
        }

        private static string MakeCode(DateTime end, IEnumerable<BinanceInfo> BinanceInfo, TimeSpan stepLen, int parts, DateTime pos, ref decimal priceAtPos)
        {
            lock (BinanceInfo)
            {
                string fractalCode = "";
                int n = 0;
                while (pos < end && n < parts)
                {
                    var inV = priceAtPos;
                    var inRange = BinanceInfo.Where(k => k.Time >= pos && k.Time < pos.Add(stepLen));

                    var outV = priceAtPos;

                    if (inRange.Count() > 0)
                        outV = inRange.Last().RatePrice;

                    string fractalStage = "S";
                    int percent = 0;

                    if (inV > outV)
                    {
                        fractalStage = "D";
                        percent = (int)(-10000 + inV / outV * 10000);
                    }
                    else if (outV > inV)
                    {
                        fractalStage = "U";
                        percent = (int)(-10000 + outV / inV * 10000);
                    }
                    fractalStage += percent.ToString("00 ");
                    fractalCode += fractalStage;

                    ++n;
                    pos += stepLen;
                    priceAtPos = outV;

                }
                Console.WriteLine("Now fractal is " + fractalCode);
                return fractalCode;
            }
        }
        public static IEnumerable<Candle> MakeCandles(DateTime end, IEnumerable<BinanceInfo> BinanceInfo, TimeSpan stepLen, int parts)
        {
            if (BinanceInfo.Count() == 0) yield break;
            lock (BinanceInfo)
            {
                var pos = end.Subtract(stepLen * parts);
                int n = 0;
                while (pos < end && n < parts)
                {
                    var inRange = BinanceInfo.Where(k => k.Time >= pos && k.Time < pos.Add(stepLen));
                    if (inRange.Count() > 0)
                    {
                        var priceAtPos = inRange.First().RatePrice;
                        var inV = priceAtPos;
                        var outV = priceAtPos;

                        if (inRange.Count() > 0)
                            outV = inRange.Last().RatePrice;

                        priceAtPos = outV;
                        var prices = inRange.Select(bi => bi.RatePrice);
                        yield return new Candle()
                        {
                            start = pos,
                            len = stepLen,
                            inV = inV,
                            maxV = prices.Max(),
                            minV = prices.Min(),
                            outV = outV
                        };
                    }
                    ++n;
                    pos += stepLen;
                }
            }
        }
        public static string LatestCode = "";

    }
}
