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

        public BinanceInfo GetElementByTime(DateTime time)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                var result = context.BinanceInfo.ToList().Where(e => e.Time == time).Last();
                return result;
            }
        }

        public BinanceInfo GetElementById(int id)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                var result = context.BinanceInfo.Where(e => e.Id == id).Last();
                return result;
            }
        }

        public IEnumerable<BinanceInfo> GetRangeOfElementsByTime(DateTime time)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                var result = context.BinanceInfo.ToList().Where(e => e.Time.CompareTo(time) > 0);
                return result;
            }
        }

        public IEnumerable<BinanceInfo> GetRangeOfElementsByTime(DateTime time1, DateTime time2)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                var result = context.BinanceInfo.ToList().Where(e => e.Time.CompareTo(time1) > 0 && e.Time.CompareTo(time2) < 0);
                return result;
            }
        }
    }
}
