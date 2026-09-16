using ExchangeRate.Application.Interfaces;
using ExchangeRate.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace ExchangeRate.Application.Services
{
    public class ExchangeRateService
    {
        private readonly IExchangeRateRepository _repository;
        private readonly IMemoryCache _cache;

        public ExchangeRateService(
            IExchangeRateRepository repository,
            IMemoryCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<List<CurrencyRate>> GetAllRatesAsync()
        {
            const string cacheKey = "all_exchange_rates";

            if (_cache.TryGetValue(
                    cacheKey,
                    out List<CurrencyRate>? cachedRates))
            {
                return cachedRates!;
            }

            var rates = await _repository.GetAllAsync();

            _cache.Set(
                cacheKey,
                rates,
                TimeSpan.FromMinutes(10));

            return rates;
        }

        public async Task<List<CurrencyRate>> GetRatesByDateAsync(string date)
        {
            if (!DateTime.TryParseExact(
                    date,
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var parsedDate))
            {
                throw new ArgumentException(
                    "Tarih formatı geçersiz. Tarih YYYY-MM-DD formatında olmalıdır. Örnek: 2026-09-01");
            }

            var cacheKey = $"exchange_rates_{parsedDate:yyyy-MM-dd}";

            if (_cache.TryGetValue(
                    cacheKey,
                    out List<CurrencyRate>? cachedRates))
            {
                return cachedRates!;
            }

            var rates = await _repository.GetByDateAsync(parsedDate);

            if (rates.Count > 0)
            {
                _cache.Set(
                    cacheKey,
                    rates,
                    TimeSpan.FromMinutes(10));
            }

            return rates;
        }
    }
}