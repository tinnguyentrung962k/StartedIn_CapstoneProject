using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services.BackgroundWorkerServices;

public class ContractCancelBackgroundWorker : BackgroundService
{
    private readonly ILogger<ContractCancelBackgroundWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private const int ONE_HOUR = 3600000;

    public ContractCancelBackgroundWorker(ILogger<ContractCancelBackgroundWorker> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ContractCancelBackgroundWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IContractService contractService = scope.ServiceProvider.GetRequiredService<IContractService>();
                await contractService.CancelContractAfterDueDate();
            }

            _logger.LogInformation("Contract cancellation check completed. Waiting for next execution...");

            // Wait for the specified interval before executing again
            await Task.Delay(ONE_HOUR, stoppingToken);
        }

        _logger.LogInformation("ContractCancelBackgroundWorker stopping.");
    }
}