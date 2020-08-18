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
        /// Размер графика сегмента по умолчанию (длина и ширина)
        /// Используется когда график ещё не на экране, и узнать его размер нельзя,
        /// но прикинуть данные уже нужно.
        /// </summary>
        const int DEFAULT_GRAPH_SIZE = 54;
        /// <summary>
        /// Делитель процентов в настройках, то есть если в настройках рост на 100, а делитель 1000, значит настроен рост на 0.1
        /// </summary>
        public float Divisor = 100F;
        /// <summary>
        /// Событие, при помощи которого обслуживаются наблюдения за свойствами класса
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Режимы работы сегмента - рост курса, падение или стоп - плоский курс
        /// </summary>
        public enum SegmentMode { STOP = 0, UP = 1, DOWN = 2 }

        /// <summary>
        /// Символики режимов, которые используются на экране
        /// </summary>
        string[] SIGNS = new string[] { "▬", "▲", "▼" };
        /// <summary>
        /// Символ режима сегмента
        /// </summary>
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
            set
            {
                if (mode == value) return;
                mode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Mode"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ModeSign"));
                NotifyOutMax();
                NotifyOutMin();
                UpdateNext();
            }
        }
        /// <summary>
        /// Автоматическое обновление следующих по очереди сегментов
        /// если этот сегмент расположен на панели с другими сегментами
        /// </summary>
        private void UpdateNext()
        {
            try // Подразумевается, что сегмент должен быть на панели среди других сегментов, и ничего другого на панели быть не должно, но что-то может быть не так
            {
                var parentControls = (Parent as Panel).Children;
                var thisIndex = parentControls.IndexOf(this);
                if (thisIndex < parentControls.Count - 1)
                {
                    (parentControls[thisIndex + 1] as Segment).InMin = this.OutMin;
                    (parentControls[thisIndex + 1] as Segment).InMax = this.OutMax;
                }
            }
            finally { }
        }

        string title;
        /// <summary>
        /// Подпись слева сверху - имя сегмента
        /// </summary>
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

        double maxD = double.NaN;
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
                NotifyOutMax();
                NotifyOutMin();
                UpdateNext();
            }
        }

        double minD = double.NaN;
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
                NotifyOutMax();
                NotifyOutMin();
                UpdateNext();
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
                NotifyOutMin();
                UpdateNext();
            }
        }

        /// <summary>
        /// Используется в сеттерах для извещения о том, что изменились выходные значения максимума
        /// </summary>
        private void NotifyOutMin()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMin"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMinP"));
        }

        /// <summary>
        /// Используется в сеттерах для извещения о том, что изменились выходные значения минимума
        /// </summary>
        private void NotifyOutMax()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMax"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutMaxP"));
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
                NotifyOutMax();
                UpdateNext();
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

        public double h => graphB.RenderSize.Height>0? graphB.RenderSize.Height:DEFAULT_GRAPH_SIZE;
        public double w => graphB.RenderSize.Width>0? graphB.RenderSize.Width:DEFAULT_GRAPH_SIZE;

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
