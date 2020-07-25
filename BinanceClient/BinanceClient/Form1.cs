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
        List<Tuple<DateTime, decimal>> trades = new List<Tuple<DateTime, decimal>>();
        List<Tuple<DateTime, decimal>> rates = new List<Tuple<DateTime, decimal>>();
        public Form1()
        {
            InitializeComponent();

            using (var client = new Binance.Net.BinanceClient())
            {
                foreach (var symbol in client.GetExchangeInfo().Data.Symbols)
                    SymbolsComboBox.Items.Add(symbol.Name);
            }
        }


        private void UnloadButton_Click(object sender, EventArgs e)
        {
            using (var client = new Binance.Net.BinanceClient())
            {
                unloader.GetTradesAndRates(client, SymbolsComboBox.SelectedItem.ToString(), ref trades, ref rates, StartTime.Value, EndTime.Value);
            }

            foreach (var item in rates)
            {
                UnloadedInfoTextBox.Text += item.ToString().Trim(new char[]{'(', ')'}) + Environment.NewLine;
            }
        }

        private void AutoUnloadCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
