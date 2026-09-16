using ExchangeRate.Domain.Entities;

namespace ExchangeRate.Application.Interfaces
{
    public interface IExchangeRateRepository
    {
        Task SaveAsync(CurrencyRate currencyRate);

        Task<List<CurrencyRate>> GetAllAsync();

        Task<List<CurrencyRate>> GetByDateAsync(DateTime date);

        Task<CurrencyRate?> GetByDateAndCurrencyAsync(DateTime date, string currencyCode);

        Task<List<CurrencyRate>> GetByDateRangeAndCurrencyAsync(DateTime startDate, DateTime endDate, string currencyCode);
    }
}