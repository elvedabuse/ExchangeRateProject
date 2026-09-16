using ExchangeRate.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExchangeRate.Infrastructure.Jobs
{
    public class DailyRateJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailyRateJob> _logger;

        public DailyRateJob(
            IServiceScopeFactory scopeFactory,
            ILogger<DailyRateJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var target = DateTime.Today
                    .AddHours(15)
                    .AddMinutes(30);

                if (now >= target)
                {
                    target = target.AddDays(1);
                }

                while (target.DayOfWeek == DayOfWeek.Saturday ||
                       target.DayOfWeek == DayOfWeek.Sunday)
                {
                    target = target.AddDays(1);
                }

                var delay = target - now;

                await Task.Delay(delay, stoppingToken);

                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var updateDailyRates =
                        scope.ServiceProvider
                            .GetRequiredService<UpdateDailyRates>();

                    await updateDailyRates.ExecuteAsync(
                        forceRefresh: true);

                    _logger.LogInformation(
                        "Günlük döviz kurları başarıyla güncellendi.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Günlük döviz kurları güncellenirken hata oluştu.");
                }
            }
        }
    }
}