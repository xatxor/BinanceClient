using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BinanceCore
{
    /// <summary>
    /// Расширения, которые исопльзуются в проекте BinanceCore
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Загрузка DrawingCore Bitmap в Image на экране
        /// </summary>
        /// <param name="iv">Image контрол</param>
        /// <param name="graphBmp">Картинка в формате DrawingCore</param>
        public static void LoadBitmap(this System.Windows.Controls.Image iv, Bitmap graphBmp)
        {
            var stream = new MemoryStream();
            graphBmp.Save(stream, System.DrawingCore.Imaging.ImageFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            iv.Source = bitmapImage;
        }
        /// <summary>
        /// Преобразование числа и даты в Tuple DateTime/decimal
        /// </summary>
        /// <param name="v">Число, которое надо датировать</param>
        /// <param name="t">Дата, к которой нужно привязать число</param>
        /// <returns></returns>
        public static Tuple<DateTime, decimal> at(this decimal v, DateTime t)
        {
            return new Tuple<DateTime, decimal>(t, v);
        }

        /// <summary>
        /// Преобразование строки в цвет (#ffffff -> Color)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static System.Windows.Media.Color ToColor(this string s)
        {
            return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(s);
        }

        /// <summary>
        /// Пытается загрузить целое число из строки текстбокса
        /// и подсвечивает текстбокс красным если строка не парсится
        /// </summary>
        /// <param name="input">текстбокс</param>
        /// <param name="output">приёмник числа</param>
        public static void TrySaveInt(this TextBox input, out int output)
        {
            if (!int.TryParse(input.Text, out output))
                input.Background = System.Windows.Media.Brushes.Pink;
            else
                input.Background = System.Windows.Media.Brushes.White;
        }

    }
}
