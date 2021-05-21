using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PayrollLoad.ModelsDAL;
using System.Collections.Generic;
using System.IO;

namespace PayrollLoad
{
    public class PayrollContext : DbContext
    {
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentType> PaymentType { get; set; }
        public DbSet<LoadedFile> LoadedFile { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appconfig.json").Build();
            optionsBuilder.UseSqlServer(config.GetConnectionString("ConnectionStrings"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentType>().HasData(new List<PaymentType>() {
                            new PaymentType(){ Id = 1, TypeName = "Начислено" },
                            new PaymentType(){ Id = 2, TypeName = "Удержано" },
                            new PaymentType(){ Id = 3, TypeName = "Натуральных доходов" },
                            new PaymentType(){ Id = 4, TypeName = "Выплачено" }
                        }); ;
        }
    }
}
