using Binance.Net;
using Binance.Net.Objects.Spot;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace BinanceCore
{
    public partial class App : Application
    {
        /// <summary>
        /// Инициализация приложения. Соержит настройку параметров связи с бинансом.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Настройка дефолтных параметров для клиента
            BinanceClient.SetDefaultOptions(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials("Ir1QoGFgAuLJpPnqp6z9x6wjEHinmy9yTNye46luxfKZEynU71YQDklbmIF9dWgT", "1ZKZaK4kWgtWcHo8KjKbWqCX1i7Ds2OBXK0QwfuQby0q6NGeFgGIG4soWAWwirkB"),
                LogVerbosity = LogVerbosity.Debug,
                LogWriters = new List<TextWriter> { Console.Out }
            });
            // Настройка дефолтных параметров для сокета
            BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials("Ir1QoGFgAuLJpPnqp6z9x6wjEHinmy9yTNye46luxfKZEynU71YQDklbmIF9dWgT", "1ZKZaK4kWgtWcHo8KjKbWqCX1i7Ds2OBXK0QwfuQby0q6NGeFgGIG4soWAWwirkB"),
                LogVerbosity = LogVerbosity.Debug,
                LogWriters = new List<TextWriter> { Console.Out }
            });
            base.OnStartup(e);
        }
    }
}
