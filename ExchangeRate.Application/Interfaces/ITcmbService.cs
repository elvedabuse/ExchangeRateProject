using ExchangeRate.Application.DTOs;


namespace ExchangeRate.Application.Interfaces
{
    public interface ITcmbService
    {
        Task<List<ExchangeRateDto>> GetDailyRatesAsync();

        Task<List<ExchangeRateDto>> GetRatesByDateAsync(DateTime date);

        Task<bool> CurrencyExistsAsync(string currencyCode);
    }
}

