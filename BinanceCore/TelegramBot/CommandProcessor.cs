using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Binance.Net.Objects.Spot.MarketData;

namespace BinanceCore.TelegramBot
{
    public class CommandProcessor
    {
        string[] _commands = {"buy", "sell", "graph", "bal", "setbase", "stop","go","", "help"};

        public delegate void CmdDgt(long chatid);
        public event CmdDgt Stop;
        public event CmdDgt Buy;
        public event CmdDgt Sell;
        public event CmdDgt Graph;
        public event CmdDgt Bal;
        public event CmdDgt SetBase;
        public event CmdDgt Go;
        public event CmdDgt Help;
        public bool CanProcess(string commandname)
        {
            return _commands.Contains(commandname);
        }

        public void ProcessCommand(string commandname, string[] args, long chatid)
        {
                if (!CanProcess(commandname)) throw new ArgumentException("Возникла ошибка с командой " + commandname);

                switch (commandname)
                {
                    case "buy":
                        Buy?.Invoke(chatid);
                        break;
                    case "sell":
                        Sell?.Invoke(chatid);
                        break;
                    case "graph":
                        Graph?.Invoke(chatid);
                        break;
                    case "bal":
                        Bal?.Invoke(chatid);
                        break;
                    case "setbase":
                        SetBase?.Invoke(chatid);
                        break;
                    case "stop":
                        Stop?.Invoke(chatid);
                        break;
                    case "go":
                        Go?.Invoke(chatid);
                        break;
                    case "help":
                    case "":
                        Help?.Invoke(chatid);
                        break;
                }
        }
    }
}
