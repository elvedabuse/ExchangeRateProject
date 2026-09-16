using ExchangeRate.Application.Interfaces;
using ExchangeRate.Domain.Entities;

namespace ExchangeRate.Application.Services
{
    public class ExchangeService
    {
        private readonly IExchangeRateRepository _exchangeRateRepository;

        public ExchangeService(
            IExchangeRateRepository exchangeRateRepository)
        {
            _exchangeRateRepository = exchangeRateRepository;
        }

        public async Task<List<CurrencyRate>> GetCurrentRatesAsync()
        {
            var now = DateTime.Now;

            var effectiveDate = now.Date;

            // 15:30'dan önce bugünün kuru henüz yayınlanmış kabul edilmez.
            if (now.TimeOfDay < new TimeSpan(15, 30, 0))
            {
                effectiveDate = effectiveDate.AddDays(-1);
            }

            // Hafta sonuysa önceki güne git.
            while (effectiveDate.DayOfWeek == DayOfWeek.Saturday ||
                   effectiveDate.DayOfWeek == DayOfWeek.Sunday)
            {
                effectiveDate = effectiveDate.AddDays(-1);
            }

            var rates = await _exchangeRateRepository
                .GetByDateAsync(effectiveDate);

            return rates;
        }

        public async Task<List<CurrencyRate>> GetHistoricalRatesAsync(
            DateTime startDate,
            DateTime endDate,
            string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                throw new ArgumentException(
                    "Para birimi kodu zorunludur.");
            }

            var normalizedCurrencyCode =
                currencyCode.Trim().ToUpperInvariant();

            if (startDate > endDate)
            {
                throw new ArgumentException(
                    "Başlangıç tarihi, bitiş tarihinden büyük olamaz.");
            }

            if ((endDate.Date - startDate.Date).TotalDays > 7)
            {
                throw new ArgumentException(
                    "Tarih aralığı en fazla 7 gün olabilir.");
            }

            var rates = await _exchangeRateRepository
                .GetByDateRangeAndCurrencyAsync(
                    startDate,
                    endDate,
                    normalizedCurrencyCode);

            return rates;
        }
    }
}