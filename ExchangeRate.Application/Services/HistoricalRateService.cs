using ExchangeRate.Application.Interfaces;
using ExchangeRate.Domain.Entities;

namespace ExchangeRate.Application.Services
{
    public class HistoricalRateService
    {
        private readonly IExchangeRateRepository _exchangeRateRepository;
        private readonly ITcmbService _tcmbService;

        public HistoricalRateService(
            IExchangeRateRepository exchangeRateRepository,
            ITcmbService tcmbService)
        {
            _exchangeRateRepository = exchangeRateRepository;
            _tcmbService = tcmbService;
        }

        public async Task<CurrencyRate?> GetRateAsync(
            string? date,
            string? currencyCode)
        {
            if (string.IsNullOrWhiteSpace(date))
            {
                throw new ArgumentException(
                    "Tarih belirtilmelidir. Örnek: 2026-09-01");
            }

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

            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                throw new ArgumentException(
                    "Para birimi kodu boş olamaz. Örnek: USD");
            }

            var normalizedCurrencyCode =
                currencyCode.Trim().ToUpperInvariant();

            if (normalizedCurrencyCode.Length != 3)
            {
                throw new ArgumentException(
                    "Para birimi kodu 3 harften oluşmalıdır. Örnek: USD");
            }

            if (!normalizedCurrencyCode.All(char.IsLetter))
            {
                throw new ArgumentException(
                    "Para birimi kodu yalnızca harflerden oluşmalıdır. Örnek: USD");
            }

            var currencyExists =
                await _tcmbService.CurrencyExistsAsync(
                    normalizedCurrencyCode);

            if (!currencyExists)
            {
                throw new ArgumentException(
                    "Bu para birimi TCMB tarafından desteklenmiyor.");
            }

            var existingRate =
                await _exchangeRateRepository.GetByDateAndCurrencyAsync(
                    parsedDate,
                    normalizedCurrencyCode);

            if (existingRate != null)
            {
                return existingRate;
            }

            var rates =
                await _tcmbService.GetRatesByDateAsync(parsedDate);

            var rate = rates.FirstOrDefault(x =>
                x.CurrencyCode == normalizedCurrencyCode);

            if (rate == null)
            {
                return null;
            }

            var currencyRate = new CurrencyRate
            {
                CurrencyCode = rate.CurrencyCode,
                BuyingRate = rate.BuyingRate,
                SellingRate = rate.SellingRate,
                Date = rate.Date
            };

            await _exchangeRateRepository.SaveAsync(currencyRate);

            return currencyRate;
        }
    }
}
