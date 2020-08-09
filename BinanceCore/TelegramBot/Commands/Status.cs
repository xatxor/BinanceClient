using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceCore.TelegramBot.Commands
{
    class Status : ICommand
    {
        public string Name { get;} = "status";
        public bool HaveArgs { get; } = false;
        public void Process(string[] args, long chatid)
        {

        }
    }
}
