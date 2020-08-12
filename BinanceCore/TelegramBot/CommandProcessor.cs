using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Binance.Net.Objects.Spot.MarketData;

namespace BinanceCore.TelegramBot
{
    public class CommandProcessor
    {
        string[] _commands = {"buy", "sell", "graph", "bal", "base", "stop"};

        public delegate void BuyDgt(long chatid);
        public event BuyDgt Buy;

        public delegate void SellDgt(long chatid);
        public event SellDgt Sell;

        public delegate void GraphDgt(long chatid);
        public event GraphDgt Graph;

        public delegate void BalDgt(long chatid);
        public event BalDgt Bal;

        public delegate void BaseDgt(long chatid);
        public event BaseDgt Base;

        public delegate void StopDgt(long chatid);
        public event StopDgt Stop;
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
                    Buy(chatid);
                    break;
                case "sell":
                    Sell(chatid);
                    break;
                case "graph":
                    Graph(chatid);
                    break;
                case "bal":
                    Bal(chatid);
                    break;
                case "base":
                    Base(chatid);
                    break; 
                case "stop": 
                    Stop(chatid); 
                    break;
            }
        }
    }
}
