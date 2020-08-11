using Binance.Net;
using Binance.Net.Objects.Spot;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using BinanceCore.Entities;
using System.Timers;
using System.Windows.Controls;
using BinanceCore.Services;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using Binance.Net.Objects.Spot.SpotData;
using System.ComponentModel;

///  TODO: Пора добавлять следящие ползунки, дающие сигналы торговли

namespace BinanceCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Доступ к нашей БД
        /// </summary>
        Repository repos = new Repository();
        /// <summary>
        /// Клиент для работы с бинансом
        /// </summary>
        BinanceClient client = new BinanceClient();               //  создание глобального клиента для связи с бинансом

        /// <summary>
        /// Для поддержки наблюдаемых свойств в классе - это позволяет их привязать к полям через биндинг
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private int timeout;
        /// <summary>
        /// Длина интервала между автоматическими обновлениями графика
        /// </summary>
        public int Timeout
        {
            get => timeout;
            set
            {
                timeout = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Timeout"));
                }
            }
        }
        int timePassed = 0;
        Timer timer = new Timer(1000);

        Telega telega = new Telega("1294746661:AAGeFjeIBPTvG2pUhcdflPD4Nc_pj8ExdXI", 109159596);
        public MainWindow()
        {
            InitializeComponent();
            balance.Client = client;                    //  выдача показывалке балансов клиента для связи с бинансом
            balance.Log += (sender, msg) => Log(msg);   //  привязка логов показывалки балансов к логам главного окна

            timer.Elapsed += Timer_Elapsed;             //  привязка ежесекундного таймера
            symbolSelector.LoadSymbols(client, new string[] { "USDT" });
            symbolSelector.SetPair("LTCUSDT");                  //  установим торговую пару по умолчанию
            LoadDefaultProject();
            symbolSelector.SymbolSelected += symbolChanged;     //  При изменении выбора торговой пары

            followA.GotFall += FollowA_GotFall;
            followA.GotRise += FollowA_GotRise;
            followA.LostFall += FollowA_LostFall;
            followA.LostRise += FollowA_LostRise;
            followA.LogMsg += FollowA_LogMsg;


            balance.UpdateBalance();
            symbolSelector.StableSet += symbolChanged;          //  не важно, изменилась ли вся пара или часть
            symbolSelector.TradeSet += symbolChanged;           //  нужно выполнить некоторую актуализацию

            TopLevelContainer.DataContext = this;               //  Чтобы работали биндинги

            Task.Run(async () => await telega.MessageMaster("BinanceCore v.0.1 started."));
        }

        private void symbolChanged(string data)
        {
            balance.Tokens = new string[] { symbolSelector.Trade, symbolSelector.Stable };
            cache.Clear();
            balance.UpdateBalance();
        }

        private async void FollowA_LogMsg(object sender, string msg)
        {
            await telega.MessageMaster($"<i>{msg}</i>");
        }

        private decimal GetBalance(string token)
        {
            var info = client.GetAccountInfo();
            return info.Data.Balances.Where(b => b.Asset == token).Single().Free;
        }
        #region Реакции на Follower
        DateTime lastAlertTime = DateTime.MinValue;
        TimeSpan alertInterval = new TimeSpan(0, 3, 0);
        private async void FollowA_LostRise(Controls.FollowerAnalyzer sender)
        {
            await Alert("Не дождались роста - падает! Может продать?.. курс по паре: " + LastPriceTrimmed);
        }

        private string LastPriceTrimmed=>
            LastPrice.ToString().TrimEnd('0');

        /// <summary>
        /// Отправляет сообщения, но блокирует отправку слишком часто. 
        /// Не чаще alertInterval срабатывает отправка
        /// </summary>
        /// <param name="msg">сообщение</param>
        /// <returns>асинхронная задача по отправке сообщения</returns>
        private async Task Alert(string msg)
        {
            if (DateTime.Now.Subtract(lastAlertTime) > alertInterval)
            {
                await telega.MessageMaster(msg);
                lastAlertTime = DateTime.Now;
            }
        }
        private async void FollowA_LostFall(Controls.FollowerAnalyzer sender)
        {
            await Alert("Не дождались падения - растёт! Может купить?.. курс по паре: " + LastPriceTrimmed);
        }

        private async void FollowA_GotRise(Controls.FollowerAnalyzer sender)
        {
            balance.UpdateBalance();
            await telega.MessageMaster("Курс вырос! Продаю по " + LastPriceTrimmed+"\nБудем ждать падения и купим снова.\n" + balance.BalInfo);
            SellBTCClicked(null, null);
            System.Threading.Thread.Sleep(1000);
            balance.UpdateBalance();
            await telega.MessageMaster("Теперь у нас\n" + balance.BalInfo);
        }

        private async void FollowA_GotFall(Controls.FollowerAnalyzer sender)
        {
            balance.UpdateBalance();
            await telega.MessageMaster("Курс упал! Покупаю по " + LastPriceTrimmed + "\nБудем ждать роста и продадим.\n"+balance.BalInfo);
            BuyBTCClicked(null, null);
            System.Threading.Thread.Sleep(1000);
            balance.UpdateBalance();
            await telega.MessageMaster("Теперь у нас\n" + balance.BalInfo);
        }
        #endregion

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { 
                if (++timePassed > Timeout)
                {
                    timePassed = 0;
                    timer.Stop();
                    autoCB.Content = "⏳";
                    Graph_Clicked(null, null);
                    this.DoEvents();
                    timer.Start();
                    this.DoEvents();
                    autoCB.Content = "✓";
                    followA.PriceUpdate(LastPrice);
                    balance.UpdateBalance();
                }
                else
                     autoCB.Content = $"{(Timeout-timePassed)}";
            }));

        }

        private void Log(string v)
        {
            Title = v;
            Console.WriteLine(v);
            this.DoEvents();
        }

        List<BinanceInfo> cache = new List<BinanceInfo>();


        /// <summary>
        /// ID последней кэшированной записи бинанса
        /// </summary>
        long LastCached => cache.Count() > 0 ? cache.Last().Id : 0;
        decimal LastPrice => cache.Count() > 0 ? cache.Last().RatePrice : 0;
        DateTime LastMoment => cache.Count() > 0 ? cache.Last().Time : DateTime.UtcNow.AddDays(-1);

        #region Реакции на нажатия
        private void Graph_Clicked(object sender, RoutedEventArgs e)
        {
            Log("Loading history...");
            DateTime fin = DateTime.UtcNow;
            var graphDuration = new TimeSpan(1, 0, 0, 0);

            cache.RemoveAll(r => r.Time < fin.AddDays(-graphDuration.TotalDays));       //   убираем из кэша устаревшие данные (те что в прошлом за пределами графика)

            var BinanceInfo = repos.GetRangeOfElementsByTime(                           // Выгрузим из БД записи по дате
                fin.Subtract(graphDuration),fin,SelectedPair,true)   // до текущего момента на длину графика заданную пару с учётом полноты SHORT
                .Where(tr => tr.Id > LastCached);                                       // и выберем оттуда только те записи, у которых номера больше, чем нам уже известны и есть в кэше

            cache.AddRange(BinanceInfo);                                                // добавляем свежие данные в кэш

            Log("History loaded. Drawing...");
            Coding.MakeCode(fin, graphDuration, cache);
            iv.LoadBitmap(Drawing.MakeGraph("", fin, graphDuration, cache));

            #region Отрисовка фракталов
            canv.Children.Clear();  //  очистка накладки на график - той накладки, где рисуются все найденные фракталы

            foreach (var configurator in fractalsSP.Children)           //  перебор всех фракталов, загруженных в плашки конфигураций
                Drawing.DrawFoundFractals(                              //  рисуем найденные случаи очередного фрактала
                    FractalMath.FindFractal(                            //  для этого находим фрактал
                        (configurator as FractalConfiguration).FractalDefinition, //  очередной
                        Coding.LatestCode),                             //  в коде графика
                    canv);
            #endregion
        }

        private void addFractalB_Click(object sender, RoutedEventArgs e)
        {
            CreateFractalConfiguration();
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            timePassed = 0;
            timer.Enabled = autoCB.IsChecked == true;
            if (!timer.Enabled)
                autoCB.Content = "AUTO";
        }

        private void saveB_Click(object sender, RoutedEventArgs e)
        {
            SaveProjectToDefault();
            Log($"Project Saved! ({DateTime.Now.ToString("HH:mm:ss")})");
        }

        private void loadB_Click(object sender, RoutedEventArgs e)
        {
            try                                                                 //  При загрузке сейва могут быть ошибки, поэтому try/catch
            {
                LoadDefaultProject();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "EXCEPTION", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SellBTCClicked(object sender, RoutedEventArgs e)
        {
            var bal = GetBalance(TradingToken);
            bal = ((int)(bal * 10000)) / 10000M;

            var res = client.PlaceOrder(SelectedPair,                               // торговую монету в паре
                                Binance.Net.Enums.OrderSide.Sell,                   //  продаём
                                Binance.Net.Enums.OrderType.Market,
                                bal);    //  по доступной цене
            if (res.Data != null)
                followA.Mode = Controls.Mode.WAIT_FALL;
            else
                await telega.MessageMaster($"Не могу продать {bal} {TradingToken}");

            Console.Write(res.ToString());
        }

        #region Быстрый доступ на чтение к выбранной паре токенов
        private string SelectedPair => symbolSelector.Symbol;
        private string TradingToken => symbolSelector.Trade;
        private string StableToken => symbolSelector.Stable;
        #endregion

        private async void BuyBTCClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var bal = GetBalance(StableToken);
                var will = ((int)(10000 * bal / LastPrice)) / 10000M;

                var res = client.PlaceOrder(SelectedPair,                               //  торговую монету в паре
                                    Binance.Net.Enums.OrderSide.Buy,                   //  покупаем
                                    Binance.Net.Enums.OrderType.Market,
                                    will);    //  по доступной цене
                if (res.Data != null)
                {
                    followA.Mode = Controls.Mode.WAIT_RISE;
                }
                else
                    await telega.MessageMaster($"Не могу купить {will} {TradingToken}");

                Console.Write(res.ToString());
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }

        }
        #endregion


        /// <summary>
        /// Создаёт контрол FractalDefinition, грузит в него настройки фрактала,
        /// привязывает событие удаления фрактала и выводит фрактал в список на экране.
        /// </summary>
        /// <param name="def">Описание фрактала или null если создаётся фрактал по умолчанию</param>
        /// <returns>Контрол фрактала, размещенный в списке на экране</returns>
        private FractalConfiguration CreateFractalConfiguration(FractalDefinition def=null)
        {
            var cfg = new FractalConfiguration();
            if (def != null)
            {
                cfg.Code = def.Code;                                          //  загружаем туда код фрактала
                cfg.Title = def.Title;                                        //  устанавливаем название
                cfg.FractalColor = def.Color.ToColor();                              //  цвет парсим из строки настройки (в настройках цвет в виде #ffffff
                cfg.Symbol = def.Symbol;                                       //  устанавливаем символ пометки фрактала
            }

            cfg.DeleteRequested += (sender) => { fractalsSP.Children.Remove(sender); };
            fractalsSP.Children.Add(cfg);
            return cfg;
        }


        private void SaveProjectToDefault()
        {
            List<FractalDefinition> fractalsList = new List<FractalDefinition>();
            foreach (var configurator in fractalsSP.Children)   //  перебор всех фракталов, загруженных в плашки конфигураций
                fractalsList.Add((configurator as FractalConfiguration).FractalDefinition);  //  берём очередную конфигурацию

            var proj = new Project()
            {
                fractals = fractalsList.ToArray(),
                interval = Timeout,
                symbol = symbolSelector.Symbol
            };
            proj.Save();
        }

        private void LoadDefaultProject()
        {
            try
            {
                var proj = Project.Load();                                      //  Загружаем объект сохранёнки
                ClearProject();
                foreach (var f in proj.fractals)                                //  перебираем все фракталы из загруженного проекта
                    CreateFractalConfiguration(f);

                Timeout = proj.interval;                     //  загружаем интервал автоматического обновления
                symbolSelector.SetPair(proj.symbol); //  установим торговую пару по умолчанию
                balance.Tokens = new string[] { symbolSelector.Trade, symbolSelector.Stable };
            }
            catch (Exception ex)
            {
                Log($"Проект загрузить не удалось ({ex.Message})");
            }                                    //  устанавливаем выбранный тикер
        }

        private void ClearProject()
        {
            fractalsSP.Children.Clear();                                    //  удаляем содержимое списка фракталов если там остались старые
        }

    }
}
