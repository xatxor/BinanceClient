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

            base.OnStartup(e);
        }
    }
}
