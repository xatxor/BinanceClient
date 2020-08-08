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
    /// Логика взаимодействия для FollowerAnalyzer.xaml
    /// </summary>
    public partial class FollowerAnalyzer : UserControl
    {
        public delegate void FollowEventDgt(FollowerAnalyzer sender);
        public delegate void LogDgt(FollowerAnalyzer sender, string msg);
        public event FollowEventDgt GotFall;
        public event FollowEventDgt GotRise;
        public event FollowEventDgt LostFall;
        public event FollowEventDgt LostRise;
        public event LogDgt LogMsg;

        decimal range = 0;
        public decimal Range
        {
            get => range;
            set { range = value; if (rangeTB.Text != range.ToString()) rangeTB.Text = range.ToString(); Log($"Range set to {value}"); }
        }

        decimal basePrice = 0;
        public decimal BasePrice
        {
            get => basePrice;
            set { basePrice = value; if(baseTB.Text!=BasePrice.ToString()) baseTB.Text = basePrice.ToString(); Log($"Base Price set to {value}"); }
        }

        private void Log(string v)
        {
            LogMsg?.Invoke(this, $"{v}");
        }

        public Mode mode;
        public Mode Mode
        {
            get => mode;
            set { mode = value; modeB.Content=mode==Mode.WAIT_FALL?"жду падения":"жду роста"; Log($"Mode Price set to {value}"); }
        }
        decimal latestPrice = 0;
        decimal allowLostBeforeUpdateBasePrice = 3;
        public void PriceUpdate(decimal newPrice)
        {

            latestPrice = newPrice;
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
                    if (basePrice + (range* allowLostBeforeUpdateBasePrice) < newPrice)
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
                    if(BasePrice- (range * allowLostBeforeUpdateBasePrice)>newPrice)
                        BasePrice = newPrice;
                }
            }

        }

        public FollowerAnalyzer()
        {
            InitializeComponent();
            rangeTB.TextChanged+=(a,b)=>rangeTB.TrySaveDecimal(out range);
            baseTB.TextChanged += (a, b) => baseTB.TrySaveDecimal(out basePrice);
            failTB.TextChanged += (a, b) => failTB.TrySaveDecimal(out allowLostBeforeUpdateBasePrice);
            modeB.Click += (a, b) => Mode = Mode == Mode.WAIT_FALL ? Mode.WAIT_RISE : Mode.WAIT_FALL;
        }

        private void BasePriceB_Click(object sender, RoutedEventArgs e)
        {
            BasePrice = latestPrice;
        }



        private void fallTB_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
    public enum Mode { WAIT_RISE, WAIT_FALL };
}
