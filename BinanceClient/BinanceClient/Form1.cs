using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
            
            SymbolsComboBox.SelectedItem = "ETHBTC";        // выберем сразу хоть что-то чтобы не рухнуло если нажать выгрузку    
            StartTime.Value = DateTime.UtcNow.AddHours(-1);   // выберем сразу последний час по UTC
            EndTime.Value = DateTime.UtcNow;              // там записи имеют время в UTC чтобы весь мир пользовался

            TimeInterval.Enabled = false;
            AutoUnloadButton.Enabled = false;
            TimeoutTextBox.Enabled = false;
        }


        private void UnloadButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var client = new Binance.Net.BinanceClient())
                {
                    unloader.GetTradesAndRates(client, SymbolsComboBox.SelectedItem.ToString(), StartTime.Value, EndTime.Value);
                }

                var info1 = repos.GetElementByTime(StartTime.Value);
                var info2 = repos.GetElementByTime(EndTime.Value);
                foreach (var item in repos.GetRangeOfElementsFromId(info1.Id, info2.Id))
                {
                    outputer.OutPutBinanceInfoToTextbox(item, UnloadedInfoTextBox);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"ОШИБКА: {ex.Message}");
            }
        }

        private void AutoUnloadCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (AutoUnloadCheckBox.Checked)
            {
                StartTime.Enabled = false;
                EndTime.Enabled = false;
                UnloadButton.Enabled = false;
                TimeInterval.Enabled = true;
                AutoUnloadButton.Enabled = true;
                TimeoutTextBox.Enabled = true;
            }
            else
            {
                timer1.Stop();
                StartTime.Enabled = true;
                EndTime.Enabled = true;
                UnloadButton.Enabled = true;
                TimeInterval.Enabled = false;
                AutoUnloadButton.Enabled = false;
                TimeoutTextBox.Enabled = false;
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
                int id = info.Id;
                unloader.GetTradesAndRates(client, SymbolsComboBox.SelectedItem.ToString(), start.AddMilliseconds(1), start.AddMinutes(Convert.ToInt32(TimeInterval.Text)));
                IEnumerable<BinanceInfo> ieinfo = repos.GetRangeOfElementsFromId(id + 1);
                foreach (var item in ieinfo)
                    outputer.OutPutBinanceInfoToTextbox(item, UnloadedInfoTextBox);
            }
        }
    }
}
