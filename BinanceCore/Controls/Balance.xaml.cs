using Binance.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
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
    /// TODO: Добавить доллары в баланс чтобы видеть суммарный капитал. Учитывать трейд монету с вычетом цены конвертации
    /// TODO: Добавить панель настроек с комиссией и ключами от телеги и бинанса
    /// TODO: Переключить телегу на канал

    /// <summary>
    /// Панель отображения монет на балансе бинанса.
    /// Для работы использует клиент соединения с бинансом (нужно настроить через BinanceClient).
    /// При нажатии на панель данные обновляются и панель моргает.
    /// Программно можно обновить вызовом UpdateBalance
    /// </summary>
    public partial class Balance : UserControl
    {
        public event LogDgt Log;
        private Timer fxTimer = new Timer(100);
        /// <summary>
        /// Многократно используемый клиент бинанса
        /// </summary>
        public BinanceClient Client { get; set; }
        /// <summary>
        /// Балансы будут выгружаться по указанным валютам
        /// </summary>
        public string[] Tokens = { "BTC", "USDT" };
        public Balance()
        {
            InitializeComponent();
            fxTimer.Elapsed += FxTimer_Elapsed;
            fxTimer.Start();
        }

        private void FxTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current?.Dispatcher.Invoke(new Action(() =>
            {
                if (over.IsVisible)
                {
                    if (over.Opacity > 0.01) over.Opacity /= 2;
                    else if (over.Opacity > 0) over.Opacity = 0;
                }
            }));
        }
        /// Моргает, включая видимсоть накладки над всем этим контролом.
        /// Накладка потом угаснет по таймеру.
        private void Blink()
        {
            over.Opacity = 255;
        }

        /// <summary>
        /// Быстрая возможность получить текст баланса
        /// </summary>
        public string BalInfo
        {
            get   {
                var bal = "";
                    Application.Current.Dispatcher.Invoke(  () =>   bal = balanceTB.Text    );
                return bal;
            }
        }
        /// <summary>
        /// Пишет в на экран (в текстблок) баланс всех заданных в Tokens токенов по одному токену в строку
        /// В случае ошибки генерирует сообщение через эвент Log
        /// </summary>
        public void UpdateBalance()
        {
            try
            {
                balanceTB.Text = "";
                foreach (var token in Tokens)
                    balanceTB.Text += $"{token.PadLeft(5)}: {GetBalance(token).ToString("0.#######").PadLeft(14).TrimEnd('0')}\n";
                Blink();
            }
            catch (Exception ex) {
                Log?.Invoke(this, $"Update Balance error: {ex.Message}");
            }
        }


        /// <summary>
        /// Пытается узнать баланс по токену через клиент бинанса.
        /// Клиента надо задатьь заранее через свойство Client, иначе будет Exception
        /// </summary>
        /// <param name="token">Токен монеты, по которой нужно узнать баланс</param>
        /// <returns></returns>
        private decimal GetBalance(string token)
        {
            if (Client == null) throw new Exception("Необходимо установить значение Balance.Client прежде чем запрашивать баланс!");
            var info = Client.GetAccountInfo();
            return info.Data.Balances.Where(b => b.Asset == token).Single().Free;
        }

        /// <summary>
        /// При нажатии мышкой на панель баланса срабатывает это событие, и баланс обновляется на экране.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void balanceTB_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            UpdateBalance();
        }
    }
}
