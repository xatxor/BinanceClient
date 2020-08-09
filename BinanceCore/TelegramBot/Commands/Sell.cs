using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceCore.TelegramBot.Commands
{
    class Sell : ICommand
    {
        public string Name { get; } = "sell";
        public bool HaveArgs { get; } = true;
        public void Process(string[] args, long chatid)
        {

        }
    }
}
