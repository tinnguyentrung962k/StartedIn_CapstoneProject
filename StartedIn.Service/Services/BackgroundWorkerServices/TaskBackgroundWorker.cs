using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services.BackgroundWorkerServices;

public class TaskBackgroundWorker : BackgroundService
{
    private readonly ILogger<TaskBackgroundWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private const int ONE_HOUR = 3600000;

    public TaskBackgroundWorker(ILogger<TaskBackgroundWorker> logger, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TaskWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ITaskService taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                await taskService.MarkTaskAsLate();
                await taskService.StartTask();
                await taskService.MarkTaskAsStartLate();
            }

            _logger.LogInformation("Task check completed. Waiting for next execution...");

            // Wait for the specified interval before executing again
            await Task.Delay(ONE_HOUR, stoppingToken);
        }

        _logger.LogInformation("TaskWorker stopping.");
    }
}