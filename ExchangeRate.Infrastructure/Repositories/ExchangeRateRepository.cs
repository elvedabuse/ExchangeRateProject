using ExchangeRate.Application.Interfaces;
using ExchangeRate.Domain.Entities;
using ExchangeRate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRate.Infrastructure.Repositories
{
    public class ExchangeRateRepository : IExchangeRateRepository
    {
        private readonly ExchangeRateDbContext _context;

        public ExchangeRateRepository(ExchangeRateDbContext context)
        {
            _context = context;
        }

        public async Task<List<CurrencyRate>> GetAllAsync()
        {
            return await _context.CurrencyRates.ToListAsync();
        }

        public async Task<List<CurrencyRate>> GetByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            return await _context.CurrencyRates
                .Where(x => x.Date >= startDate && x.Date < endDate)
                .ToListAsync();
        }

        public async Task<CurrencyRate?> GetByDateAndCurrencyAsync(
            DateTime date,
            string currencyCode)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            return await _context.CurrencyRates
                .FirstOrDefaultAsync(x =>
                    x.CurrencyCode == currencyCode &&
                    x.Date >= startDate &&
                    x.Date < endDate);
        }

        public async Task<List<CurrencyRate>> GetByDateRangeAndCurrencyAsync(
            DateTime startDate,
            DateTime endDate,
            string currencyCode)
        {
            var start = startDate.Date;
            var end = endDate.Date.AddDays(1);

            return await _context.CurrencyRates
                .Where(x =>
                    x.CurrencyCode == currencyCode &&
                    x.Date >= start &&
                    x.Date < end)
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task SaveAsync(CurrencyRate currencyRate)
        {
            var startDate = currencyRate.Date.Date;
            var endDate = startDate.AddDays(1);

            var existingRate = await _context.CurrencyRates
                .FirstOrDefaultAsync(x =>
                    x.CurrencyCode == currencyRate.CurrencyCode &&
                    x.Date >= startDate &&
                    x.Date < endDate);

            if (existingRate != null)
            {
                return;
            }

            await _context.CurrencyRates.AddAsync(currencyRate);
            await _context.SaveChangesAsync();
        }
    }
}