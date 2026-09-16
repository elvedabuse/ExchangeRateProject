
namespace ExchangeRate.Domain.Entities
{
    public class Currency
    {
        public int Id { get; set; }

        public required string CurrencyCode { get; set; }
    }
}
