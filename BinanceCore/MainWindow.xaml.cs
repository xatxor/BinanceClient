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
using BinanceCore.TelegramBot;
using System.Drawing;
using System.DrawingCore;
using System.DrawingCore.Imaging;
using System.Windows.Media.Imaging;
using FreeImageAPI;

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
        BinanceClient client;              

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

        private decimal stopBalance;
        /// <summary>
        /// Длина интервала между автоматическими обновлениями графика
        /// </summary>
        public decimal StopBalance
        {
            get => stopBalance;
            set
            {
                stopBalance = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("StopBalance"));
                }
            }
        }


        CommandProcessor processor = new CommandProcessor();
        int timePassed = 0;
        Timer timer = new Timer(1000);

        Telega _telega;
        Telega Telega
        {
            get
            {
                if (_telega == null)
                {
                    _telega = new Telega(settings.Token, settings.Master);
                    Telega.GotMessage += async (bot, msg, chatid) =>
                        await _telega.TextMessage("Бот понимает только команды, начинающиеся с /", chatid);
                    Telega.GotCommand += Bot_GotCommand;
                }
                return _telega;
            }
        }
        /// <summary>
        /// Поток параллельной загрузки символов при старте программы
        /// </summary>
        System.Threading.Thread SymbolsLoaderThr;
        System.Threading.Thread BalanceUpdaterTrh;
        System.Threading.Thread GraphThr;
        public MainWindow()
        {
            InitializeComponent();
            LoadDefaultProject();                               //  Загрузка настроек проекта
            client = new BinanceClient();
            symbolSelector.client = client;                    //  выдача показывалке балансов клиента для связи с бинансом

            timer.Elapsed += Timer_Elapsed;             //  привязка ежесекундного таймера

            SymbolsLoaderThr = new System.Threading.Thread(LoadSymbols);
            SymbolsLoaderThr.Start();

            //           symbolSelector.SetPair("LTCUSDT");                  //  установим торговую пару по умолчанию
            symbolSelector.SymbolSelected += symbolChanged;     //  При изменении выбора торговой пары

            followA.GotFall += FollowA_GotFall;
            followA.GotRise += FollowA_GotRise;
            followA.LostFall += FollowA_LostFall;
            followA.LostRise += FollowA_LostRise;
            followA.LogMsg += async (s, msg) => await Telega.TextMessageMaster($"<i>{msg}</i>");

            addFractalB.Click += (s, e) => CreateFractalConfiguration();
            symbolSelector.StableSet += symbolChanged;          //  не важно, изменилась ли вся пара или часть
            symbolSelector.TradeSet += symbolChanged;           //  нужно выполнить некоторую актуализацию

            TopLevelContainer.DataContext = this;               //  Чтобы работали биндинги
            processor.Sell += SellCommand;
            processor.Buy += BuyCommand;
            processor.Bal += BalCommand;
            processor.SetBase += SetBaseCommand;
            processor.Graph += GraphCommand;
            processor.Go += StartCommand;
            processor.Stop += StopCommand;
            processor.Status += StatusCommand;
            processor.Limit += LimitCommand;
            processor.Save += (id) => saveB_Click(null, null);

            BalanceUpdaterTrh = new System.Threading.Thread(UpdateBalance);
            BalanceUpdaterTrh.Start();

            try
            {
                Telega.TextMessageMaster("BinanceCore v.0.7 started.");
            }
            catch (Exception ex)
            {
                Log("Telegram start failure: " + ex.Message);
            }

        }

        private void LoadSymbols()
        {
            symbolSelector.LoadSymbols(new string[] { "USDT" });
        }
        private void UpdateBalance()
        {
            symbolSelector.UpdateBalance();
        }

        private void LimitCommand(decimal winRise, decimal lostRise, decimal winFall, decimal lostFall)
        {
            followA.Range = winRise;
            followA.RangeBuy = winFall;
            followA.FailRaiseLevel = lostRise;
            followA.FailFallLevel = lostFall;
        }

        private async void StatusCommand(long chatid)
        {
            await Telega.Menu(
                new string[][] { new string[] { "/sell", "/buy","/setbase" }, new string[]{"/go","/stop","/status"}, new string[] { "/save","/graph","/bal"} },
                "STATUS:\n" +
                $"Timer: {(autoCB.IsChecked==true?"ON":"OFF")}   " +
                $"Follower: {(followA.Active ? "ON" : "OFF")}\n" +
                $"Price base/last: {followA.BasePrice.ToString().TrimEnd('0')} / {LastPrice.ToString().TrimEnd('0')}\n" +
                $"Mode: {followA.mode}\n"+
                $"Win if trade now: {followA.WouldWin}\n" +
                $"Rise 🌟:{followA.Range} 🔴:{ followA.FailRaiseLevel}\n" +
                $"Fall 🌟: {followA.RangeBuy} 🔴: {followA.FailFallLevel}\n"
                ,chatid);
        }

        async private void Bot_GotCommand(Telega _bot, string cmd, string[] args, long chatid)
        {
            await Application.Current.Dispatcher.Invoke(async () =>
            {
                if (processor.CanProcess(cmd))
                    processor.ProcessCommand(cmd, args, chatid);
                else
                    await _bot.TextMessage("Не удалось найти команду " + cmd, chatid);
            });
        }

        private void symbolChanged(string data)
        {
            cache.Clear();
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
            await Alert("<code>🔴Rise Lost! " + LastPriceTrimmed + " SOLD</code>");
            SellBTCClicked(null, null);
        }

        private string LastPriceTrimmed =>
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
                await Telega.TextMessageMaster(msg);
                lastAlertTime = DateTime.Now;
            }
        }
        private async void FollowA_LostFall(Controls.FollowerAnalyzer sender)
        {
            await Alert("<code>🔴Fall Lost! " + LastPriceTrimmed + " BOUGHT</code>");
            BuyBTCClicked(null, null);
        }

        private async void FollowA_GotRise(Controls.FollowerAnalyzer sender)
        {
            await Telega.TextMessageMaster("<code>🌟 Win Rise! " + LastPriceTrimmed + "</code>");
            SellBTCClicked(null, null);
            System.Threading.Thread.Sleep(1000);
            symbolSelector.UpdateBalance();
            await ReportBalance();
        }

        private async Task ReportBalance()
        {
            await Telega.TextMessageMaster("<code>"+
                symbolSelector.BalInfo+
                "</code>");
        }

        private async void FollowA_GotFall(Controls.FollowerAnalyzer sender)
        {
            await Telega.TextMessageMaster("<code>🌟 Win Fall! " + LastPriceTrimmed + "</code>");
            BuyBTCClicked(null, null);
            System.Threading.Thread.Sleep(1000);
            symbolSelector.UpdateBalance();
            await ReportBalance();
        }
        #endregion

        /// <summary>
        /// Срабатывает каждую секунду, отображает отсчёт и вызывает автообновление
        /// с заданной периодичностью
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                if (++timePassed > Timeout)                         //  Сравнение времени с прошлого обнволения с заданным периодом
                {                                                   //  если прошло достаточно времени
                    timePassed = 0;                                 //      сбрасывается счётчик секунд
                    timer.Stop();                                   //      таймер останавливается на время обновления
                    AutoUpdate();                                   //      выполняется обновлене
                    if(followA.Active && symbolSelector.Total<StopBalance)            //      если баланс не достиг дна
                    {
                        followA.Active = false;
                        Sell();
                        Telega.TextMessageMaster("🔴🔴🔴 WARNING 🔴🔴🔴\nStop Balance!\nTrading terminated.\nRun to stable.");
                        ReportBalance();
                    }
                    timer.Start();                              //      таймер запускается снова
                }
                else                                                //  если же времени прошло недостаточно
                    autoCB.Content = $"{(Timeout - timePassed)}";   //      то просто обновим индикатор отсчёта
            }));

        }

        /// <summary>
        /// По таймеру обновляется график, при этом в Follower отправляется свежий курс и обновляется баланс
        /// </summary>
        private void AutoUpdate()
        {
            try
            {
                autoCB.Content = "⏳";
                Graph_Clicked(null, null);
                this.DoEvents();
                autoCB.Content = "✓";
                symbolSelector.UpdateBalance();
            }catch(Exception ex)
            {
                Log(ex.Message);
            }
        }

        private void Log(string v)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                v = v ?? "### NULL STRING ###";
                Title = v;
                Console.WriteLine(v);
                this.DoEvents();
            });
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
            if (GraphThr != null) return;
            GraphThr = new System.Threading.Thread(UpdateGraphic);
            GraphThr.Start();
        }

        private void UpdateGraphic()
        {
            var bmp = GetGraph();
            Application.Current.Dispatcher.Invoke(() =>
            {
                iv.LoadBitmap(bmp);
                followA.PriceUpdate(LastPrice);
                symbolSelector.LastPrice = LastPrice;
                canv.Children.Clear();  //  очистка накладки на график - той накладки, где рисуются все найденные фракталы
                foreach (var configurator in fractalsSP.Children)           //  перебор всех фракталов, загруженных в плашки конфигураций
                    Drawing.DrawFoundFractals(                              //  рисуем найденные случаи очередного фрактала
                        FractalMath.FindFractal(                            //  для этого находим фрактал
                            (configurator as FractalConfiguration).FractalDefinition, //  очередной
                            Coding.LatestCode),                             //  в коде графика
                        canv);
            });
            GraphThr = null;
        }

        private Bitmap GetGraph()
        {
            Log("Loading history...");
            DateTime fin = LastMoment.AddSeconds(-0.0001);
            var graphDuration = new TimeSpan(1, 0, 0, 0);

            lock (cache)
            {
                cache.RemoveAll(r => r.Time < fin.AddDays(-graphDuration.TotalDays));       //   убираем из кэша устаревшие данные (те что в прошлом за пределами графика)
            }
            long _last = LastCached;
            var BinanceInfo = repos.GetRangeOfElementsByTime(                           // Выгрузим из БД записи по дате
                    fin, DateTime.UtcNow, SelectedPair, true)   // до текущего момента на длину графика заданную пару с учётом полноты SHORT
                .Where(tr => tr.Id > _last);                                       // и выберем оттуда только те записи, у которых номера больше, чем нам уже известны и есть в кэше
            lock (cache)
            {
                cache.AddRange(BinanceInfo);                                                // добавляем свежие данные в кэш
            }
            Log("History loaded. Drawing...");
            Coding.MakeCode(fin, graphDuration, cache);
            Bitmap bmp = Drawing.MakeGraph("", fin, graphDuration, cache);
            Log("Done");
            return bmp;
        }

        private async void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            timePassed = 0;
            timer.Enabled = autoCB.IsChecked == true;
            if (!timer.Enabled)
                autoCB.Content = "AUTO";
            await Telega.TextMessageMaster($"AUTO: {(autoCB.IsChecked == true ? "ON" : "OFF")}");
        }

        private async void saveB_Click(object sender, RoutedEventArgs e)
        {
            SaveProjectToDefault();
            Log($"Project Saved! ({DateTime.Now.ToString("HH:mm:ss")})");
            await Telega.TextMessageMaster("Project saved");
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

        private async void SetBaseCommand(long chatid)
        {
            await Telega.TextMessage("Базовая цена была " + followA.BasePrice, chatid);
            followA.BasePrice=LastPrice;
        }
        private async void GraphCommand(long chatid)
        {
            var bmp = GetGraph();

            var bg = new Bitmap(bmp);                       //  Создадим битмап с аналогичными графику параметрами
            using (var g = Graphics.FromImage(bg))          //  сделаем из него холст
            {
                g.Clear(System.DrawingCore.Color.Black);    //  и кинем туда чёрный фон
                g.DrawImage(bmp, 0, 0);                      //  сверху наложим график
            }

            using (MemoryStream stream = new MemoryStream())
            {
                bg.Save(stream, ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);
                await Telega.PhotoMessage(stream, chatid, "График на данный момент - " + DateTime.UtcNow);
            }
        }
        private async void StopCommand(long chatid)
        {
            await Telega.TextMessage("Stoping auto update", chatid);
            followA.Active = false;
        }
        private async void BalCommand(long chatid)
        {
            await ReportBalance();
        }
        private async void StartCommand(long chatid)
        {
            await Telega.TextMessage("Starting auto update", chatid);
            followA.Active = true;
        }
        private async void SellCommand(long chatid)
        {
            await Telega.TextMessage($"Продаю {TradingToken}", chatid);
            var data = Sell();
            if (data == null)
                await Telega.TextMessage($"Не могу продать", chatid);
            else
                await Telega.TextMessage("Продано!", chatid);
        }
        private void SellBTCClicked(object sender, RoutedEventArgs e)
        {
            Sell();
        }

        private BinancePlacedOrder Sell()
        {
            try
            {
                var bal = GetBalance(TradingToken);
                bal = ((int)(bal * 10000)) / 10000M;

                var res = client.PlaceOrder(SelectedPair, // торговую монету в паре
                    Binance.Net.Enums.OrderSide.Sell, //  продаём
                    Binance.Net.Enums.OrderType.Market,
                    bal); //  по доступной цене
                if (res.Data != null)
                {
                    followA.Mode = Controls.Mode.WAIT_FALL;
                    followA.BasePrice = LastPrice;
                }
                else
                    Task.Run(() =>
                    {
                        return Telega.TextMessageMaster(
                                $"Не могу продать {bal} {TradingToken}");
                    });
                Console.Write(res.ToString());
                return res.Data;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return null;
            }
        }

        #region Быстрый доступ на чтение к выбранной паре токенов
        private string SelectedPair
        {
            get
            {
                var s = "";
                Application.Current.Dispatcher.Invoke(() => s = symbolSelector.Symbol);
                return s;
            }
        }
        private string TradingToken
        { 
            get
            {
                var s = "";
                Application.Current.Dispatcher.Invoke(()=>s=symbolSelector.Trade);
                return s;
            }
        }
        private string StableToken
        {
            get
            {
                var s = "";
                Application.Current.Dispatcher.Invoke(() => s = symbolSelector.Stable);
                return s;
            }
        }
        #endregion

        private async void BuyCommand(long chatid)
        {
            await Telega.TextMessage($"Покупаю {TradingToken}", chatid);
            var data = Buy();
            if (data == null)
                await Telega.TextMessage($"Не могу купить", chatid);
            else
                await Telega.TextMessage("Куплено!", chatid);
        }
        private void BuyBTCClicked(object sender, RoutedEventArgs e)
        { 
            Buy();
        }
        #endregion

        private BinancePlacedOrder Buy()
        {
            try
            {
                var bal = GetBalance(StableToken);
                bal *= 0.99M;
                var will = ((int)(10000 * bal / LastPrice)) / 10000M;
                var res = client.PlaceOrder(SelectedPair,                               //  торговую монету в паре
                    Binance.Net.Enums.OrderSide.Buy,                   //  покупаем
                    Binance.Net.Enums.OrderType.Market,
                    will);    //  по доступной цене
                if (res.Data != null)
                {
                    followA.Mode = Controls.Mode.WAIT_RISE;
                    followA.BasePrice = LastPrice;
                }
                else
                    Task.Run(() =>
                    {
                        return Telega.TextMessageMaster($"Не могу купить {will} {TradingToken}");
                    });
                return res.Data;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return null;
            }
        }

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
                cfg.Code = def.Code.GoodPoint();                                          //  загружаем туда код фрактала
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
                symbol = symbolSelector.Symbol,
                FailFall=followA.FailFallLevel,
                FailRise=followA.FailRaiseLevel,
                WinFall=followA.RangeBuy,
                WinRise=followA.Range,
                LastMode=followA.Mode,
                BasePrice=followA.BasePrice,
                Secret=settings.Secret,
                Token=settings.Token,
                Master=settings.Master,
                Key=settings.Key,
                StopBalance=StopBalance
        };
            proj.Save();
        }

        private void LoadDefaultProject()
        {
            try
            {
                var proj = Project.Load();                                      //  Загружаем объект сохранёнки
                fractalsSP.Children.Clear();                                    //  удаляем содержимое списка фракталов если там остались старые
                foreach (var f in proj.fractals)                                //  перебираем все фракталы из загруженного проекта
                    CreateFractalConfiguration(f);

                Timeout = proj.interval;                     //  загружаем интервал автоматического обновления
                symbolSelector.SetPair(proj.symbol); //  установим торговую пару по умолчанию
                followA.FailFallLevel=proj.FailFall;
                followA.FailRaiseLevel = proj.FailRise;
                followA.RangeBuy = proj.WinFall;
                followA.Range = proj.WinRise;
                followA.Mode = proj.LastMode;
                followA.BasePrice=proj.BasePrice;

                settings.Secret = proj.Secret;
                settings.Key = proj.Key;
                settings.Token = proj.Token;
                settings.Master = proj.Master;
                StopBalance = proj.StopBalance;

                _telega = null;


                var binanceClientID = settings.Key;
                var binanceSecret = settings.Secret;

                BinanceClient.SetDefaultOptions(new BinanceClientOptions()
                {
                    ApiCredentials = new ApiCredentials(binanceClientID, binanceSecret),
                    LogVerbosity = LogVerbosity.Debug,
                    LogWriters = new List<TextWriter> { Console.Out }
                });
                BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
                {
                    ApiCredentials = new ApiCredentials(binanceClientID, binanceSecret),
                    LogVerbosity = LogVerbosity.Debug,
                    LogWriters = new List<TextWriter> { Console.Out }
                });

                client = new BinanceClient();
            }
            catch (Exception ex)
            {
                Log($"Проект загрузить не удалось ({ex.Message})");
            }                                    //  устанавливаем выбранный тикер
        }

    }
}
