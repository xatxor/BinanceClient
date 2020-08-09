using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceCore.TelegramBot.Commands
{
    class Buy : ICommand
    {
        public string Name { get; } = "buy";
        public bool HaveArgs { get; } = true;
        public void Process(string[] args, long chatid)
        {

        }
    }
}
