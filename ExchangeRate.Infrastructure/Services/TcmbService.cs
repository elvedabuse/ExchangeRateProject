using ExchangeRate.Application.DTOs;
using ExchangeRate.Application.Interfaces;
using ExchangeRate.Infrastructure.Data;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRate.Infrastructure.Services;

public class TcmbService : ITcmbService
{
    private readonly HttpClient _httpClient;
    private readonly ExchangeRateDbContext _context;

    public TcmbService(
        HttpClient httpClient,
        ExchangeRateDbContext context)
    {
        _httpClient = httpClient;
        _context = context;
    }

    public async Task<List<ExchangeRateDto>> GetDailyRatesAsync()
    {
        var url = "https://www.tcmb.gov.tr/kurlar/today.xml";

        var response = await _httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync();

        var document = XDocument.Parse(xml);

        var rates = new List<ExchangeRateDto>();

        var root = document.Root;

        var dateText = root?.Attribute("Tarih")?.Value;

        if (!DateTime.TryParseExact(
                dateText,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var rateDate))
        {
            rateDate = DateTime.Today;
        }

        var allowedCurrencies = await _context.Currencies.Select(x => x.CurrencyCode).ToListAsync();

        var currencies = document.Descendants("Currency");

        foreach (var currency in currencies)
        {
            var code = currency.Attribute("Kod")?.Value;

            if (code == null || !allowedCurrencies.Contains(code))
                continue;

            var buying = currency.Element("ForexBuying")?.Value;
            var selling = currency.Element("ForexSelling")?.Value;

            if (decimal.TryParse(
                    buying,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var buyingRate) &&
                decimal.TryParse(
                    selling,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var sellingRate))
            {
                rates.Add(new ExchangeRateDto
                {
                    CurrencyCode = code!,
                    BuyingRate = buyingRate,
                    SellingRate = sellingRate,
                    Date = rateDate
                });
            }
        }

        return rates;
    }

    public async Task<List<ExchangeRateDto>> GetRatesByDateAsync(DateTime date)
    {
        var url = $"https://www.tcmb.gov.tr/kurlar/{date:yyyyMM}/{date:ddMMyyyy}.xml";

        var response = await _httpClient.GetAsync(url);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new List<ExchangeRateDto>();
        }

        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync();

        var document = XDocument.Parse(xml);

        var rates = new List<ExchangeRateDto>();

        var allowedCurrencies = await _context.Currencies.Select(x => x.CurrencyCode).ToListAsync();

        var currencies = document.Descendants("Currency");

        foreach (var currency in currencies)
        {
            var code = currency.Attribute("Kod")?.Value;

            if (code == null || !allowedCurrencies.Contains(code))
                continue;

            var buying = currency.Element("ForexBuying")?.Value;
            var selling = currency.Element("ForexSelling")?.Value;

            if (decimal.TryParse(
                    buying,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var buyingRate) &&
                decimal.TryParse(
                    selling,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var sellingRate))
            {
                rates.Add(new ExchangeRateDto
                {
                    CurrencyCode = code!,
                    BuyingRate = buyingRate,
                    SellingRate = sellingRate,
                    Date = date
                });
            }
        }

        return rates;
    }

    public async Task<bool> CurrencyExistsAsync(string currencyCode)
    {
        var url = "https://www.tcmb.gov.tr/kurlar/today.xml";

        var response = await _httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync();

        var document = XDocument.Parse(xml);

        var normalizedCurrencyCode =
            currencyCode.Trim().ToUpperInvariant();

        return document
            .Descendants("Currency")
            .Any(x =>
                x.Attribute("Kod")?.Value == normalizedCurrencyCode);
    }
}