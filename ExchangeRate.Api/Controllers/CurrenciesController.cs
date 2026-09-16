using ExchangeRate.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeRate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrenciesController : ControllerBase
    {
        private readonly CurrencyService _currencyService;

        public CurrenciesController(CurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpPost]
        public async Task<IActionResult> Add(string? currencyCode)
        {
            try
            {
                await _currencyService.AddAsync(currencyCode);

                return Ok("Para birimi başarıyla eklendi.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var currencies = await _currencyService.GetAllAsync();

            return Ok(currencies);
        }
    }
}

