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
using System.Linq;
using BinanceCore.Entities;

namespace BinanceCore
{
    /// <summary>
    /// Логика взаимодействия для FractalDesigner.xaml
    /// </summary>
    public partial class FractalDesigner : UserControl
    {
        Segment[] segments;
        public FractalDesigner()
        {
            InitializeComponent();

            segments = new Segment[] {
                segment1, segment2, segment3, segment4, segment5, segment6, segment7
            };

            foreach(var s in segments)
                s.Changed += SegmentChanged;

            SegmentChanged();
            MoreStepsB_Click(null, null);// ради обновления активности кнопок добавления и убавления шагов
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            SegmentChanged();

        }

        bool firstPaint = true;
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (firstPaint)
            {
                firstPaint = false;
                SegmentChanged();
            }
            base.OnRender(drawingContext);
        }

        public string Code
        {
            get {
                string ret = "";    //  Здесь будет набираться код по частям с сегментов

                for (int i = 0; i < stepCount; i++)                         //  от каждого активного сегмента
                {                                                           //  (активны те, у которых номер меньше stepCount)
                    var max = segments[i].MaxD;                             //  получим максимальное допустимое изменение
                    var min = segments[i].MinD;                             //  минимальное допустимое изменение
                    var mode = segments[i].Mode.ToString().Substring(0, 1); //  и тип изменения (одной буквой U/D/S)
                    ret += $"{mode}-{min}-{max}; ";                         //  добавим накопленные данные к выходному коду
                }
                return ret.Trim(new char[] { ';',' '});                     //  вернём весь код фрактала
            }
            set { 
                var parts=value.Split(new char []{' ',';'},StringSplitOptions.RemoveEmptyEntries);
                for(int n=0; n<parts.Length;n++)
                {
                    var segment = segments[n];
                    var subParts = parts[n].Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    segment.MinD = int.Parse(subParts[1]);
                    segment.MaxD = int.Parse(subParts[2]);
                    segment.SetModeByLetter(subParts[0]);
                }
                StepCount = parts.Length;
            }
        }
        private void SegmentChanged()
        {
            segments[0].InMin = 0.5;
            segments[0].InMax = 0.5;
            segments[0].DrawGraph();

            for (int i = 0; ++i < segments.Length;)
            {
                segments[i].InMin = segments[i - 1].OutMin;
                segments[i].InMax = segments[i - 1].OutMax;
                segments[i].DrawGraph();
            }

            status.Content = Code;
        }
        int stepCount = 7;
        public int StepCount
        {
            get
            {
                return stepCount;
            }
            set
            {
                stepCount = value;
                for (int i = segments.Length; --i >=0;)
                    segments[i].Visibility = i < stepCount ? Visibility.Visible : Visibility.Hidden;
            }

        }
        private void MoreStepsB_Click(object sender, RoutedEventArgs e)
        {
            if (StepCount < segments.Length) ++StepCount;
            MoreStepsB.IsEnabled=StepCount < segments.Length;
            LessStepsB.IsEnabled = true;
        }

        private void LessStepsB_Click(object sender, RoutedEventArgs e)
        {
            if (StepCount > 1) --StepCount;
            LessStepsB.IsEnabled = StepCount > 1;
            MoreStepsB.IsEnabled = true;
        }
    }
}
