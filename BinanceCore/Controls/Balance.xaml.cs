using Binance.Net;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Логика взаимодействия для Balance.xaml
    /// </summary>
    public partial class Balance : UserControl
    {
        BinanceClient client = null;

        public Balance()
        {
            InitializeComponent();
        }
        public string balInfo => balanceTB.Text;
        public void UpdateBalance(BinanceClient _client=null)
        {
            try
            {
                if (_client != null) client = _client;
                var btc = GetBalance("BTC").ToString().TrimEnd('0');
                var eth = GetBalance("ETH").ToString().TrimEnd('0');
                var usdt = GetBalance("USDT").ToString().TrimEnd('0');
                balanceTB.Text = $"BTC: {btc}\nETH: {eth}\nUSDT: {usdt}";
            }
            catch { }
        }
        private decimal GetBalance(string token)
        {
            var info = client.GetAccountInfo();
            return info.Data.Balances.Where(b => b.Asset == token).Single().Free;
        }

        private void balanceTB_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            UpdateBalance();
        }
    }
}
