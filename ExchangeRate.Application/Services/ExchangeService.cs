using ExchangeRate.Application.DTOs;
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

        public async Task<List<ExchangeRateChangeDto>> GetRateChangesAsync(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                throw new ArgumentException("Para birimi kodu zorunludur.");
            }

            var normalizedCurrencyCode =
                currencyCode.Trim().ToUpperInvariant();

            var now = DateTime.Now;
            var endDate = now.Date;

            // 15:30'dan önce bugünün kuru henüz güncellenmemiş kabul edilir.
            if (now.TimeOfDay < new TimeSpan(15, 30, 0))
            {
                endDate = endDate.AddDays(-1);
            }

            // Hafta sonu ise önceki iş gününe git.
            while (endDate.DayOfWeek == DayOfWeek.Saturday ||
                   endDate.DayOfWeek == DayOfWeek.Sunday)
            {
                endDate = endDate.AddDays(-1);
            }

            var startDate = endDate.AddDays(-6);

            var rates = await _exchangeRateRepository
                .GetByDateRangeAndCurrencyAsync(
                    startDate.AddDays(-1),
                    endDate,
                    normalizedCurrencyCode);

            var result = new List<ExchangeRateChangeDto>();

            CurrencyRate? previousRate = rates
                .Where(x => x.Date.Date < startDate.Date)
                .OrderByDescending(x => x.Date)
                .FirstOrDefault();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var currentRate = rates
                    .FirstOrDefault(x => x.Date.Date == date.Date);

                // O gün için kur yoksa son bilinen kuru kullan.
                if (currentRate == null)
                {
                    if (previousRate != null)
                    {
                        result.Add(new ExchangeRateChangeDto
                        {
                            Date = date,
                            BuyingRate = previousRate.BuyingRate,
                            BuyingChange = 0,
                            SellingRate = previousRate.SellingRate,
                            SellingChange = 0
                        });
                    }

                    continue;
                }

                decimal buyingChange = 0;
                decimal sellingChange = 0;

                if (previousRate != null)
                {
                    buyingChange =
                        currentRate.BuyingRate - previousRate.BuyingRate;

                    sellingChange =
                        currentRate.SellingRate - previousRate.SellingRate;
                }

                result.Add(new ExchangeRateChangeDto
                {
                    Date = currentRate.Date,
                    BuyingRate = currentRate.BuyingRate,
                    BuyingChange = buyingChange,
                    SellingRate = currentRate.SellingRate,
                    SellingChange = sellingChange
                });

                previousRate = currentRate;
            }

            return result;
        }
    }
}