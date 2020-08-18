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
    /// Дизайнер фрактала позволяет определить числовые параметры сегментов фрактала и выбрать количество используемых сегментов.
    /// </summary>
    public partial class FractalDesigner : UserControl
    {
        /// <summary>
        /// Все редакторы сегментов
        /// </summary>
        Segment[] segments;

        /// <summary>
        /// Считывание количества активных сегментов во фрактале через состояния их видимости и установка
        /// количества активных сегментов через скрытие лишних.
        /// </summary>
        public int StepCount
        {
            get => segments.Where(s=>s.Visibility==Visibility.Visible).Count();

            set
            {
                for (int i = segments.Length; --i >= 0;)
                    segments[i].Visibility = i < value ? Visibility.Visible : Visibility.Hidden;
                MoreStepsB.IsEnabled = StepCount < segments.Length; //  В зависимости от количества включенных сегментов
                LessStepsB.IsEnabled = StepCount > 1;               //  активируются кнопки добавления и убавления
            }
        }


        /// <summary>
        /// Конструктор инициализирует интерфейс и учитывает все редакторы сегментов на будущее,
        /// назначает им событие на случай изменения данных в сегменте,
        /// </summary>
        public FractalDesigner()
        {
            InitializeComponent();

            segments = new Segment[] {  //  перечислим все сегменты чтобы потом обращаться к ним через массив
                segment1, segment2, segment3, segment4, segment5, segment6, segment7
            };

     /*       foreach(var s in segments)          //  в каждом сегменте
                s.Changed += SegmentChanged;    //  к событию изменения данных привяжем местную функцию обновления всех сегментов*/

            MoreStepsB.Click += (b, a) => ++StepCount;
            LessStepsB.Click += (b, a) => --StepCount;
            segments[0].InMin = segments[0].InMax = 0.5;
//            SegmentChanged();                   //  имитируем необходимость обновить все сегменты
        }

        /// <summary>
        /// ЧИТ - обновление экрана на старте. 
        /// </summary>
        /// <param name="oldParent"></param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
           // SegmentChanged();
        }
        /// <summary>
        /// Флаг первой отрисовки ради чита рендеринга
        /// </summary>
        bool firstPaint = true;
        /// <summary>
        /// В рендеринге контрола в случае первого рендеринга выполняется обновление всех сегментов чтобы сформировалась первоначально
        /// корректная картинка и сегменты связались один с другим началами и концами
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {/*
            if (firstPaint)
            {
                firstPaint = false;
                SegmentChanged();
            }*/
            base.OnRender(drawingContext);
        }
        /// <summary>
        /// Возможность получит код всех сегментов вместе или настриоть сегменты по коду.
        /// </summary>
        public string Code
        {
            get {                   //  Геренация общего кода всех сегментов
                string ret = "";    //  Здесь будет набираться код по частям с сегментов

                for (int i = 0; i < StepCount; i++)                         //  от каждого активного сегмента
                {                                                           //  (активны те, у которых номер меньше stepCount)
                    var max = segments[i].MaxD;                             //  получим максимальное допустимое изменение
                    var min = segments[i].MinD;                             //  минимальное допустимое изменение
                    var mode = segments[i].Mode.ToString().Substring(0, 1); //  и тип изменения (одной буквой U/D/S)
                    ret += $"{mode}-{min}-{max}; ";                         //  добавим накопленные данные к выходному коду
                }
                return ret.Trim(new char[] { ';',' '});                     //  вернём весь код фрактала
            }
            set {                   //  Настройка сегментов по коду
                var parts=value.Split(new char []{' ',';'},StringSplitOptions.RemoveEmptyEntries);  //  Разбиваем код на части
                for(int n=0; n<parts.Length;n++)    //  Каждая из частей содержит настройки одного сегмента, пойдём по порядку
                {
                    var segment = segments[n];      //  выберем очередной сегмент на экране
                    var subParts = parts[n].Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    segment.MinD = int.Parse(subParts[1]);  //  разбив его описание ещё на части узнаем величины    
                    segment.MaxD = int.Parse(subParts[2]);  //  Максимума, минимума и режима ожиданий
                    segment.SetModeByLetter(subParts[0]);   //  пропишем величины в свойства сегмента на экране
                }
                StepCount = parts.Length;                   //  включим то количество сегментов на экране, сколько обнаружилось в строке кода
            }
        }

        /// <summary>
        /// В случае измения данных любого сегмента нужно перерисовать графики на остальных сегментах, и на нём тоже чтобы учесть стыковки
        /// на началах и концах графиков. У нас каждый сегмент получает на входе диапазон возможных величин, а дальше этот диапазон расширяется
        /// в зависимости от разрешенного максимума вверх и вниз.
        /// </summary>
        private void SegmentChanged()
        {
            segments[0].InMin = segments[0].InMax = 0.5;    //  первый сегмент всегда начинается посередине из одной точки - не из диапазона

            for (int i = 0; ++i < segments.Length;)         //  все сегменты кроме первого по очереди слева направо
            {
                segments[i].InMin = segments[i - 1].OutMin; //  принимают себе на вход диапазон
                segments[i].InMax = segments[i - 1].OutMax; //  преыдущих сегментов
            }
        }

    }
}
