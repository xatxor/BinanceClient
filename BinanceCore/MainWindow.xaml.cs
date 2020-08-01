using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects.Spot;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using FreeImageAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using FreeImageAPI;
using System.DrawingCore;
using System.DrawingCore.Text;
using System.Windows.Media.Imaging;
using BinanceCore.Entities;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Threading;
using BinanceCore.Services;

namespace BinanceCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BinanceSocketClient socketClient = new BinanceSocketClient();
        int timeout = 30;
        int timePassed = 0;
        public MainWindow()
        {
            InitializeComponent();

            BinanceClient.SetDefaultOptions(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials("Ir1QoGFgAuLJpPnqp6z9x6wjEHinmy9yTNye46luxfKZEynU71YQDklbmIF9dWgT", "1ZKZaK4kWgtWcHo8KjKbWqCX1i7Ds2OBXK0QwfuQby0q6NGeFgGIG4soWAWwirkB"),
                LogVerbosity = LogVerbosity.Debug,
                LogWriters = new List<TextWriter> { Console.Out }
            });

            timer.Elapsed += Timer_Elapsed;
            LoadSymbols();
            LoadDefaultProject();
        }

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
                    DoEvents();
                    timer.Start();
                    DoEvents();
                    Title = "Автообновление завершено " + DateTime.Now.ToString();
                }
                else
                    intervalTB.Text = (timeout-timePassed).ToString();
            }));

        }
        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                  new Action(delegate { }));
        }
        private void FractalConfig_DeleteRequested(FractalConfiguration sender)
        {
            fractalsSP.Children.Remove(sender);
        }


        private void LoadSymbols()
        {
            Log("Loading symbols...");
            using (var client = new BinanceClient())
            {
                // Public
                var exchangeInfo = client.GetExchangeInfo();
                List<string> symbolsItems = new List<string>();
                foreach (var symbol in exchangeInfo.Data.Symbols)
                {
                    symbolsItems.Add(symbol.Name);
                }
                Symbols.ItemsSource = symbolsItems;
            }
            Log("Symbols loaded.");
        }

        private void Log(string v)
        {
            Title = v;
            DoEvents();
        }


        private void Graph_Clicked(object sender, RoutedEventArgs e)
        {
            using (var client = new BinanceClient())
            {
                var symbol = Symbols.SelectedItem.ToString();
                Log("Loading history...");
                var span = new TimeSpan(0, 1, 0, 0);
                List<Tuple<DateTime, decimal>> trades, rates;
                DateTime fin = DateTime.UtcNow;

                GetTradesAndRates(client, symbol, span, out trades, out rates, fin);
                Log("History loaded. Drawing...");

                iv.LoadBitmap(Drawing.MakeGraph(symbol, fin, span * 24, rates, trades));

                var code = Coding.MakeCode(fin, span * 24, rates);
                Log("Image ready. Code: " + code);
            }
        }



        private void GetTradesAndRates(BinanceClient client, string symbol, TimeSpan span, out List<Tuple<DateTime, decimal>> trades, out List<Tuple<DateTime, decimal>> rates, DateTime fin)
        {
            trades = new List<Tuple<DateTime, decimal>>();
            rates = new List<Tuple<DateTime, decimal>>();
            for (int h = 24; --h >= 0;)
            {
                Log("Loading history " + h);
                var end = fin.AddHours(-h);
                var start = end.AddDays(-span.TotalDays);

                var aggTrades = client.GetAggregatedTrades(symbol, startTime: start, endTime: end, limit: 1000);
                foreach (var t in aggTrades.Data)
                {
                    trades.Add(t.Quantity.at(t.TradeTime));
                    rates.Add(t.Price.at(t.TradeTime));
                }
            }
        }

        private void FindFractal_Clicked(object sender, RoutedEventArgs e)
        {
            canv.Children.Clear();  //  очистка накладки на график - той накладки, где рисуются все найденные фракталы

            foreach (var configurator in fractalsSP.Children)           //  перебор всех фракталов, загруженных в плашки конфигураций
                Drawing.DrawFoundFractals(                              //  рисуем найденные случаи очередного фрактала
                    FractalMath.FindFractal(                            //  для этого находим фрактал
                        (configurator as FractalConfiguration).Fractal, //  очередной
                        Coding.LatestCode),                             //  в коде графика
                    canv);                                              //  и отрисовываем рамки фрактала на холсте
        }


        private void addFractalB_Click(object sender, RoutedEventArgs e)
        {
            CreateFractalConfiguration();
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
                cfg.Code = def.Code;                                          //  загружаем туда код фрактала
                cfg.Title = def.Title;                                        //  устанавливаем название
                cfg.Color = def.Color.ToColor();                              //  цвет парсим из строки настройки (в настройках цвет в виде #ffffff
                cfg.Symbol = def.Symbol;                                       //  устанавливаем символ пометки фрактала
            }

            cfg.DeleteRequested += FractalConfig_DeleteRequested;
            fractalsSP.Children.Add(cfg);
            return cfg;
        }

        Timer timer = new Timer(1000); 
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

        private void SaveProjectToDefault()
        {
            List<FractalDefinition> fractalsList = new List<FractalDefinition>();
            foreach (var configurator in fractalsSP.Children)   //  перебор всех фракталов, загруженных в плашки конфигураций
                fractalsList.Add((configurator as FractalConfiguration).Fractal);  //  берём очередную конфигурацию

            var proj = new Project()
            {
                fractals = fractalsList.ToArray(),
                interval = timeout,
                ticker = Symbols.Text
            };
            proj.Save();
        }

        private void loadB_Click(object sender, RoutedEventArgs e)
        {
            try                                                                 //  При загрузке сейва могут быть ошибки, поэтому try/catch
            {
                LoadDefaultProject();
            }
            catch (Exception ex) { 
                MessageBox.Show(ex.Message,"EXCEPTION",MessageBoxButton.OK,MessageBoxImage.Error); 
            }
        }

        private void LoadDefaultProject()
        {
            var proj = Project.Load();                                      //  Загружаем объект сохранёнки
            ClearProject();
            foreach (var f in proj.fractals)                                //  перебираем все фракталы из загруженного проекта
                CreateFractalConfiguration(f);

            intervalTB.Text = proj.interval.ToString();                     //  загружаем интервал автоматического обновления
            Symbols.Text = proj.ticker;                                     //  устанавливаем выбранный тикер
        }

        private void ClearProject()
        {
            fractalsSP.Children.Clear();                                    //  удаляем содержимое списка фракталов если там остались старые
        }

        private void intervalTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            intervalTB.TrySaveInt(out timeout);
        }
    }
}
