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
        int maxLen = 1024 * 8;
        public void OutPutBinanceInfoToTextbox(BinanceInfo info, TextBox textbox)
        {
            Log(info.Time.ToString() + " " + info.TradeQuantity.ToString() + " " + info.RatePrice.ToString(), textbox);
        }
        public void OutPutBinanceInfoToTextbox(IEnumerable<BinanceInfo> infos, TextBox textbox)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var info in infos)
                sb.AppendLine($"at {info.Time} Price:{info.RatePrice.ToString("0000.0000000")}   Volume:{info.TradeQuantity} ");

            Log(sb.ToString(), textbox);
        }

        public void Log(string moreText, TextBox textbox)
        {
            moreText = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")+"   "+moreText+Environment.NewLine;

            if (moreText.Length > maxLen) moreText = moreText.Substring(moreText.Length - maxLen);

            int extraTextLen = textbox.Text.Length + moreText.Length - maxLen;  //  если вместе с добавкой текст будет слишком длинный, то узнаем длину "хвоста"
            if (extraTextLen > 0)                                               //  и укоротим текст на длину хвоста
                textbox.Text = textbox.Text.Substring(extraTextLen);            //  иначе со временем станут длинные логи, и они будут тормозить всё жутко
            textbox.Text += moreText;

            textbox.Select(textbox.Text.Length, 0); //  перемотка в конец текста
            textbox.ScrollToCaret();
            Application.DoEvents();
        }
    }
}
