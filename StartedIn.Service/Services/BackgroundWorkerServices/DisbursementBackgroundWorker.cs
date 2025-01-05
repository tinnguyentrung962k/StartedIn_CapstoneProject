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
    public class DisbursementBackgroundWorker : BackgroundService
    {
        private readonly ILogger<DisbursementBackgroundWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const int ONE_DAY = 86400000;

        public DisbursementBackgroundWorker(ILogger<DisbursementBackgroundWorker> logger,
        IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Disbursement worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    IDisbursementService disbursementService = scope.ServiceProvider.GetRequiredService<IDisbursementService>();
                    await disbursementService.ReminderDisbursementForInvestor();
                    await disbursementService.AutoUpdateOverdueIfDisbursementsExpire();

                }
                _logger.LogInformation("Disbursement check completed. Waiting for next execution...");

                // Wait for the specified interval before executing again
                await Task.Delay(ONE_DAY, stoppingToken);
            }

            _logger.LogInformation("DisbursementWorker stopping.");
        }
    }
}
