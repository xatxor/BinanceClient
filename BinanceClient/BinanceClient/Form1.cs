using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Binance.Net;
using Binance.Net.Objects.Spot.MarketData;

namespace BinanceClient
{
    public partial class Form1 : Form
    {
        private Unloader unloader = new Unloader();
        private Repository repos = new Repository();
        private OutPuter outputer = new OutPuter();
        private string SelectedSymbol => SymbolsComboBox.SelectedItem.ToString();

        public Form1()
        {
            InitializeComponent();

            using (var client = new Binance.Net.BinanceClient())
            {
                foreach (var symbol in client.GetExchangeInfo().Data.Symbols)
                    SymbolsComboBox.Items.Add(symbol.Name);
            }

            SymbolsComboBox.SelectedItem =
                "BTCUSDT"; // выберем сразу хоть что-то чтобы не рухнуло если нажать выгрузку    
            StartTime.Value = DateTime.UtcNow.AddHours(-1); // выберем сразу последний час по UTC
            EndTime.Value = DateTime.UtcNow; // там записи имеют время в UTC чтобы весь мир пользовался

            AutoUnloadButton.Enabled = false;

            Log("Привет! Загрузчик готов к работе! v.0.1");
        }

        private void Log(string msg)
        {
            outputer.Log(msg, UnloadedInfoTextBox);
        }

        private void UnloadButton_Click(object sender, EventArgs e)
        {
            try
            {
                Log($"Начинаем загрузку {SelectedSymbol} с {StartTime.Value} по {EndTime.Value}");
                using (var client = new Binance.Net.BinanceClient())
                {
                    var tradesAndRates = unloader.GetTradesAndRates(client, SelectedSymbol, StartTime.Value, EndTime.Value);
                    Log($"Данные с сервера получены ({tradesAndRates.Count()}). Начинаем сохранять...");
                    SaveTradesAndRates(tradesAndRates);
                    Log("Готово!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ОШИБКА: {FullMessage(ex)}");
            }
        }


        private static string FullMessage(Exception ex)
        {
            return ex.Message+(ex.InnerException==null?".":(FullMessage(ex.InnerException)));
        }

        private void AutoUnloadCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (repos.IsHaveInfo())
            {
                Log("База готова к пополнению");
                if (AutoUnloadCheckBox.Checked)
                {
                    UnloadButton.Enabled = false;
                    AutoUnloadButton.Enabled = true;
                }
                else
                {
                    timer1.Stop();
                    Timeout = 0;
                    UnloadButton.Enabled = true;
                    AutoUnloadButton.Enabled = false;
                }
            }

            else
            {
                MessageBox.Show("База данных пока что пуста, загрузите данные за какой-либо период времени");
            }
        }

        private void AutoUnloadButton_Click(object sender, EventArgs e)
        {
            //1 минута - 60000 миллисекунд
            timer1.Interval = 1000;
            timer1.Start();
            Log("Поехали!");
        }
        int TimeLimit => Convert.ToInt32(TimeoutTextBox.Text);
        int timeout = 0;
        int Timeout
        {
            set { timeout = value; Text = $"BinanceClient {(timeout>0?timeout.ToString():"")}"; }
            get => timeout;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (++Timeout < TimeLimit)
                return;
            timer1.Stop();  //  притормозим таймер, ато не известно, успеем ли принять свежие данные за время интервала
            Timeout = 0;

            Log("Сработал таймер!");
            try
            {
                using (Binance.Net.BinanceClient client = new Binance.Net.BinanceClient())
                {
                    var info = repos.GetLastElement();
                    DateTime start = info.Time;
                    DateTime end = DateTime.UtcNow;
                    if (start.AddHours(1).CompareTo(end) < 0)
                    {
                        end = start.AddHours(1);
                    }
                    double count = unloader.GetTradesAndRates(client, SymbolsComboBox.SelectedItem.ToString(), start, end).Count();
                    //проверяем, успеет ли unloader загрузить данные сервера меньше чем за интервал таймера
                    //550 - приблительное количество записей, которое успевает прогрузить unloader за одну минуту
                    while (count > timer1.Interval * 550)
                    {
                        end.AddMinutes(-5);
                        count = unloader.GetTradesAndRates(client, SymbolsComboBox.SelectedItem.ToString(), start, end).Count();
                    }

                    //проверяем, не больше ли 1000 записей в этом промежутке времени
                    while (count > 1000)
                    {
                        end.AddMinutes(-5);
                        count = unloader.GetTradesAndRates(client, SymbolsComboBox.SelectedItem.ToString(), start, end).Count();
                    }
                    var symbol = SymbolsComboBox.SelectedItem.ToString();
                    IEnumerable<BinanceAggregatedTrade> tradesAndRates;
                    tradesAndRates = unloader.GetTradesAndRates(client, symbol, start.AddMilliseconds(1), end);
                    Log($"Данные с сервера получены ({tradesAndRates.Count()}). Начинаем сохранять...");
                    SaveTradesAndRates(tradesAndRates);
                    Log("Готово!");
                }
            }catch(Exception ex)
            {
                Log("EXCEPTION: " + FullMessage(ex));
            }
            timer1.Start();
        }

        private void SaveTradesAndRates(IEnumerable<BinanceAggregatedTrade> tradesAndRates)
        {
            repos.AddBinanceInfo(tradesAndRates?
                        .Select(t =>
                            new BinanceInfo(t.AggregateTradeId, t.TradeTime, SelectedSymbol, t.Quantity, t.Price)
                            ),fullCB.Checked

                    );
        }

        private void FromDBButton_Click(object sender, EventArgs e)
        {
            try
            {

                var ieinfo = repos.GetRangeOfElementsByTime(StartTime.Value, EndTime.Value, SelectedSymbol);
                outputer.Log($"В БД найдено {ieinfo.Count()} записей:", UnloadedInfoTextBox);
                outputer.OutPutBinanceInfoToTextbox(ieinfo, UnloadedInfoTextBox);
            }
            catch(Exception ex)
            { Log($"EXCEPTION: {FullMessage(ex)}"); }
        }
    }

}
