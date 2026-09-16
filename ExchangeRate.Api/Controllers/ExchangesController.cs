using ExchangeRate.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeRate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangesController : ControllerBase
    {
        private readonly ExchangeService _exchangeService;

        public ExchangesController(ExchangeService exchangeService)
        {
            _exchangeService = exchangeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentRates()
        {
            var rates = await _exchangeService.GetCurrentRatesAsync();

            return Ok(rates);
        }

        [HttpGet("historical")]
        public async Task<IActionResult> GetHistoricalRates(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string currencyCode)
        {
            try
            {
                var rates = await _exchangeService.GetHistoricalRatesAsync(
                    startDate,
                    endDate,
                    currencyCode);

                return Ok(rates);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }


}
