using ExchangeRate.Domain.Entities;

namespace ExchangeRate.Application.Interfaces
{
    public interface ICurrencyRepository
    {
        Task<bool> AddAsync(Currency currency);

        Task<List<Currency>> GetAllAsync();
    }
}