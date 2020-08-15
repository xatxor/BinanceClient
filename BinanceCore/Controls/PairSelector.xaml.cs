using Binance.Net;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System;
using Binance.Net.Objects.Spot.SpotData;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;

namespace BinanceCore.Controls
{
    /// <summary>
    /// Выбиралка символа торговой пары.
    /// Загружает список символов пар с бинанса, при выборе генерирует эвенты
    /// </summary>
    public partial class PairSelector : UserControl
    {
        #region Генерируемые в связи с вводом пользователем события
        /// <summary>
        /// Такие делегаты будут привязываться к событию о выборе пары, стабильной и торговой монеты
        /// </summary>
        /// <param name="data">выбранный вариант</param>
        public delegate void SelectedDgt(string data);
        /// <summary>
        /// Срабатывает если была выбрана пара
        /// </summary>
        public event SelectedDgt SymbolSelected;
        /// <summary>
        /// Срабатывает если был введён токен стабильной монеты - срабатывает когда пользователь уводит курсор из поля ввода токена и вместе с PairSelected
        /// </summary>
        public event SelectedDgt StableSet;
        /// <summary>
        /// Срабатывает если был введён токен торговой монеты - срабатывает когда пользователь уводит курсор из поля ввода токена и вместе с PairSelected
        /// </summary>
        public event SelectedDgt TradeSet;
        #endregion

        #region Доступы к данным в полях ввода
        public string Symbol => SymbolsCB.SelectedItem == null ? "" : SymbolsCB.SelectedItem.ToString();
        public string Stable => stableTB.Text;
        public string Trade => tradeTB.Text;
        #endregion
        private Timer fxTimer = new Timer(100);

        public decimal LastPrice;

        IEnumerable<string> knownStables = new string[] { "USDT" };

        public PairSelector()
        {
            InitializeComponent();
            balanceTB.PreviewMouseDown+= (s,e) => UpdateBalance();
            tradeTB.GotFocus += (s, e) => (tradeTB).Tag = tradeTB.Text;        //  Привяжем запоминание ранее представленного текста
            stableTB.GotFocus += (s, e) => (stableTB).Tag = stableTB.Text;      //  в тэге тексбокса если в текстбокс попал фокус, и начинается изменение данных
            tradeTB.LostFocus   +=  (s, e)  => MakeEvent(tradeTB, TradeSet);   //  привяжем генерацию событий с текстами из текстбоксов
            stableTB.LostFocus  +=  (s, e)  => MakeEvent(stableTB, StableSet);  //  в моменты покидания текстбоксами фокусов (ввели и ткнули мышкой в другое место)
            SymbolsCB.SelectionChanged += (s, e) =>
            {
                if (e.AddedItems.Count != 1) return;
                var selectedItem = e.AddedItems[0].ToString();
                foreach (var stable in knownStables)
                    if (selectedItem.StartsWith(stable) || selectedItem.EndsWith(stable))
                    {
                        stableTB.Text = stable;
                        tradeTB.Text = selectedItem.Replace(stable, "");
                        MakeEvent(stableTB, StableSet);
                        break;
                    }
                SymbolSelected?.Invoke(selectedItem);
            };
            fxTimer.Elapsed += FxTimer_Elapsed;
            fxTimer.Start();
        }
        /// <summary>
        /// Пытается узнать баланс по токену через клиент бинанса.
        /// Клиента надо задатьь заранее через свойство Client, иначе будет Exception
        /// </summary>
        /// <param name="token">Токен монеты, по которой нужно узнать баланс</param>
        /// <returns></returns>
        public decimal GetBalance(string token)
        {
            var info = client.GetAccountInfo();
            var bal=info.Data.Balances.Where(b => b.Asset == token).Single().Free;
            return bal;
        }
        private void Blink()
        {
            balanceTB.Opacity = 0;
        }
        private void FxTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current?.Dispatcher.Invoke(new Action(() =>
            {
                if (balanceTB.IsVisible)
                {
                    if (balanceTB.Opacity <1) balanceTB.Opacity+=(1- balanceTB.Opacity)/2;
                }
            }));
        }
        public string UpdateBalance()
        {
            var stableAmount = GetBalance(Stable);
            var tradeAmount = GetBalance(Trade);
            var total = tradeAmount * LastPrice + stableAmount;
            string bal =
                $"{Stable.ToString().PadLeft(4)}: {stableAmount.ToString("0.#######").PadLeft(11).TrimEnd('0')}\n" +
                $"{Trade.ToString().PadLeft(4)}: {tradeAmount.ToString("0.#######").PadLeft(11).TrimEnd('0')}\n" +
                $" SUM: {total.ToString("0.#######").PadLeft(11).TrimEnd('0')}$";
            balanceTB.Text = bal;
            Blink();
            return bal;
        }
        /// <summary>
        /// Быстрая возможность получить текст баланса
        /// </summary>
        public string BalInfo
        {
            get
            {
                var bal = "";
                Application.Current.Dispatcher.Invoke(() => bal = balanceTB.Text);
                return bal;
            }
        }
        public BinanceClient client;
        /// <summary>
        /// Позволяет установить пару и задать в ней торговый и стабильный токены
        /// </summary>
        /// <param name="pair">Символ пары</param>
        /// <param name="trade">Торговый токен. Если его не указать, то он будет сформирован убиранием стейбла из символа пары</param>
        /// <param name="stable">Стабильный токен. Если его не указать, то стейлб будет USDT</param>
        internal void SetPair(string pair, string trade=null, string stable="USDT")
        {
            if (trade == null) trade = pair.Replace(stable,"");
            SymbolsCB.SelectedItem = pair;
            tradeTB.Text = trade;
            stableTB.Text = stable;
        }

        /// <summary>
        /// Загружает список всех возможных торговых пар с бинанса, содержащих в себе какой-то из известрых стабильных токенов
        /// Список стабильных токенов
        /// </summary>
        /// <param name="client">Клиент к бинансу, через который можно обратиться на апи</param>
        /// <param name="KnownStables">Перечисление известных стейбл-коинов (выбиралка их запомнит на будущее чтобы при выборе символа определять, где в нём стейбл)</param>
        public void LoadSymbols(IEnumerable<string> KnownStables=null)
        {
            if (KnownStables != null) knownStables = KnownStables;  //  Если в параметрах было передано перечисление стейблов, запомним его вместо старого

            SymbolsCB.Items?.Clear();                               //  очистим выбиралку если она не пустая чтобы можно было заменить ей источник данных
            var symbols = client.GetExchangeInfo().                 //  загрузим данные о курсах с бинанса
                                Data.Symbols.Select(s => s.Name).   //  оттуда захватим только имена символов
                                OrderBy(s => s).ToList();           //  и отсортируем их

            var symbolsWithStables = new List<string>();            //  теперь отбрерём только те
            foreach (var s in symbols.ToArray())                    //  символы, в которых
                foreach (var ks in KnownStables)                    //  хотя бы один из известных стейблов
                    if (s.StartsWith(ks) || s.EndsWith(ks))         //  указан в начале или конце символа
                    {
                        symbolsWithStables.Add(s);                  //  такие символы можно добавлять
                        break;                                      //  и не искать для них дальше соответствия в стейблах чтобы не было дубликатов
                    }

            SymbolsCB.ItemsSource = symbolsWithStables;             //  в итоге загрузим список подходящих для торговли символов в выбиралку
        }

        /// <summary>
        /// Отправляет эвент по заданному делегату если в текстбоксе изменился текст
        /// Изменился ли текст выясняется сравнением со старым образцом, который надо сохранять в Tag заранее
        /// В параметр улетает текст из заданного текстбокса
        /// </summary>
        /// <param name="editor">Текстбокс, откуда текст надо выслать эвентом</param>
        /// <param name="dgt">Эвент, на который надо выслать текст</param>
        private void MakeEvent(TextBox editor, SelectedDgt dgt)
        {
            if (editor.Tag==null ||                                 //  Текст точно изменился если до этого его не было вообще
                (editor.Tag.ToString() != editor.Text))             //  Если был, то сравним, изменился ли
                dgt?.Invoke((string)(editor.Tag = editor.Text));    //  Если изменился - вызовем событие по переданному через аргумент делегату
        }

    }
}
