using System;


namespace ExchangeRate.Domain.Entities
{
    public class CurrencyRate
    {
        public int Id { get; set; }

        public required string CurrencyCode { get; set; }

        public decimal BuyingRate { get; set; }

        public decimal SellingRate { get; set; }

        public DateTime Date { get; set; }
    }
}
