using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BinanceCore.Services
{
    class Telega
    {
        public delegate void GotCommandDgt(string cmd, string args, int from);
        public event GotCommandDgt GotCommand;

        public delegate void GotMessageDgt(string msg, int from);
        public event GotMessageDgt GotMessage;

        public delegate void GotUserDgt(int from, string username, string firstname, string lastname);
        public event GotUserDgt GotUser;

        public delegate void LogDgt(string msg);
        public event LogDgt Log;


        Telegram.Bot.TelegramBotClient _bot;
        int _master;
        int lastMessageID = -1;

        public Telega(string key, int master)
        {
            _bot = new Telegram.Bot.TelegramBotClient(key/*,new WebProxy("116.203.82.48",8080)*/);
            _master = master;
            _bot.OnMessage += OnMessage;
            _bot.OnCallbackQuery += OnCallback;
            _bot.StartReceiving();
        }

        private void OnCallback(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            var txt = e.CallbackQuery.Data;
            if (txt.StartsWith("/"))
            {
                var cmd = (txt + " ").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].ToLower().Substring(1);
                var args = txt.Substring(cmd.Length + 1).Trim();

                GotCommand?.Invoke(cmd, args, e.CallbackQuery.From.Id);
            }
            else
                GotMessage?.Invoke(txt, e.CallbackQuery.From.Id);
        }

        private void OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
        }
        /// TODO: Добавить возможность отправки изображений

        async internal Task<int> Message(string v, int from)
        {
            try
            {
                var sentMsg = await _bot.SendTextMessageAsync(from, v, Telegram.Bot.Types.Enums.ParseMode.Html, true);
                UpdateLastMessageID(sentMsg);
                return sentMsg.MessageId;
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        async internal Task<int> MessageMaster(string v)
        {
            return await Message(v,_master);
        }

        private void UpdateLastMessageID(Message sentMsg)
        {
            if (lastMessageID < sentMsg.MessageId) lastMessageID = sentMsg.MessageId;
        }

        internal async Task CallbackMenu(string[][] v, string text, int to)
        {
            var allCallbackss = new List<List<Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton>>();
            Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup kb2 = null;

            foreach (var ss in v)
            {
                var buttons = new List<Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton>();
                foreach (var s in ss)
                    buttons.Add(new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton()
                    {
                        Text = s.Split(new char[] { '§' })[0],
                        CallbackData = s.Split(new char[] { '§' })[1]
                    });
                allCallbackss.Add(buttons);
                kb2 = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(allCallbackss);
            }
            if (text == null || text.Length == 0) text = "<b>Выберите пункт меню</b>";
            var tt = await _bot.SendTextMessageAsync(to, text,
            Telegram.Bot.Types.Enums.ParseMode.Html, true, false, 0, kb2);
        }

        internal async Task Menu(string[][] mainMenu, string v, int from)
        {
            var allbuttons = new List<List<Telegram.Bot.Types.ReplyMarkups.KeyboardButton>>();
            Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup kb = null;
            foreach (var ss in mainMenu)
            {
                var buttons = new List<Telegram.Bot.Types.ReplyMarkups.KeyboardButton>();
                foreach (var s in ss)
                    buttons.Add(new Telegram.Bot.Types.ReplyMarkups.KeyboardButton(s));
                allbuttons.Add(buttons);
                kb = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup(allbuttons, true);
            }
            await _bot.SendTextMessageAsync(new Telegram.Bot.Types.ChatId(from), v,
            Telegram.Bot.Types.Enums.ParseMode.Html, true, false, 0, kb);
        }
    }
}
