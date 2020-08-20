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
        static Font font = System.DrawingCore.SystemFonts.DefaultFont;
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
        public static Bitmap MakeGraph(DateTime end, TimeSpan len, IEnumerable<Candle> candles, int w=640, int h=320)
        {
            {
                var hoursS = "24h";

                var start = end.Subtract(len);

                var vals = candles.Select(hh => hh.outV).ToList();
                vals.AddRange(candles.Select(hh => hh.inV));
                vals.AddRange(candles.Select(hh => hh.maxV));
                vals.AddRange(candles.Select(hh => hh.minV));
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
                int graphH = 160;


                int parts = 24 * 4;

                if (len.TotalDays == 7)
                {
                    hoursS = "7d";
                    parts = 7 * 12;
                }
                if (len.TotalDays == 30)
                {
                    hoursS = "30d";
                    parts = 30 * 4;
                }
                var priceAtPos = candles.Count() > 0 ? candles.First().inV : 0;

                int xPad = 5;

                Bitmap bmp = new Bitmap(w, h);

                double cW = w - xPad * 2;
                double partW = cW / parts;
                double candleW = partW - 2;


                using (var g = Graphics.FromImage(bmp))
                {
                    List<decimal> tradeSums = new List<decimal>();
                    DrawGraph(candles, max, min, graphH, parts, h, xPad, partW, g);
                    DrawSums(h, xPad, partW, candleW, g, tradeSums);
                    DrawLatestPrice(candles, g);

                    DrawDate(w, end, g);

                    Pen gridPen = new Pen(Color.FromArgb(30, Color.White)); //  цвет линий процентной сетки
                    DrawDaysGrid(end, len, w, h, start, xPad, g, gridPen);

                    DrawPercentGrid(w, h, max, min, graphH, g, gridPen);

                    DrawMinMax(candles, w, h, hoursS, g, -4);
                }


                return bmp;
            }
        }

        private static void DrawDate(int w, DateTime end2, Graphics g)
        {
            var dateS = end2.ToString("dd.MM.yyyy HH:mm UTC");
            var dateStringSize = g.MeasureString(dateS, font);
            g.DrawString($"{dateS}", font, Brushes.White, w - dateStringSize.Width, 22);
        }

        static SolidBrush FillColor = new SolidBrush(Color.FromArgb(255, 7, 190, 170));// Brushes.DarkBlue;
        static SolidBrush AntiFillColor = new SolidBrush(Color.FromArgb(255, 247, 50, 91));// Brushes.DarkBlue;
        static SolidBrush sumColor = new SolidBrush(Color.FromArgb(100, 255, 255, 255));


        private static void DrawLatestPrice(IEnumerable<Candle> candles, Graphics g)
        {
            var curVal = candles.Count() > 0 ? candles.Last().outV : 0;
            string valPrice = ValPrice(curVal);
            var priceColor = FillColor;
            if (candles.Count() > 1 && candles.Last().outV < candles.ElementAt(candles.Count() - 2).outV)
                priceColor = AntiFillColor;
            string priceString = $"≈ {valPrice}";
            SizeF coinNameSize = new SizeF(100, 10);
            g.DrawString(priceString, font, priceColor, coinNameSize.Width, 18 - 8);
        }

        private static void DrawSums(int h, int xPad, double partW, double candleW, Graphics g, List<decimal> tradeSums)
        {
            if (tradeSums.Count == 0) return;
            var maxTrade = tradeSums.Max();

            var sumsMaxHeight = 25;

            int ti = 0;
            while (ti < tradeSums.Count())
            {
                var sumP = ValueToPos(maxTrade, 0, sumsMaxHeight, tradeSums[ti]);
                if (sumP < 1) sumP = 1;
                int cX = (int)(xPad + (ti + 0.5F) * partW);

                var rect = new Rectangle((int)(cX - candleW / 2), h - (int)(sumP) - 50, (int)(candleW), (int)sumP);
                g.FillRectangle(sumColor, rect);

                ++ti;
            }
        }

        private static void DrawPercentGrid(int w, int h, decimal max, decimal min, int graphH, Graphics g, Pen gridPen)
        {
            decimal mid = (max + min) / 2;
            decimal plusPercent = mid * 1.01m;
            decimal minusPercent = mid - (plusPercent - mid);
            var plusLevel = (int)ValueToPos(max, min, graphH, plusPercent);
            var minusLevel = (int)ValueToPos(max, min, graphH, minusPercent);
            var midLevel = (int)ValueToPos(max, min, graphH, mid);
            double percentValue = -ValueToPos(max, min, graphH, mid) + ValueToPos(max, min, graphH, plusPercent);


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
        }

        private static void DrawDaysGrid(DateTime end, TimeSpan len, int w, int h, DateTime start,int xPad, Graphics g, Pen gridPen)
        {
            if (len.TotalHours == 24 * 7)
            {
                var pos = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0).AddHours(1);
                while (pos < end.AddHours(3))
                {
                    int cX = (int)(xPad + (pos.Subtract(start).TotalHours * (w - xPad * 2) / 24 / 7));

                    g.DrawLine(gridPen, (int)(cX), (int)(h / 4), (int)(cX), (int)(h * 3 / 4));
                    pos = pos.AddDays(1);
                }

            }

            if (len.TotalHours == 24 * 30)
            {
                var pos = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0).AddHours(1);
                while (pos < end.AddHours(3))
                {
                    int cX = (int)(xPad + (pos.Subtract(start).TotalHours * (w - xPad * 2) / 24 / 30));

                    g.DrawLine(gridPen, (int)(cX), (int)(h / 4), (int)(cX), (int)(h * 3 / 4));
                    pos = pos.AddDays(1);
                }

            }
        }

        private static void DrawMinMax(IEnumerable<Candle> candles, int w, int h, string hoursS, Graphics g, int minMaxTop)
        {
            var maxS = candles.Count() > 0 ? $" {hoursS} = {ValPrice(candles.Select(h => h.outV).Max())}" : "";
            var maxSSize = g.MeasureString(maxS, font);
            var maxSSize2 = g.MeasureString("MAX", font);
            g.DrawString("MAX", font, Brushes.White, w - 4 - maxSSize.Width - maxSSize2.Width, h - 10 - maxSSize.Height + minMaxTop);
            g.DrawString(maxS, font, Brushes.White, w - 4 - maxSSize.Width, h - 10 - maxSSize.Height + minMaxTop);
            g.DrawString($"MIN", font, Brushes.White, 8, h - 10 - maxSSize.Height + minMaxTop);
            var minS = candles.Count() > 0 ? $" {hoursS} = {ValPrice(candles.Select(h => h.outV).Min())}" : "";
            var minSSize = g.MeasureString(minS, font);
            g.DrawString(minS, font, Brushes.White, 10 + 50, h - 10 - maxSSize.Height + minMaxTop);
        }

        private static void DrawGraph(IEnumerable<Candle> candless, decimal max, decimal min, int graphH, int parts, int h, int xPad, double partW, Graphics g)
        {
            lock (candless)
            {
                double candleW = partW - 2;
                var FillColor = new SolidBrush(Color.FromArgb(255, 7, 190, 170));// Brushes.DarkBlue;
                var AntiFillColor = new SolidBrush(Color.FromArgb(255, 247, 50, 91));// Brushes.DarkBlue;
                var candles = candless.ToArray();
                for(int n=0;n<parts && n<candles.Count();n++)
                {
                    var candle = candles[n];

                    var inP = h - ValueToPos(max, min, graphH, candle.inV) - 80;
                    var outP = h - ValueToPos(max, min, graphH, candle.outV) - 80;
                    var minP = h - ValueToPos(max, min, graphH, candle.minV) - 80;
                    var maxP = h - ValueToPos(max, min, graphH, candle.maxV) - 80;

                    int cX = (int)(xPad + (n + 0.5F) * partW);

                    var candleH = (int)(outP - inP);
                    SolidBrush c = AntiFillColor;

                    if (candleH < 0)
                    {
                        inP = outP;
                        candleH = -candleH;
                        c = FillColor;
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
                }
            }
        }


    }
}
