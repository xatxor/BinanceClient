using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace BinanceCore.TelegramBot
{
    class Telega
    {
        public delegate void GotCommandDgt(Telega _bot, string cmd, string[] args, long chatid);
        public event GotCommandDgt GotCommand;

        public delegate void GotMessageDgt(Telega _bot, string msg, long chatid);
        public event GotMessageDgt GotMessage;

        public delegate void LogDgt(string msg);
        public event LogDgt Log;

        private TelegramBotClient _bot;
        long _master;
        int lastMessageID = -1;
        private CommandProcessor processor=new CommandProcessor();
        public Telega(string key, long master)
        {
            try
            {
                _bot = new TelegramBotClient(key);
                _master = master;
                _bot.OnMessage += OnMessage;
                _bot.OnCallbackQuery += OnCallback;
                _bot.StartReceiving();
                Log += Bot_Log;
            }
            finally { }
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
                var args = message.Substring(cmd.Length + 1).Split(' ', StringSplitOptions.RemoveEmptyEntries);

                GotCommand?.Invoke(this, cmd, args, chatid);
            }
            else
                GotMessage?.Invoke(this, message, chatid);
        }


        async private void Bot_Log(string msg)
        {
            await TextMessageMaster(msg);
        }

        async internal Task<int> TextMessage(string v, long chatid)
        {
            if (_bot == null) return 0;
            try
            {
                var sentMsg = await _bot.SendTextMessageAsync(chatid, v, Telegram.Bot.Types.Enums.ParseMode.Html, true);
                UpdateLastMessageID(sentMsg);
                return sentMsg.MessageId;
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        async internal Task<int> PhotoMessage(Stream stream, long chatid, string caption = null)
        {
            if (_bot == null) return 0;

            try
            {
                var sentMsg = await _bot.SendPhotoAsync(chatid, stream, caption);
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
            return await TextMessage(v, _master);
        }

        async internal Task<int> PhotoMessageMaster(Stream stream, string caption = null)
        {

            return await PhotoMessage(stream, _master, caption);
        }

        private void UpdateLastMessageID(Message sentMsg)
        {
            if (lastMessageID < sentMsg.MessageId) lastMessageID = sentMsg.MessageId;
        }

        internal async Task CallbackMenu(string[][] v, string text, int to)
        {
            if (_bot == null) return;

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

        internal async Task Menu(string[][] mainMenu, string v, long chatID)
        {
            if (_bot == null) return;

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
            await _bot.SendTextMessageAsync(new Telegram.Bot.Types.ChatId(chatID), v,
            Telegram.Bot.Types.Enums.ParseMode.Html, true, false, 0, kb);
        }
    }
}
