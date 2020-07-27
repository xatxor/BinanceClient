using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Binance.Net;

namespace BinanceClient
{
    public partial class Form1 : Form
    {
        private Unloader unloader = new Unloader();
        private Repository repos = new Repository();
        private OutPuter outputer = new OutPuter();

        public Form1()
        {
            InitializeComponent();

            using (var client = new Binance.Net.BinanceClient())
            {
                foreach (var symbol in client.GetExchangeInfo().Data.Symbols)
                    SymbolsComboBox.Items.Add(symbol.Name);
            }

            SymbolsComboBox.SelectedItem =
                "ETHBTC"; // выберем сразу хоть что-то чтобы не рухнуло если нажать выгрузку    
            StartTime.Value = DateTime.UtcNow.AddHours(-1); // выберем сразу последний час по UTC
            EndTime.Value = DateTime.UtcNow; // там записи имеют время в UTC чтобы весь мир пользовался

            AutoUnloadButton.Enabled = false;
        }


        private void UnloadButton_Click(object sender, EventArgs e)
        {
            try
            {
                var symbol = SymbolsComboBox.SelectedItem.ToString();
                using (var client = new Binance.Net.BinanceClient())
                {
                    var tradesAndRates = unloader.GetTradesAndRates(client, symbol, StartTime.Value, EndTime.Value);
                    if (tradesAndRates != null)
                    {
                        foreach (var t in tradesAndRates)
                            repos.AddBinanceInfo(new BinanceInfo(t.TradeTime, symbol, Convert.ToInt32(t.Quantity), t.Price));
                    }
                }

                List<BinanceInfo> ieinfo = repos.GetRangeOfElementsByTime(StartTime.Value, EndTime.Value, symbol).ToList();
                if (ieinfo.Any())
                {
                    foreach (var item in ieinfo)
                        outputer.OutPutBinanceInfoToTextbox(item, UnloadedInfoTextBox);
                }
                else
                    MessageBox.Show("За этот период записей нет");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ОШИБКА: {ex.Message}");
            }
        }

        private void AutoUnloadCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (repos.IsHaveInfo())
            {
                if (AutoUnloadCheckBox.Checked)
                {
                    UnloadButton.Enabled = false;
                    AutoUnloadButton.Enabled = true;
                }
                else
                {
                    timer1.Stop();
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
            timer1.Interval = Convert.ToInt32(TimeoutTextBox.Text) * 60000;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            using (Binance.Net.BinanceClient client = new Binance.Net.BinanceClient())
            {
                var info = repos.GetLastElement();
                DateTime start = info.Time;
                DateTime end = DateTime.UtcNow;
                if(start.AddHours(1).CompareTo(end) < 0)
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
                var tradesAndRates = unloader.GetTradesAndRates(client, symbol, start.AddMilliseconds(1), end);
                if (tradesAndRates != null)
                {
                    foreach (var t in tradesAndRates)
                        repos.AddBinanceInfo(new BinanceInfo(t.TradeTime, symbol, Convert.ToInt32(t.Quantity), t.Price));
                }

                List<BinanceInfo> ieinfo = repos.GetRangeOfElementsByTime(start, symbol).ToList();
                foreach (var i in ieinfo)
                    outputer.OutPutBinanceInfoToTextbox(i, UnloadedInfoTextBox);
            }
        }
    }

}
