using ExchangeRate.Application.DTOs;
using ExchangeRate.Application.Interfaces;
using ExchangeRate.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace ExchangeRate.Application.Services
{
    public class UpdateDailyRates
    {
        private const string DailyRatesCacheKey = "daily_rates";

        private readonly ITcmbService _tcmbService;
        private readonly IExchangeRateRepository _repository;
        private readonly IMemoryCache _cache;

        public UpdateDailyRates(
            ITcmbService tcmbService,
            IExchangeRateRepository repository,
            IMemoryCache cache)
        {
            _tcmbService = tcmbService;
            _repository = repository;
            _cache = cache;
        }

        public async Task ExecuteAsync(bool forceRefresh = false)
        {
            if (!forceRefresh &&
                _cache.TryGetValue(
                    DailyRatesCacheKey,
                    out List<ExchangeRateDto>? cachedRates))
            {
                return;
            }

            var rates = await _tcmbService.GetDailyRatesAsync();

            if (rates.Count == 0)
            {
                throw new InvalidOperationException(
                    "TCMB'den herhangi bir kur verisi alınamadı.");
            }

            foreach (var rate in rates)
            {
                var currencyRate = new CurrencyRate
                {
                    CurrencyCode = rate.CurrencyCode,
                    BuyingRate = rate.BuyingRate,
                    SellingRate = rate.SellingRate,
                    Date = rate.Date
                };

                await _repository.SaveAsync(currencyRate);
            }

            _cache.Remove("all_exchange_rates");
            _cache.Remove($"exchange_rates_{DateTime.Today:yyyy-MM-dd}");

            _cache.Set(
                DailyRatesCacheKey,
                rates,
                TimeSpan.FromMinutes(10));
        }
    }
}