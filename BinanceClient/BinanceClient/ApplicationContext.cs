using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySQL;

namespace BinanceClient
{
    class ApplicationContext : DbContext
    {
        public DbSet<BinanceInfo> BinanceInfo { get; set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=localhost;UserId=admin;Password=admin;database=binanceclientbd;");
        }
    }
}
