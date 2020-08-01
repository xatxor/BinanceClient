using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BinanceCore.Services
{
    public static class Coding
    {
        public static string MakeCode(DateTime end, TimeSpan len, List<Tuple<DateTime, decimal>> history)
        {
            var timeShift = 0;
            var start = end.Subtract(len);
            var start2 = start.AddHours(timeShift);
            var end2 = end.AddHours(timeShift);

            var vals = history.Select(hh => hh.Item2);
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
            var pos = start;
            var pos2 = start2;
            var priceAtPos = history.Count() > 0 ? history.First().Item2 : 0;

            LatestCode= MakeCode(end, history, stepLen, parts, pos, pos2, ref priceAtPos);
            return LatestCode;
        }

        private static string MakeCode(DateTime end, List<Tuple<DateTime, decimal>> history, TimeSpan stepLen, int parts, DateTime pos, DateTime pos2, ref decimal priceAtPos)
        {
            string fractalCode = "";
            int n = 0;
            while (pos < end && n < parts)
            {
                var inV = priceAtPos;
                var inRange = history.Where(k => k.Item1 >= pos && k.Item1 < pos.Add(stepLen));

                var outV = priceAtPos;

                if (inRange.Count() > 0)
                    outV = inRange.Last().Item2;

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
                pos2 += stepLen;
                priceAtPos = outV;

            }
            Console.WriteLine("Now fractal is " + fractalCode);
            return fractalCode;
        }
        public static string LatestCode = "";

    }
}
