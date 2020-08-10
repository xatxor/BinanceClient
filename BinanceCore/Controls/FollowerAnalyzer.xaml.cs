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
    public partial class FollowerAnalyzer : UserControl
    {
        public delegate void FollowEventDgt(FollowerAnalyzer sender);
        public event FollowEventDgt GotFall;
        public event FollowEventDgt GotRise;
        public event FollowEventDgt LostFall;
        public event FollowEventDgt LostRise;
        public event LogDgt LogMsg;

        decimal range = 0;
        /// <summary>
        /// Величина ожидаемого роста или падения
        /// </summary>
        public decimal Range
        {
            get => range;
            set { range = value; if (rangeTB.Text != range.ToString()) rangeTB.Text = range.ToString(); Log($"Range set to {value}"); }
        }

        decimal basePrice = 0;
        /// <summary>
        /// Базовая цена, относительно которой отсчитываются рост и падение
        /// </summary>
        public decimal BasePrice
        {
            get => basePrice;
            set { basePrice = value; if(baseTB.Text!=BasePrice.ToString()) baseTB.Text = basePrice.ToString(); Log($"Base Price set to {value}"); }
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
            set { mode = value; modeB.Content=mode==Mode.WAIT_FALL?"Жду падения":"Жду роста"; Log($"Mode Price set to {value}"); }
        }
        decimal latestPrice = 0;
        decimal allowLostBeforeUpdateBasePrice = 3;
        /// <summary>
        /// Метод приёма очередной цены - при приёме цены если анализатор включен (галочка Active), nо могут сгенерироваться
        /// реакции на изменение цены
        /// </summary>
        /// <param name="newPrice">новая цена токена по паре</param>
        public void PriceUpdate(decimal newPrice)
        {
            latestPrice = newPrice;

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
            if (Mode == Mode.WAIT_FALL)
            {
                if (basePrice - range > newPrice)
                {
                    GotFall(this);
                    Mode = Mode.WAIT_RISE;
                    BasePrice = newPrice;
                }
                else
                if (basePrice + range < newPrice)
                {
                    LostFall(this);
                    if (basePrice + (range * allowLostBeforeUpdateBasePrice) < newPrice)
                        BasePrice = newPrice;
                }
            }
            else
            {
                if (basePrice + range < newPrice)
                {
                    GotRise(this);
                    Mode = Mode.WAIT_FALL;
                    BasePrice = newPrice;
                }
                else
                if (basePrice - range > newPrice)
                {
                    LostRise(this);
                    if (BasePrice - (range * allowLostBeforeUpdateBasePrice) > newPrice)
                        BasePrice = newPrice;
                }
            }
        }

        /// <summary>
        /// В конструкторе анализатора привязываются обработчики событий по изменению текста и нажатию на кнопку режима
        /// </summary>
        public FollowerAnalyzer()
        {
            InitializeComponent();
            rangeTB.TextChanged+=(a,b)=>rangeTB.TrySaveDecimal(out range);  //  при изменении текста в поле range попытаемся записать введенное в переменную range как число
            baseTB.TextChanged += (a, b) => baseTB.TrySaveDecimal(out basePrice);
            failTB.TextChanged += (a, b) => failTB.TrySaveDecimal(out allowLostBeforeUpdateBasePrice);
            modeB.Click += (a, b) => Mode = Mode == Mode.WAIT_FALL ? Mode.WAIT_RISE : Mode.WAIT_FALL;
        }

        /// <summary>
        /// При нажатии на кнопку базовой цены последняя загруженная цена становится базовой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasePriceB_Click(object sender, RoutedEventArgs e)
        {
            BasePrice = latestPrice;
        }


    }

    /// <summary>
    /// Режимы работы следящего анализатора-командира: 
    /// WAIT_FALL: режим ожидания падения курса (чтобы купить токен)
    /// WAIT_RISE: режим ожидания роста (чтобы продать)
    /// </summary>
    public enum Mode { WAIT_RISE, WAIT_FALL };
}
