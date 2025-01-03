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
    public class InvestmentCallBackgroundWorker : BackgroundService
    {
        private readonly ILogger<InvestmentCallBackgroundWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const int ONE_DAY = 86400000;

        public InvestmentCallBackgroundWorker(ILogger<InvestmentCallBackgroundWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("InvestmentCallWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    IInvestmentCallService investmentCallService = scope.ServiceProvider.GetRequiredService<IInvestmentCallService>();
                    await investmentCallService.CloseOverdueInvestmentCalls();
                }

                _logger.LogInformation("Investment Call check completed. Waiting for next execution...");

                // Wait for the specified interval before executing again
                await Task.Delay(ONE_DAY, stoppingToken);
            }

            _logger.LogInformation("InvestmentCallWorker stopping.");
        }
    }
}
