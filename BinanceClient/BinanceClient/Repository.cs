using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;

namespace BinanceClient
{
    class Repository
    {
        public bool IsHaveInfo()
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                return context.BinanceInfo.Any();
            }
        }
        public void AddBinanceInfo(BinanceInfo info)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                context.BinanceInfo.Add(info);
                context.SaveChanges();
            }
        }

        public BinanceInfo GetLastElement()
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                var result = context.BinanceInfo.AsEnumerable().Last();
                return result;
            }
        }


        public IEnumerable<BinanceInfo> GetRangeOfElementsByTime(DateTime time, string symbol)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                var result = context.BinanceInfo.ToList().Where(e => (e.Time.CompareTo(time) > 0) && (e.Symbol == symbol));
                return result;
            }
        }

        public IEnumerable<BinanceInfo> GetRangeOfElementsByTime(DateTime time1, DateTime time2, string symbol)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                var result = context.BinanceInfo.ToList().Where(e => (e.Time.CompareTo(time1) > 0) && (e.Time.CompareTo(time2) < 0) && (e.Symbol == symbol));
                return result;
            }
        }
    }
}
