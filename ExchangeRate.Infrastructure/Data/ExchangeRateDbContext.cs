using ExchangeRate.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRate.Infrastructure.Data
{
    public class ExchangeRateDbContext : DbContext
    {
        public ExchangeRateDbContext(
            DbContextOptions<ExchangeRateDbContext> options)
            : base(options)
        {
        }

        public DbSet<CurrencyRate> CurrencyRates { get; set; }
        public DbSet<Currency> Currencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CurrencyRate>()
                .Property(x => x.BuyingRate)
                .HasPrecision(18, 6);

            modelBuilder.Entity<CurrencyRate>()
                .Property(x => x.SellingRate)
                .HasPrecision(18, 6);

            modelBuilder.Entity<Currency>()
                .HasIndex(x => x.CurrencyCode)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}