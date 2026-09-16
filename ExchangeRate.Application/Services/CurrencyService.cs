using ExchangeRate.Application.Interfaces;
using ExchangeRate.Domain.Entities;

namespace ExchangeRate.Application.Services
{
    public class CurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository;
        private readonly ITcmbService _tcmbService;
        private readonly IExchangeRateRepository _exchangeRateRepository;
        public CurrencyService(
            ICurrencyRepository currencyRepository,
            ITcmbService tcmbService,
            IExchangeRateRepository exchangeRateRepository)
        {
            _currencyRepository = currencyRepository;
            _tcmbService = tcmbService;
            _exchangeRateRepository = exchangeRateRepository;
        
        }

        public async Task<bool> AddAsync(string? currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                throw new ArgumentException("Para birimi kodu boş olamaz.");
            }

            var normalizedCurrencyCode =
                currencyCode.Trim().ToUpperInvariant();

            var existsInTcmb =
                await _tcmbService.CurrencyExistsAsync(normalizedCurrencyCode);

            if (!existsInTcmb)
            {
                throw new ArgumentException(
                    "Bu para birimi TCMB tarafından desteklenmiyor.");
            }

            var currency = new Currency
            {
                CurrencyCode = normalizedCurrencyCode
            };

            var added = await _currencyRepository.AddAsync(currency);

            if (!added)
            {
                throw new InvalidOperationException(
                    "Bu para birimi zaten mevcut.");
            }

            var currentDate = DateTime.Today;
            var businessDays = 0;
            var daysChecked = 0;

            while (businessDays < 10)
            {
                currentDate = currentDate.AddDays(-1);

                if (currentDate.DayOfWeek == DayOfWeek.Saturday ||
                    currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }

                var rates =
                    await _tcmbService.GetRatesByDateAsync(currentDate);

                var newCurrencyRate = rates.FirstOrDefault(x =>
                    x.CurrencyCode == normalizedCurrencyCode);

                if (newCurrencyRate != null)
                {
                    var currencyRate = new CurrencyRate
                    {
                        CurrencyCode = newCurrencyRate.CurrencyCode,
                        BuyingRate = newCurrencyRate.BuyingRate,
                        SellingRate = newCurrencyRate.SellingRate,
                        Date = newCurrencyRate.Date
                    };

                    await _exchangeRateRepository.SaveAsync(currencyRate);

                    businessDays++;
                }

                daysChecked++;

                if (daysChecked > 30)
                {
                    break;
                }
            }

            return true;
        }

        public async Task<List<Currency>> GetAllAsync()
        {
            return await _currencyRepository.GetAllAsync();
        }
    }
}