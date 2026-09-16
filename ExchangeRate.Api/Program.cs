using ExchangeRate.Application.Interfaces;
using ExchangeRate.Application.Services;
using ExchangeRate.Infrastructure.Data;
using ExchangeRate.Infrastructure.Jobs;
using ExchangeRate.Infrastructure.Repositories;
using ExchangeRate.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ExchangeRateDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();

builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();

builder.Services.AddScoped<CurrencyService>();

builder.Services.AddScoped<HistoricalRateService>();

builder.Services.AddHttpClient<ITcmbService, TcmbService>();

builder.Services.AddScoped<UpdateDailyRates>();

builder.Services.AddScoped<ExchangeService>();

builder.Services.AddScoped<ExchangeRateService>();

builder.Services.AddHostedService<DailyRateJob>();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();