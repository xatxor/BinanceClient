using System;
using System.Windows;
using System.Windows.Controls;

namespace BinanceCore
{
    /// <summary>
    /// Редактор сегмента позволяет настроить числовые параметры и режим работы (рост, падение или плоский курс)
    /// и отображает, диапазон возможного хода графика на сегменте.
    /// </summary>
    public partial class Segment : UserControl
    {
        /// <summary>
        /// Делитель процентов в настройках, то есть если в настройках рост на 100, а делитель 1000, значит настроен рост на 0.1
        /// </summary>
        public float Divisor = 1000F;

        public delegate void ChangedDgt();
        /// <summary>
        /// Событие возникает если изменились параметры работы сегмента (минимум, максимум изменения или режим)
        /// </summary>
        public event ChangedDgt Changed;

        /// <summary>
        /// Режимы работы сегмента - рост курса, падение или стоп - плоский курс
        /// </summary>
        public enum SegmentMode { STOP = 0, UP = 1, DOWN = 2 }

        /// <summary>
        /// Символики режимов, которые используются на экране
        /// </summary>
        string[] SIGNS = new string[] { "▬", "▲", "▼" };


        #region Входные параметры графика

        SegmentMode mode = SegmentMode.STOP;
        /// <summary>
        /// Режим сегмента - растущий или падающий или не растёт и не падает
        /// </summary>
        public SegmentMode Mode
        {
            get { return mode; }
            set { 
                mode = value;
                UpdateOuts();
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

        /// <summary>
        /// Установка режима сегмента по букве (используется при настройке по строковому коду)
        /// </summary>
        /// <param name="letter"></param>
        public void SetModeByLetter(string letter)
        {
            switch (letter)
            {
                case "D":   Mode = SegmentMode.DOWN;    break;
                case "U":   Mode = SegmentMode.UP;      break;
                default:    Mode = SegmentMode.STOP;    break;
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
            if (graphB != null && graphB.RenderSize.Width>0)        //  Чит чтобы не рисовать когда окно не готово
            {
                var h = graphB.RenderSize.Height;                   //  определим размеры окошка, где рисуем
                var w = graphB.RenderSize.Width;
                pathStart.StartPoint = MakePoint(0, h - InMin * h); //  перенастроим точки многоугольника,
                pathP2.Point = MakePoint(0, h - InMax * h);         //  символизирующего возможности хода крурса
                pathP3.Point = MakePoint(w - 1, h - OutMax * h);    //  выставим точки слева на стартовый диапазон по InMin, InMax
                pathP4.Point = MakePoint(w - 1, h - OutMin * h);    //  и точки справа на выходной согласно посчитанны OutMax, OutMin
                graphB.Content = SIGNS[(int)Mode];  //  вывод символа роста, падения или плоского графика в кнопку на теле самого графика - если нажать эту кнопку, режим изменится
            }
        }

        /// <summary>
        /// Обновляет значения на выходах в соответствии со значениями на входах и режимом графика
        /// </summary>
        /// <param name="silent">Отключает обработчик события об изменении в полях ввода чтобы на экран отражалось сразу, но не зацикливалось эвентами обновления в поле ввода</param>
        private void UpdateOuts(bool silent = false)
        {
            var oldHandler = Changed;
            if (silent)
                Changed = null;

            switch (mode)
            {
                case SegmentMode.STOP:
                    OutMax = InMax + MaxD / Divisor;  //  при стопе график может вырасти не более чем на верхнее число
                    OutMin = InMin - MinD / Divisor;  //  или упасть не более чем на нижнее число
                    break;

                case SegmentMode.UP:
                    OutMax = InMax + MaxD / Divisor;  //  при росте график может вырасти не более чем на верхнее число
                    OutMin = InMin + MinD / Divisor;  //  и не менее, чем на нижнее
                    break;

                case SegmentMode.DOWN:
                    OutMax = InMax - MinD / Divisor;  //  при падении график может упасть сверху не более, чем на нижнее число
                    OutMin = InMin - MaxD / Divisor;  //  или упасть снизу не далее, чем на верхнее
                    break;
            }
            if (silent)
                Changed = oldHandler;

        }

        public Segment()
        {
            InitializeComponent();
            graphB.Click+=(b,a) => Mode = Mode > 0 ? --Mode : SegmentMode.DOWN; //  циклическое переключение режимов при нажатии на кнопку режима (она же отображает символ режима)
            DataContext = this;
            UpdateOuts();
            DrawGraph();
        }




    }
}
