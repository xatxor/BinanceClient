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
        public event FollowEventDgt GotFall;
        public event FollowEventDgt GotRise;
        public event FollowEventDgt LostFall;
        public event FollowEventDgt LostRise;

        decimal range = 0;
        public decimal Range
        {
            get => range;
            set { range = value;  }
        }

        decimal basePrice = 0;
        public decimal BasePrice
        {
            get => basePrice;
            set { basePrice = value; }
        }

        public Mode mode;
        public Mode Mode
        {
            get => mode;
            set { mode = value; modeB.Content=mode==Mode.WAIT_FALL?"жду падения":"жду роста"; }
        }

        public void PriceUpdate(decimal newPrice)
        {
            if (Mode == Mode.WAIT_FALL)
            {
                if (basePrice - range > newPrice)
                {
                    GotFall(this);
                    Mode = Mode.WAIT_RISE;
                }
                if (basePrice + range < newPrice)
                {
                    GotRise(this);
                    Mode = Mode.WAIT_FALL;
                }
            }
            else
            {
                if (basePrice + range < newPrice)
                {
                    LostFall(this);
                    basePrice = newPrice;
                }
                if (basePrice - range > newPrice)
                {
                    LostRise(this);
                    basePrice = newPrice;
                }
            }

        }

        public FollowerAnalyzer()
        {
            InitializeComponent();
            rangeTB.TextChanged+=(a,b)=>rangeTB.TrySaveDecimal(out range);
            baseTB.TextChanged += (a, b) => baseTB.TrySaveDecimal(out basePrice);
            modeB.Click += (a, b) => Mode = Mode == Mode.WAIT_FALL ? Mode.WAIT_RISE : Mode.WAIT_FALL;
        }

    }
    public enum Mode { WAIT_RISE, WAIT_FALL };
}
