using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace BinanceCore.Services
{
    class Telega
    {
        public delegate void GotCommandDgt(string cmd, string args, long chatid);
        public event GotCommandDgt GotCommand;

        public delegate void GotMessageDgt(string msg, long chatid);
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
            GotMessage += Bot_GotMessage;
            GotCommand += Bot_GotCommand;
            Log += Bot_Log;
        }

        private void OnCallback(object sender, CallbackQueryEventArgs e)
        {
            MessageHandler(e.CallbackQuery.Data, e.CallbackQuery.Message.Chat.Id);
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            MessageHandler(e.Message.Text, e.Message.Chat.Id);
        }

        private void MessageHandler(string message, long chatid)
        {
            if (message.StartsWith("/"))
            {
                var cmd = (message + " ").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].ToLower().Substring(1);
                var args = message.Substring(cmd.Length + 1).Trim();

                GotCommand?.Invoke(cmd, args, chatid);
            }
            else
                GotMessage?.Invoke(message, chatid);
        }

        private void Bot_GotCommand(string cmd, string args, long chatid)
        {

        }

        private void Bot_GotMessage(string msg, long chatid)
        {
            _bot.SendTextMessageAsync(chatid, "Бот понимает только команды, начинающиеся с /");
        }

        async private void Bot_Log(string msg)
        {
            await TextMessageMaster(msg);
        }

        async internal Task<int> TextMessage(string v, int from)
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

        async internal Task<int> PhotoMessage(string photopath, int from, string caption = null)
        {
            try
            {
                var sentMsg = await _bot.SendPhotoAsync(from, photopath, caption);
                UpdateLastMessageID(sentMsg);
                return sentMsg.MessageId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        async internal Task<int> TextMessageMaster(string v)
        {
            return await TextMessage(v,_master);
        }

        async internal Task<int> PhotoMessageMaster(string photopath, string caption = null)
        {
            return await PhotoMessage(photopath, _master, caption);
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
