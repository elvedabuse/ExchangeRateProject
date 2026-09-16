using System;


namespace ExchangeRate.Application.DTOs
{
    public class ExchangeRateDto
    {
        public required string CurrencyCode { get; set; }

        public decimal BuyingRate { get; set; }

        public decimal SellingRate { get; set; }

        public DateTime Date { get; set; }
    }
}
