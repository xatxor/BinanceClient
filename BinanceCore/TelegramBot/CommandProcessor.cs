using System;
using System.Collections.Generic;
using System.Text;
using Binance.Net.Objects.Spot.MarketData;
using BinanceCore.TelegramBot.Commands;

namespace BinanceCore.TelegramBot
{
    public class CommandProcessor
    {
        Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();

        public void Register(string commandName, ICommand command)
        {
            _commands.Add(commandName, command);
        }

        public bool CanProcess(string commandname)
        {
            return _commands.ContainsKey(commandname);
        }

        public bool HaveArgs(string commandname)
        {
            return _commands[commandname].HaveArgs;
        }

        public void ProcessCommand(string commandname, string[] args, long chatid)
        {
            if (!CanProcess(commandname)) throw new ArgumentException("Возникла ошибка с командой " + commandname);
            _commands[commandname].Process(args, chatid);
        }
    }
}
