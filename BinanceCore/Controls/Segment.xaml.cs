using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BinanceCore
{
    /// <summary>
    /// Логика взаимодействия для Segment.xaml
    /// </summary>
    public partial class Segment : UserControl
    {
        public delegate void ChangedDgt();
        public event ChangedDgt Changed;

        public enum SegmentMode { STOP = 0, UP = 1, DOWN = 2 }
        string[] SIGNS = new string[] { "▬", "▲", "▼" };
        #region Входные параметры графика

        SegmentMode mode = SegmentMode.STOP;
        public SegmentMode Mode
        {
            get { return mode; }
            set { 
                mode = value;
                DrawGraph();
            }
        }
        int maxD = 10;
        /// <summary>
        /// Максимальный сдвиг курса в указанном Mode направлении (задаётся юзером)
        /// </summary>
        public int MaxD
        {
            get { return maxD; }
            set {
                    maxD = value;
                    maxDTB.Text = value.ToString();
                }
        }

        int minD = 10;
        /// <summary>
        /// Минимальный сдвиг курса в указанном Mode направлении (задаётся юзером)
        /// </summary>
        public int MinD
        {
            get { return minD; }
            set {
                minD = value;
                minDTB.Text = value.ToString(); 
            } 
        }

        double inMin = 0.5;
        /// <summary>
        /// Минимум возможных входящих значений (левая нижняя точка графика)
        /// </summary>
        public double InMin
        {
            get { return inMin; }
            set
            {
                inMin = value;
                DrawGraph();
            }
        }

        double inMax = 0.5;
        /// <summary>
        /// Максимум возможных входящих значений (левая верхняя точка графика)
        /// </summary>
        public double InMax
        {
            get { return inMax; }
            set
            {
                inMax = value;
                DrawGraph();
            }
        }


        public void SetModeByLetter(string letter)
        {
            switch (letter)
            {
                case "D":
                    Mode = SegmentMode.DOWN;
                    break;
                case "U":
                    Mode = SegmentMode.UP;
                    break;
                default:
                    Mode = SegmentMode.STOP;
                    break;
            }
        }
        #endregion

        #region Выходные значения графика
        double outMin = 0.4;
        /// <summary>
        /// Минимум возможных выходных значений (правая нижняя точка графика)
        /// </summary>
        public double OutMin
        {
            get { return outMin; }
            set
            {
                outMin = value;
                Changed?.Invoke();
            }
        }

        double outMax = 0.7;
        /// <summary>
        /// Максимум возможных выходных значений (правая верхняя точка графика)
        /// </summary>
        public double OutMax
        {
            get { return outMax; }
            set
            {
                outMax = value;
                Changed?.Invoke();
            }
        }
        #endregion
        /// <summary>
        /// Пытается превратить любую пару значений в Point(X,Y)
        /// </summary>
        /// <param name="x">число или строка с координатой (можно дробную)</param>
        /// <param name="y">число или строка с координатой (можно дробную)</param>
        /// <returns>Точка по заданным координатам</returns>
        Point MakePoint(object x, object y)
        {
            try
            {
                double xd = double.Parse(x.ToString());
                double yd = double.Parse(y.ToString());
                return new Point((int)xd, (int)yd);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Making Point from X:{(x == null ? "NULL" : x)}, Y:{(y == null ? "NULL" : y)}", ex);
            }
        }
        public void DrawGraph()
        {
            if (graphB != null && graphB.RenderSize.Width>0)
            {
                var h = graphB.RenderSize.Height;
                var w = graphB.RenderSize.Width;
                pathStart.StartPoint = MakePoint(0, h - InMin * h);
                pathP2.Point = MakePoint(0, h - InMax * h);
                pathP3.Point = MakePoint(w - 1, h - OutMax * h);
                pathP4.Point = MakePoint(w - 1, h - OutMin * h);
                graphB.Content = SIGNS[(int)Mode];
            }
        }

        /// <summary>
        /// Обновляет значения на выходах в соответствии со значениями на входах и режимом графика
        /// </summary>
        private void UpdateOuts(bool silent = false)
        {
            var oldHandler = Changed;
            if (silent)
                Changed = null;

            switch (mode)
            {
                case SegmentMode.STOP:
                    OutMax = InMax + MaxD / 1000F;  //  при стопе график может вырасти не более чем на верхнее число
                    OutMin = InMin - MinD / 1000F;  //  или упасть не более чем на нижнее число
                    break;

                case SegmentMode.UP:
                    OutMax = InMax + MaxD / 1000F;  //  при росте график может вырасти не более чем на верхнее число
                    OutMin = InMin + MinD / 1000F;  //  и не менее, чем на нижнее
                    break;

                case SegmentMode.DOWN:
                    OutMax = InMax - MinD / 1000F;  //  при падении график может упасть сверху не более, чем на нижнее число
                    OutMin = InMin - MaxD / 1000F;  //  или упасть снизу не далее, чем на верхнее
                    break;
            }
            if (silent)
                Changed = oldHandler;

        }

        public Segment()
        {
            InitializeComponent();
            Redraw();
        }

        private void graphB_Click(object sender, RoutedEventArgs e)
        {
            int maxMode = SIGNS.Length - 1; //  выясним максимально возможный номер режима
            if ((int)Mode < maxMode) ++Mode;//  если сейчас ещё не максимальный номер, то увеличим номер режима
            else Mode = 0;                  //  если уже последний режим, то возвращаемся к нулевому
            Redraw();
        }

        private void maxDTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            maxDTB.TrySaveInt(out maxD);
            Redraw();
        }

        private void minDTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            minDTB.TrySaveInt(out minD);
            Redraw();
        }

        private void Redraw()
        {
            UpdateOuts();
            DrawGraph();
        }


    }
}
