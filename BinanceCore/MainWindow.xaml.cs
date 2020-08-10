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

///  TODO: Пора добавлять следящие ползунки, дающие сигналы торговли

namespace BinanceCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Repository repos = new Repository();
        BinanceSocketClient socketClient = new BinanceSocketClient();
        int timeout = 30;
        int timePassed = 0;
        Timer timer = new Timer(1000);

        BinanceClient client = null;
        Telega telega = new Telega("1294746661:AAGeFjeIBPTvG2pUhcdflPD4Nc_pj8ExdXI", 109159596);
        public MainWindow()
        {
            InitializeComponent();
            BinanceClient.SetDefaultOptions(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials("Ir1QoGFgAuLJpPnqp6z9x6wjEHinmy9yTNye46luxfKZEynU71YQDklbmIF9dWgT", "1ZKZaK4kWgtWcHo8KjKbWqCX1i7Ds2OBXK0QwfuQby0q6NGeFgGIG4soWAWwirkB"),
                LogVerbosity = LogVerbosity.Debug,
                LogWriters = new List<TextWriter> { Console.Out }
            });
            BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials("Ir1QoGFgAuLJpPnqp6z9x6wjEHinmy9yTNye46luxfKZEynU71YQDklbmIF9dWgT", "1ZKZaK4kWgtWcHo8KjKbWqCX1i7Ds2OBXK0QwfuQby0q6NGeFgGIG4soWAWwirkB"),
                LogVerbosity = LogVerbosity.Debug,
                LogWriters = new List<TextWriter> { Console.Out }
            });
            client = new BinanceClient();               //  создание глобального клиента для связи с бинансом
            balance.Client = client;                    //  выдача показывалке балансов клиента для связи с бинансом
            balance.Log += (sender, msg) => Log(msg);   //  привязка логов показывалки балансов к логам главного окна

            timer.Elapsed += Timer_Elapsed;             //  привязка ежесекундного таймера
            LoadDefaultProject();

            followA.GotFall += FollowA_GotFall;
            followA.GotRise += FollowA_GotRise;
            followA.LostFall += FollowA_LostFall;
            followA.LostRise += FollowA_LostRise;
            followA.LogMsg += FollowA_LogMsg;


            symbolSelector.LoadSymbols(client, new string[] { "USDT"});
            symbolSelector.SymbolSelected += symbolChanged;     //  При изменении выбора торговой пары
            symbolSelector.SetPair("LTCUSDT");                  //  установим торговую пару по умолчанию
            balance.UpdateBalance();
            symbolSelector.StableSet += symbolChanged;          //  не важно, изменилась ли вся пара или часть
            symbolSelector.TradeSet += symbolChanged;           //  нужно выполнить некоторую актуализацию

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
            await telega.MessageMaster("Курс вырос! Продавай. " + LastPriceTrimmed+"\nБудем ждать падения и купим снова.\n" + balance.BalInfo);
            SellBTCClicked(null, null);
            System.Threading.Thread.Sleep(1000);
            balance.UpdateBalance();
            await telega.MessageMaster("Теперь у нас\n" + balance.BalInfo);
        }

        private async void FollowA_GotFall(Controls.FollowerAnalyzer sender)
        {
            balance.UpdateBalance();
            await telega.MessageMaster("Курс упал! Покупай. " + LastPriceTrimmed + "\nБудем ждать роста и продадим.\n"+balance.BalInfo);
            BuyBTCClicked(null, null);
            System.Threading.Thread.Sleep(1000);
            balance.UpdateBalance();
            await telega.MessageMaster("Теперь у нас\n" + balance.BalInfo);
        }
        #endregion

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { 
                if (++timePassed > timeout)
                {
                    timePassed = 0;
                    timer.Stop();
                    Title = "АВТООБНОВЛЕНИЕ...";
                    Graph_Clicked(null, null);
                    FindFractal_Clicked(null, null);
                    this.DoEvents();
                    timer.Start();
                    this.DoEvents();
                    Title = "Автообновление завершено " + DateTime.Now.ToString();
                    FindFractal_Clicked(null, null);
                    followA.PriceUpdate(LastPrice);
                    balance.UpdateBalance();
                }
                else
                    Title = $"Timeout: {(timeout-timePassed)}";
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

        private void Graph_Clicked(object sender, RoutedEventArgs e)
        {
            Log("Loading history...");
            var span = new TimeSpan(0, 1, 0, 0);
            DateTime fin = DateTime.UtcNow;

            var BinanceInfo=GetTradesAndRates(SelectedPair, LastMoment, fin).Where(tr=>tr.Id>LastCached);
            cache.RemoveAll(r => r.Time < fin.AddDays(-1));//   убираем из кэша старые данные
            cache.AddRange(BinanceInfo);    // добавляем свежие

            Log("History loaded. Drawing...");

            iv.LoadBitmap(Drawing.MakeGraph(SelectedPair, fin, span * 24, cache));

            var code = Coding.MakeCode(fin, span * 24, cache);
            Log("Image ready. Code: " + code);
        }



        private IEnumerable<BinanceInfo> GetTradesAndRates(string symbol,DateTime start, DateTime fin)
        {
            Log($"Loading history {start}...{fin} ({fin.Subtract(start).TotalMinutes} minutes)");
            var tradesAndRates = repos.GetRangeOfElementsByTime(start, fin, symbol,shortCB.IsChecked==true);
            Log("History is loaded");
            return tradesAndRates;
        }

        #region Реакции на нажатия
        private void FindFractal_Clicked(object sender, RoutedEventArgs e)
        {
            canv.Children.Clear();  //  очистка накладки на график - той накладки, где рисуются все найденные фракталы

            foreach (var configurator in fractalsSP.Children)           //  перебор всех фракталов, загруженных в плашки конфигураций
                Drawing.DrawFoundFractals(                              //  рисуем найденные случаи очередного фрактала
                    FractalMath.FindFractal(                            //  для этого находим фрактал
                        (configurator as FractalConfiguration).FractalDefinition, //  очередной
                        Coding.LatestCode),                             //  в коде графика
                    canv);                                              //  и отрисовываем рамки фрактала на холсте
        }

        private void addFractalB_Click(object sender, RoutedEventArgs e)
        {
            CreateFractalConfiguration();
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            int.TryParse(intervalTB.Text, out timeout);
            timePassed = 0;
            timer.Enabled = (sender as CheckBox).IsChecked == true;
            if (!timer.Enabled) intervalTB.Text = timeout.ToString();
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
        private void intervalTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            timeout = intervalTB.TrySaveInt(timeout);
        }

        private async void SellBTCClicked(object sender, RoutedEventArgs e)
        {
            var bal = GetBalance(TradingToken);
            bal = ((int)(bal * 10000)) / 10000M;

            var res = client.PlaceOrder(SelectedPair,                               // торговую монету в паре
                                Binance.Net.Enums.OrderSide.Sell,                   //  продаём
                                Binance.Net.Enums.OrderType.Market,
                                bal);    //  по доступной цене
            if (res.Data == null)
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
                var will = ((int)(1000 * bal / LastPrice)) / 1000M;

                var res = client.PlaceOrder(SelectedPair,                               //  торговую монету в паре
                                    Binance.Net.Enums.OrderSide.Buy,                   //  покупаем
                                    Binance.Net.Enums.OrderType.Market,
                                    will);    //  по доступной цене
                if (res.Data == null)
                    await telega.MessageMaster($"Не могу купить {bal} {TradingToken}");

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
                interval = timeout,
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

                intervalTB.Text = proj.interval.ToString();                     //  загружаем интервал автоматического обновления
                symbolSelector.SetPair(proj.symbol); //  установим торговую пару по умолчанию
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
