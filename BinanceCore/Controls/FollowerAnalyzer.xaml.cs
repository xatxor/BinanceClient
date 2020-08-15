using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace BinanceCore.Controls
{
    /// <summary>
    /// Следящий анализатор-командир закупок.
    /// Анализатор запоминает базовый уровень цены и ждёт, пока токен опустится ниже этого уровня на заданную величину,
    /// тогда токен покупается. После этого ожидается рост от уровня покупки на ту же заданную величину, и по достижению - продажа.
    /// Цикл продолжается - ожидается падение курса, затем производится покупка, ожидание роста и продажа.
    /// Если курс падает в N раз сильнее, чем ожидался рост, то новый уровень признаётся новым базовым из-за сильного падения вместо роста.
    /// Если курс растёт в N раз сильнее, чем ожидалось падение, то новый уровень признаётся новым базовым из-за сильного роста вместо падения.
    /// </summary>
    public partial class FollowerAnalyzer : UserControl, INotifyPropertyChanged
    {
        public delegate void FollowEventDgt(FollowerAnalyzer sender);
        public event FollowEventDgt GotFall;
        public event FollowEventDgt GotRise;
        public event FollowEventDgt LostFall;
        public event FollowEventDgt LostRise;
        public event LogDgt LogMsg;
        public event PropertyChangedEventHandler PropertyChanged;

        decimal range = 0;
        /// <summary>
        /// Величина ожидаемого роста 
        /// </summary>
        public decimal Range
        {
            get => range;
            set
            {
                if (range == value) return;
                range = value;
                Log($"Range (will sell on reach) set to {value}%");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Range"));
            }
        }

        bool active = false;
        /// <summary>
        /// Величина ожидаемого роста 
        /// </summary>
        public bool Active
        {
            get => active;
            set
            {
                if (active == value) return;
                active = value;
                Log($"FOLLOWER IS {(value?"ACTIVE":"PASSIVE")}");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Active"));
            }
        }

        decimal rangeBuy = 0;
        /// <summary>
        /// Величина ожидаемого падения
        /// </summary>
        public decimal RangeBuy
        {
            get => rangeBuy;
            set
            {
                if (rangeBuy == value) return;
                rangeBuy = value;
                Log($"RangeBuy (will buy on reach) set to {value}%");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RangeBuy"));
            }
        }

        decimal basePrice = 0;
        /// <summary>
        /// Базовая цена, относительно которой отсчитываются рост и падение
        /// </summary>
        public decimal BasePrice
        {
            get => basePrice;
            set
            {
                if (basePrice == value) return;
                basePrice = value;
                Log($"<code>Base Price: {value.ToString().TrimEnd('0')}</code>");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BasePrice"));
            }
        }

        /// <summary>
        /// Метод отправки отладочных сообщений в хозяйский код
        /// </summary>
        /// <param name="v"></param>
        private void Log(string v)
        {
            LogMsg?.Invoke(this, $"{v}");
        }

        public Mode mode;
        /// <summary>
        /// Режим работы следилки-командира - ожидать падения курса чтобы купить токен или ожидать роста чтобы его продать. Сдедилка не покупает и не продаёт, но следит
        /// и даёт эвенты на продажу и покупку, а также извещает алертами (эвентами неприятных сообщений) если произошел неожиданный обратный ход курса.
        /// </summary>
        public Mode Mode
        {
            get => mode;
            set { mode = value; modeB.Content=mode==Mode.WAIT_FALL? "Жду падения, у меня стейбл" : "Жду роста, у меня есть токены"; Log($"<code>      Mode: {value}</code>"); }
        }
        decimal latestPrice = 0;
        /// <summary>
        /// Последняя известная цена монеты по отношению к стейблу
        /// </summary>
        public decimal LatestPrice
        {
            get => latestPrice;
            set
            {
                if (latestPrice == value) return;
                latestPrice = value;
                PropertyChanged?.Invoke(this,new PropertyChangedEventArgs("LatestPrice"));
            }
        }

        decimal failFallLevel = 3;
        /// <summary>
        /// На сколько процентов должен вырасти курс когда сидим в стейбле
        /// чтобы пришлось признать, что падения не дождались
        /// </summary>
        public decimal FailFallLevel
        {
            get => failFallLevel;
            set
            {
                if (failFallLevel == value) return;
                failFallLevel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FailFallLevel"));
            }
        }

        decimal failRaiseLevel = 3;
        /// <summary>
        /// На сколько процентов должен упасть курс когда монета куплена
        /// чтобы пришлось признать, что рост не состоялся
        /// </summary>
        public decimal FailRaiseLevel
        {
            get => failRaiseLevel;
            set
            {
                if (failRaiseLevel == value) return;
                failRaiseLevel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FailRaiseLevel"));
            }
        }
        /// <summary>
        /// Метод приёма очередной цены - при приёме цены если анализатор включен (галочка Active), nо могут сгенерироваться
        /// реакции на изменение цены
        /// </summary>
        /// <param name="newPrice">новая цена токена по паре</param>
        public void PriceUpdate(decimal newPrice)
        {
            LatestPrice = newPrice;

            var dPrice = newPrice - BasePrice;
            string wouldWin = "0";
            winNowL.Foreground = Brushes.Orange;
            if (Mode == Mode.WAIT_FALL) //  Если ожидается не рост, а падение
                dPrice = -dPrice;       //  то бдуем учитывать изменение цены наоборот чтобы отрицательные значения считались плохими

            if (basePrice != 0)
            {
                if (dPrice > 0.1M)
                    winNowL.Foreground = Brushes.Green;
                else
                    winNowL.Foreground = Brushes.Red;

                wouldWin = (dPrice / BasePrice - 0.001M).ToString("0.###%");
            }
            winNowL.Content = wouldWin;

            if (activeCB.IsChecked==true)
                CourseChangeReaction(newPrice);
        }

        /// <summary>
        /// Реакции на обновление курса.
        /// В качестве реакций могут быть эвенты о том, что достигнут нужный рост
        /// или нужное падение или падение не состоялось или рост не состоялся.
        /// Рост и падение считаются достигнутыми при увеличении или уменьшении курса на заданное число range.
        /// Несостоявшимся считается обратный ход курса на число range*allowedLostBeforeUpdateBasePrice
        /// </summary>
        /// <param name="newPrice">актуальная цена токена, на котором идёт торг против стейбла (например против USDT)</param>
        private void CourseChangeReaction(decimal newPrice)
        {
            var d = BasePrice - newPrice;
            var dp = 0M;
            if (d > 0) dp = d * 100 / BasePrice;
            if (d < 0) dp = d * 100 / BasePrice;

            if (Mode == Mode.WAIT_FALL)
            {
                if (dp > range && dp>0)
                {
                    GotFall(this);
                    Mode = Mode.WAIT_RISE;
                    BasePrice = newPrice;
                }
                else
                if (dp < -FailFallLevel && dp<0)
                {
                    LostFall(this);
                }
            }
            else
            {
                if (-dp> rangeBuy && dp<0)
                {
                    GotRise(this);
                    Mode = Mode.WAIT_FALL;
                    BasePrice = newPrice;
                }
                else
                if (dp>FailRaiseLevel && dp>0)
                {
                    LostRise(this);
                }
            }


        }

        /// <summary>
        /// В конструкторе анализатора привязываются обработчики событий по изменению текста и нажатию на кнопку режима
        /// </summary>
        public FollowerAnalyzer()
        {
            InitializeComponent();
            modeB.Click += (a, b) => Mode = Mode == Mode.WAIT_FALL ? Mode.WAIT_RISE : Mode.WAIT_FALL;
            TopLevelController.DataContext = this;
        }

        /// <summary>
        /// При нажатии на кнопку базовой цены последняя загруженная цена становится базовой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasePriceB_Click(object sender, RoutedEventArgs e)
        {
            BasePrice = LatestPrice;
        }


    }

    /// <summary>
    /// Режимы работы следящего анализатора-командира: 
    /// WAIT_FALL: режим ожидания падения курса (чтобы купить токен)
    /// WAIT_RISE: режим ожидания роста (чтобы продать)
    /// </summary>
    public enum Mode { WAIT_RISE, WAIT_FALL };
}
