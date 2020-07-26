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

            AutoUnloadButton.Enabled = false;
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
                List<BinanceInfo> ieinfo = repos.GetRangeOfElementsFromId(info1.Id, info2.Id).ToList();
                if (ieinfo.Any())
                {
                    foreach (var item in ieinfo)
                    {
                        outputer.OutPutBinanceInfoToTextbox(item, UnloadedInfoTextBox);
                    }
                }
                else
                    MessageBox.Show("За этот период записей нет");
            }
            catch(Exception ex)
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
                int id = info.Id;
                unloader.GetTradesAndRates(client, SymbolsComboBox.SelectedItem.ToString(), start.AddMilliseconds(1), DateTime.Now);
                IEnumerable<BinanceInfo> ieinfo = repos.GetRangeOfElementsFromId(id + 1);
                foreach (var item in ieinfo)
                    outputer.OutPutBinanceInfoToTextbox(item, UnloadedInfoTextBox);
            }
        }
    }
}
