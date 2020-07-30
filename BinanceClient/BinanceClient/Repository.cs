using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public IEnumerable<long> IdOfInfoNotInBd(ApplicationContext context, IEnumerable<BinanceInfo> ieinfo)
        {
            var newIDs = ieinfo.Select(u => u.Id).Distinct().ToArray();
            var infoInDb = context.BinanceInfo.Where(u => newIDs.Contains(u.Id))
                .Select(u => u.Id).ToArray();
            var usersNotInDb = newIDs.Where(u => !infoInDb.Contains(u));
            return usersNotInDb;
        }
        public void AddBinanceInfo(IEnumerable<BinanceInfo> ieinfo)
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                var ids = IdOfInfoNotInBd(context, ieinfo);
                context.BinanceInfo.AddRange(ieinfo.Where(u => ids.Contains(u.Id)));
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
