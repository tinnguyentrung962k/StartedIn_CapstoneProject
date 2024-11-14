using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services.BackgroundWorkerServices;

public class ContractBackgroundWorker : BackgroundService
{
    private readonly ILogger<ContractBackgroundWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private const int ONE_HOUR = 3600000;

    public ContractBackgroundWorker(ILogger<ContractBackgroundWorker> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ContractWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IContractService contractService = scope.ServiceProvider.GetRequiredService<IContractService>();
                await contractService.CancelContractAfterDueDate();
            }

            _logger.LogInformation("Contract check completed. Waiting for next execution...");

            // Wait for the specified interval before executing again
            await Task.Delay(ONE_HOUR, stoppingToken);
        }

        _logger.LogInformation("ContractWorker stopping.");
    }
}