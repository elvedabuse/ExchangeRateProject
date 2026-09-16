using ExchangeRate.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeRate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly UpdateDailyRates _updateDailyRates;
        private readonly HistoricalRateService _historicalRateService;
        private readonly ExchangeRateService _exchangeRateService;

        public ExchangeRatesController(
            UpdateDailyRates updateDailyRates,
            HistoricalRateService historicalRateService,
            ExchangeRateService exchangeRateService)
        {
            _updateDailyRates = updateDailyRates;
            _historicalRateService = historicalRateService;
            _exchangeRateService = exchangeRateService;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateRates()
        {
            try
            {
                await _updateDailyRates.ExecuteAsync(forceRefresh: true);

                return Ok("Günlük kurlar başarıyla güncellendi.");
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    "Döviz kurları güncellenirken bir hata oluştu.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRates()
        {
            var rates = await _exchangeRateService.GetAllRatesAsync();

            return Ok(rates);
        }

        [HttpGet("{date}")]
        public async Task<IActionResult> GetRatesByDate(string date)
        {
            try
            {
                var rates = await _exchangeRateService
                    .GetRatesByDateAsync(date);

                if (rates.Count == 0)
                {
                    return NotFound(
                        "Belirtilen tarihe ait kur bulunamadı.");
                }

                return Ok(rates);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("historical")]
        public async Task<IActionResult> GetHistorical(
            string? date,
            string? currencyCode)
        {
            try
            {
                var rate = await _historicalRateService.GetRateAsync(
                    date,
                    currencyCode);

                if (rate == null)
                {
                    return NotFound(
                        "Belirtilen tarih ve para birimi için kur bulunamadı.");
                }

                return Ok(rate);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}