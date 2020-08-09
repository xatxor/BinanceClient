using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceCore.TelegramBot.Commands
{
    public interface ICommand
    {
        public string Name { get; }
        public bool HaveArgs { get; }
        public void Process(string[] args, long chatid);
    }
}
