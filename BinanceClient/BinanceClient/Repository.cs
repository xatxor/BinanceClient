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
                var result = context.BinanceInfo.Last();
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

        public IEnumerable<BinanceInfo> GetRangeOfElementsFromId(int id)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                var result = context.BinanceInfo.ToList().Where(e => e.Id > id);
                return result;
            }
        }

        public IEnumerable<BinanceInfo> GetRangeOfElementsFromId(int id1, int id2)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                var result = context.BinanceInfo.ToList().Where(e => e.Id > id1 && e.Id < id2);
                return result;
            }
        }
    }
}
