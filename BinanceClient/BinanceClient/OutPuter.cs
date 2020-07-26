using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BinanceClient
{
    class OutPuter
    {
        public void OutPutBinanceInfoToTextbox(BinanceInfo info, TextBox textbox)
        {
            textbox.Text += info.Time.ToString() + " " + info.TradeQuantity.ToString() + " " + info.RatePrice.ToString() + Environment.NewLine;
        }
    }
}
