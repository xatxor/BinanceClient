using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore;

namespace BinanceCore
{
    class ApplicationContext : DbContext
    {
        public DbSet<BinanceInfo> BinanceInfo { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=116.203.82.48;UserId=binance;Password=binance;database=binance;");
        }
    }
}

