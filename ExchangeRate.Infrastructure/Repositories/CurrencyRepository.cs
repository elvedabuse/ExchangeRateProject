using ExchangeRate.Application.Interfaces;
using ExchangeRate.Domain.Entities;
using ExchangeRate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRate.Infrastructure.Repositories
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly ExchangeRateDbContext _context;

        public CurrencyRepository(ExchangeRateDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(Currency currency)
        {
            var existingCurrency = await _context.Currencies
                .FirstOrDefaultAsync(x =>
                    x.CurrencyCode == currency.CurrencyCode);

            if (existingCurrency != null)
            {
                return false;
            }

            await _context.Currencies.AddAsync(currency);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Currency>> GetAllAsync()
        {
            return await _context.Currencies.ToListAsync();
        }
    }
}