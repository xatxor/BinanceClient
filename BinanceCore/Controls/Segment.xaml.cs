using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace BinanceCore
{
    /// <summary>
    /// Редактор сегмента позволяет настроить числовые параметры и режим работы (рост, падение или плоский курс)
    /// и отображает, диапазон возможного хода графика на сегменте.
    /// </summary>
    public partial class Segment : UserControl, INotifyPropertyChanged
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
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Режимы работы сегмента - рост курса, падение или стоп - плоский курс
        /// </summary>
        public enum SegmentMode { STOP = 0, UP = 1, DOWN = 2 }

        /// <summary>
        /// Символики режимов, которые используются на экране
        /// </summary>
        string[] SIGNS = new string[] { "▬", "▲", "▼" };

        public string ModeSign
        {
            get => SIGNS[(int)Mode];
        }

        #region Входные параметры графика

        SegmentMode mode = SegmentMode.STOP;
        /// <summary>
        /// Режим сегмента - растущий или падающий или не растёт и не падает
        /// </summary>
        public SegmentMode Mode
        {
            get { return mode; }
            set {
                if (mode == value) return;
                mode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Mode"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ModeSign"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMin"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMax"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMinP"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMaxP"));
            }
        }

        string title;
        public string Title
        {
            get => title;
            set
            {
                if (title == value) return;
                title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title"));
            }
        }

        double maxD = 10;
        /// <summary>
        /// Максимальный сдвиг курса в указанном Mode направлении (задаётся юзером)
        /// </summary>
        public double MaxD
        {
            get { return maxD; }
            set {
                    if (maxD == value || h == 0) return;
                    maxD = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxD"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMin"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMax"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMinP"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMaxP"));
            }
        }

        double minD = 10;
        /// <summary>
        /// Минимальный сдвиг курса в указанном Mode направлении (задаётся юзером)
        /// </summary>
        public double MinD
        {
            get { return minD; }
            set {
                if (minD == value || h == 0) return;
                minD = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MinD"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMin"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMax"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMinP"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMaxP"));
            }
        }

        double inMin = double.NaN;
        /// <summary>
        /// Минимум возможных входящих значений (левая нижняя точка графика)
        /// </summary>
        public double InMin
        {
            get { return inMin; }
            set
            {
                if (inMin == value) return;
                inMin = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InMin"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InMinP"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMin"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMinP"));
            }
        }

        double inMax = double.NaN;
        /// <summary>
        /// Максимум возможных входящих значений (левая верхняя точка графика)
        /// </summary>
        public double InMax
        {
            get { return inMax; }
            set
            {
                if (inMax == value) return;
                inMax = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InMax"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InMaxP"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMax"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMaxP"));

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
        /// <summary>
        /// Минимум возможных выходных значений (правая нижняя точка графика)
        /// </summary>
        public double OutMin => Mode == SegmentMode.DOWN ? (InMin - MaxD / Divisor) : ((Mode==SegmentMode.UP ? (InMin + MinD / Divisor): (InMin - MinD / Divisor)));

        /// <summary>
        /// Максимум возможных выходных значений (правая верхняя точка графика)
        /// </summary>
        public double OutMax => Mode == SegmentMode.DOWN ? (InMax - MinD / Divisor) : ((Mode==SegmentMode.UP? (InMax + MaxD / Divisor):(InMax + MaxD / Divisor)));

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

        public double h => 54;// graphB.RenderSize.Height;
        public double w => 54;// graphB.RenderSize.Width;

        public Point InMinP => MakePoint(0.5, h - InMin * h);
        public Point InMaxP => MakePoint(0, h - InMax * h);
        public Point OutMinP => MakePoint(w-1, h - OutMin * h);
        public Point OutMaxP => MakePoint(w-1, h - OutMax * h);

        public Segment()
        {
            InitializeComponent();
            graphB.Click+=(b,a) => Mode = Mode > 0 ? --Mode : SegmentMode.DOWN; //  циклическое переключение режимов при нажатии на кнопку режима (она же отображает символ режима)
            TopLevelController.DataContext = this;  //  чтобы работали биндинги
        }




    }
}
