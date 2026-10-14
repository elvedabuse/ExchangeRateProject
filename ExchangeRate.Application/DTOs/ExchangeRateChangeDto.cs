namespace ExchangeRate.Application.DTOs
{
    public class ExchangeRateChangeDto
    {
        public DateTime Date { get; set; }

        public decimal BuyingRate { get; set; }

        public decimal BuyingChange { get; set; }

        public decimal SellingRate { get; set; }

        public decimal SellingChange { get; set; }
    }
}