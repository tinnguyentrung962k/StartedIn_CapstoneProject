using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.BackgroundWorkerServices
{
    public class DisbursementReminderBackgroundWorker : BackgroundService
    {
        private readonly ILogger<DisbursementReminderBackgroundWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const int ONE_HOUR = 3600000;

        public DisbursementReminderBackgroundWorker(ILogger<DisbursementReminderBackgroundWorker> logger,
        IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DisbursementReminder started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    IDisbursementService disbursementService = scope.ServiceProvider.GetRequiredService<IDisbursementService>();
                    await disbursementService.ReminderDisbursementForInvestor();
                }

                _logger.LogInformation("Disbursement check completed. Waiting for next execution...");

                // Wait for the specified interval before executing again
                await Task.Delay(ONE_HOUR, stoppingToken);
            }

            _logger.LogInformation("DisbursementReminderBackgroundWorker stopping.");
        }
    }
}
