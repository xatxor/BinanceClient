using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.Text;
using System.Linq;
using System.DrawingCore.Text;
using BinanceCore.Entities;
using System.Windows.Controls;
using Binance.Net.Objects.Spot.WalletData;

namespace BinanceCore.Services
{
    /// <summary>
    /// Рисует графики торгов и сигналы фракталов на битмапах и холстах
    /// </summary>
    class Drawing
    {
        public static int timeShift = -3;
        static Font coinFont = System.DrawingCore.SystemFonts.CaptionFont;
        static Font dateFont = System.DrawingCore.SystemFonts.DefaultFont;
        static Font priceFont = System.DrawingCore.SystemFonts.DefaultFont;
        static Font minMaxFont = System.DrawingCore.SystemFonts.DialogFont;
        static Font minMax2Font = System.DrawingCore.SystemFonts.DialogFont;
        static PrivateFontCollection foo;
        static PrivateFontCollection fooR;
        static PrivateFontCollection fooM;
        static PrivateFontCollection fooB;
        static float baseSize = 35;
        static float lessSize = baseSize * 32 / 42;
        static float miniSize = baseSize * 16 / 28F;
        /// <summary>
        /// Инициализирует шрифты, нужные для рисования графика
        /// </summary>
        private static void initFOnts()
        {
            // 'PrivateFontCollection' is in the 'System.Drawing.Text' namespace
            /*   if (foo == null)
               {
                   foo = new PrivateFontCollection();
                   // Provide the path to the font on the filesystem
                   foo.AddFontFile("coin.ttf");

                   fooR = new PrivateFontCollection();
                   fooR.AddFontFile("regular.ttf");
                   fooM = new PrivateFontCollection();
                   fooM.AddFontFile("medium.ttf");
                   fooB = new PrivateFontCollection();
                   fooB.AddFontFile("bold.ttf");
                   coinFont = new Font((FontFamily)fooB.Families[0], baseSize, GraphicsUnit.Pixel);
                   dateFont = new Font((FontFamily)fooR.Families[0], miniSize, GraphicsUnit.Pixel);
                   priceFont = new Font((FontFamily)fooM.Families[0], lessSize, GraphicsUnit.Pixel);
                   minMaxFont = new Font((FontFamily)fooM.Families[0], miniSize, GraphicsUnit.Pixel);
                   minMax2Font = new Font((FontFamily)fooR.Families[0], miniSize, GraphicsUnit.Pixel);
               }*/
        }
        /// <summary>
        /// Определяет вертикальное положение значения на графике
        /// </summary>
        /// <param name="max">Максимальное отображаемое на графике значение</param>
        /// <param name="min">Минимальное отображаемое на графике значение</param>
        /// <param name="h">Высота графика</param>
        /// <param name="inV">Значение, для которого нужно узнать высоту</param>
        /// <returns>Позиция значения по высоте на графике</returns>
        private static double ValueToPos(decimal max, decimal min, int h, decimal inV)
        {
            decimal v = inV;
            v -= min;
            decimal realD = max - min;
            if (realD == 0) return (double)(inV / 2);
            v /= realD;
            v *= h;
            return (double)v;
        }

        /// <summary>
        /// Добавляет на Canvas пометки фракталов
        /// </summary>
        /// <param name="config">Описание фрактала</param>
        /// <param name="symbolsAtStages"></param>
        /// <param name="canv1"></param>
        public static void DrawFoundFractals(/*FractalDefinition config, */List<Tuple<int, FractalDefinition>> symbolsAtStages, Canvas canv1)
        {
//            var fractalPartsCount = config.Code.Split(new char[] { ';' }).Length;

            double stepSize = 630F / 24 / 4;
            int xd = 4;
            foreach (var mark in symbolsAtStages)
            {
                int stage = mark.Item1;
                FractalDefinition fractal = mark.Item2;
                var fractalPartsCount = fractal.Code.Split(new char[] { ';' }).Length;

                var c = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(fractal.Color);
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle()
                {
                    Stroke = new System.Windows.Media.SolidColorBrush(),
                    Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(
                        (byte)(c.A / 4),
                        c.R,
                        c.G,
                        c.B)),
                    Width = stepSize * fractalPartsCount,
                    Height = canv1.RenderSize.Height,
                    StrokeThickness = 2
                };
                System.Windows.Controls.Canvas.SetLeft(rect, (int)(mark.Item1 * stepSize + xd));
                System.Windows.Controls.Canvas.SetTop(rect, 0);
                canv1.Children.Add(rect);
            }
        }

        private static string ValPrice(decimal curVal)
        {
            string valPrice = ((int)curVal).ToString();
            if (curVal < 1000)
                valPrice = curVal.ToString("0.#");
            if (curVal < 100)
                valPrice = curVal.ToString("0.##");
            if (curVal < 10)
                valPrice = curVal.ToString("0.###");
            if (curVal < 1)
                valPrice = curVal.ToString("0.####");
            if (curVal < (decimal)(0.1))
                valPrice = curVal.ToString("0.#####");
            if (curVal < (decimal)(0.01))
                valPrice = curVal.ToString("0.#######");
            return valPrice;
        }

        /// <summary>
        /// Создаёт Bitmap с графиком торгов (японские свечи и график объёмов под ними)
        /// </summary>
        /// <param name="CoinName">имя тикера для подписи</param>
        /// <param name="end">Момент окончания графика</param>
        /// <param name="len">Длительность графика</param>
        /// <param name="history">История курсов в Tuple DateTime/decimal/</param>
        /// <param name="trades">Объёмы торгов в Tuple DateTime/decimal</param>
        /// <param name="w">Ширина итоговой картинки</param>
        /// <param name="h">Высота итоговой картинки</param>
        /// <returns></returns>
        public static Bitmap MakeGraph(string CoinName, DateTime end, TimeSpan len, IEnumerable<BinanceInfo> BinanceInfo, int w=640, int h=320)
        {
            List<Tuple<DateTime, decimal>> history = new List<Tuple<DateTime, decimal>>();
            foreach (var item in BinanceInfo)
            {
                Tuple<DateTime, decimal> tuple = new Tuple<DateTime, decimal>(item.Time, item.RatePrice);
                history.Add(tuple);
            }
            initFOnts();
            var hoursS = "24h";

            timeShift = 0;
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
            /*            max += d / 1.8;
                        min -= d / 1.6;*/
            if (min < 0) min = 0;
            int graphH = 160;


            var stepLen = new TimeSpan(0, 15, 0);
            int parts = 24 * 4;

            if (len.TotalDays == 7)
            {
                stepLen = new TimeSpan(2, 0, 0);
                hoursS = "7d";
                parts = 7 * 12;
            }
            if (len.TotalDays == 30)
            {
                stepLen = new TimeSpan(6, 0, 0);
                hoursS = "30d";
                parts = 30 * 4;
            }
            var pos = start;
            var pos2 = start2;
            var priceAtPos = history.Count() > 0 ? history.First().Item2 : 0;


            int xPad = 5;

            Bitmap bmp = new Bitmap(w, h);

            double cW = w - xPad * 2;
            double partW = cW / parts;
            double candleW = partW - 2;
            var sumColor = new SolidBrush(Color.FromArgb(100, 255, 255, 255));
            var FillColor = new SolidBrush(Color.FromArgb(255, 7, 190, 170));// Brushes.DarkBlue;
            var AntiFillColor = new SolidBrush(Color.FromArgb(255, 247, 50, 91));// Brushes.DarkBlue;


            using (var g = Graphics.FromImage(bmp))
            {
                List<decimal> tradeSums = new List<decimal>();
                DrawGraph(end, BinanceInfo, max, min, graphH, stepLen, parts, pos, pos2, ref priceAtPos, h, xPad, partW, g, tradeSums);
                pos = start;
                var maxTrade = tradeSums.Max();

                var sumsMaxHeight = 25;

                int ti = 0;
                while (pos < end.AddHours(3) && ti < tradeSums.Count())
                {
                    var sumP = ValueToPos(maxTrade, 0, sumsMaxHeight, tradeSums[ti]);
                    if (sumP < 1) sumP = 1;
                    int cX = (int)(xPad + (ti + 0.5F) * partW);

                    var rect = new Rectangle((int)(cX - candleW / 2), h - (int)(sumP) - 50, (int)(candleW), (int)sumP);
                    g.FillRectangle(sumColor, rect);

                    ++ti;
                    pos += stepLen;
                }

                var curVal = history.Count() > 0 ? history.Last().Item2 : 0;
                string valPrice = ValPrice(curVal);
                var nameColor = FillColor;
                if (history.Count() > 1 && history.Last().Item2 < history.ElementAt(history.Count() - 2).Item2)
                    nameColor = AntiFillColor;
                string priceString = $"≈ {valPrice}";
                int titleSize = 1000;
                float divider = 1;
                SizeF coinNameSize = new SizeF(100, 10);
                while (titleSize > 380 && fooB != null)
                {
                    coinFont = new Font((FontFamily)fooB.Families[0], baseSize / divider, GraphicsUnit.Pixel);
                    dateFont = new Font((FontFamily)fooR.Families[0], miniSize / divider, GraphicsUnit.Pixel);
                    priceFont = new Font((FontFamily)fooM.Families[0], lessSize / divider, GraphicsUnit.Pixel);

                    coinNameSize = g.MeasureString(CoinName, coinFont);
                    var priceSize = g.MeasureString(priceString, priceFont);
                    titleSize = (int)(coinNameSize.Width + priceSize.Width);
                    divider *= 1.01F;
                }

                g.DrawString(CoinName, coinFont, nameColor, 5, 12 - 4);
                g.DrawString(priceString, priceFont, Brushes.White, coinNameSize.Width, 18 - 8);

                var dateS = end2.ToString("dd.MM.yyyy HH:mm UTC");
                var dateStringSize = g.MeasureString(dateS, dateFont);
                g.DrawString($"{dateS}", dateFont, Brushes.White, w - dateStringSize.Width, 22);

                int minMaxTop = -10 + 6;
                Pen gridPen = new Pen(Color.FromArgb(20, Color.White));
                decimal mid = (max + min) / 2;
                decimal plusPercent = mid * 1.01m;
                decimal minusPercent = mid - (plusPercent - mid);
                var plusLevel = (int)ValueToPos(max, min, graphH, plusPercent);
                var minusLevel = (int)ValueToPos(max, min, graphH, minusPercent);
                var midLevel = (int)ValueToPos(max, min, graphH, mid);
                double percentValue = -ValueToPos(max, min, graphH, mid) + ValueToPos(max, min, graphH, plusPercent);

                if (len.TotalHours == 24 * 7)
                {
                    pos = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0).AddHours(1 + timeShift);
                    while (pos < end.AddHours(3))
                    {
                        int cX = (int)(xPad + (pos.Subtract(start.AddHours(+timeShift)).TotalHours * (w - xPad * 2) / 24 / 7));

                        g.DrawLine(gridPen, (int)(cX), (int)(h / 4), (int)(cX), (int)(h * 3 / 4));
                        pos = pos.AddDays(1);
                    }

                }

                if (len.TotalHours == 24 * 30)
                {
                    pos = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0).AddHours(1 + timeShift);
                    while (pos < end.AddHours(3))
                    {
                        int cX = (int)(xPad + (pos.Subtract(start.AddHours(+timeShift)).TotalHours * (w - xPad * 2) / 24 / 30));

                        g.DrawLine(gridPen, (int)(cX), (int)(h / 4), (int)(cX), (int)(h * 3 / 4));
                        pos = pos.AddDays(1);
                    }

                }

                if (percentValue >= 5)
                {
                    g.DrawLine(gridPen, 0, midLevel + 79, w, midLevel + 79);
                    //                g.DrawLine(gridPen, 0, plusLevel+79, w, plusLevel+79);
                    //              g.DrawLine(gridPen, 0, minusLevel+79, w, minusLevel+79);

                    int p = 1;
                    while (percentValue * p < h / 4)
                    {
                        var add = (int)percentValue * p;
                        g.DrawLine(gridPen, 0, midLevel + add + 79, w, midLevel + add + 79);
                        g.DrawLine(gridPen, 0, midLevel - add + 79, w, midLevel - add + 79);
                        ++p;
                    }
                }

                var maxS = history.Count > 0 ? $" {hoursS} = {ValPrice(history.Select(h => h.Item2).Max())}" : "";
                var maxSSize = g.MeasureString(maxS, minMax2Font);
                var maxSSize2 = g.MeasureString("MAX", minMaxFont);
                g.DrawString("MAX", minMaxFont, Brushes.White, w - 4 - maxSSize.Width - maxSSize2.Width, h - 10 - maxSSize.Height + minMaxTop);
                g.DrawString(maxS, minMax2Font, Brushes.White, w - 4 - maxSSize.Width, h - 10 - maxSSize.Height + minMaxTop);
                g.DrawString($"MIN", minMaxFont, Brushes.White, 8, h - 10 - maxSSize.Height + minMaxTop);
                var minS = history.Count > 0 ? $" {hoursS} = {ValPrice(history.Select(h => h.Item2).Min())}" : "";
                var minSSize = g.MeasureString(minS, minMax2Font);
                g.DrawString(minS, minMax2Font, Brushes.White, 10 + 50, h - 10 - maxSSize.Height + minMaxTop);

            }


            return bmp;
        }
        private static string DrawGraph(DateTime end, IEnumerable<BinanceInfo> BinanceInfo, decimal max, decimal min, int graphH, TimeSpan stepLen, int parts, DateTime pos, DateTime pos2, ref decimal priceAtPos, int h, int xPad, double partW, Graphics g, List<decimal> tradeSums)
        {
            double candleW = partW - 2;

            var FillColor = new SolidBrush(Color.FromArgb(255, 7, 190, 170));// Brushes.DarkBlue;
            var AntiFillColor = new SolidBrush(Color.FromArgb(255, 247, 50, 91));// Brushes.DarkBlue;
            string fractalCode = "";
            int n = 0;
            while (pos < end && n < parts)
            {
                var inV = priceAtPos;
                var inRange = BinanceInfo.Where(k => k.Time>= pos && k.Time < pos.Add(stepLen));
                var tradesRange = BinanceInfo.Where(k => k.Time >= pos2 && k.Time < pos2.Add(stepLen));
                var tradeSum = tradesRange.Select(tr => tr.TradeQuantity).Sum();
                tradeSums.Add(tradeSum);

                var outV = priceAtPos;

                if (inRange.Count() > 0)
                    outV = inRange.Last().RatePrice;


                var minV = Math.Min(inV, outV);
                if (inRange.Count() > 0)
                    minV = inRange.Select(r => r.RatePrice).Min();

                var maxV = Math.Max(inV, outV);
                if (inRange.Count() > 0)
                    maxV = inRange.Select(r => r.RatePrice).Max();
                var inP = h - ValueToPos(max, min, graphH, inV) - 80;
                var outP = h - ValueToPos(max, min, graphH, outV) - 80;
                var minP = h - ValueToPos(max, min, graphH, minV) - 80;
                var maxP = h - ValueToPos(max, min, graphH, maxV) - 80;

                int cX = (int)(xPad + (n + 0.5F) * partW);

                var candleH = (int)(outP - inP);
                /*                    if (candleH >= 0 && candleH < 1) candleH = 1;
                                    if (candleH < 0 && candleH > -1) candleH = -1;*/
                string fractalStage = "S";
                int percent = 0;
                SolidBrush c = FillColor;
                if (inV > outV)
                {
                    c = AntiFillColor;  //  рост или падение
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

                if (candleH < 0)
                {
                    inP = outP;
                    candleH = -candleH;
                }

                if (maxP < minP)
                {
                    var t = minP;
                    minP = maxP;
                    maxP = t;
                }

                var rect = new Rectangle((int)(cX - candleW / 2), (int)(inP) - 1, (int)(candleW), candleH + 2);
                g.FillRectangle(c, rect);
                var rect2 = new Rectangle((int)(cX - 1), (int)(minP), (int)(1), (int)(maxP - minP));
                g.FillRectangle(c, rect2);

                ++n;
                pos += stepLen;
                pos2 += stepLen;
                priceAtPos = outV;

            }
            Console.WriteLine("Now fractal is " + fractalCode);
            return fractalCode;
        }


    }
}
