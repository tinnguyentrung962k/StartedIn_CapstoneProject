using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services.BackgroundWorkerServices;

public class AppointmentBackgroundWorker : BackgroundService
{
    private readonly ILogger<TaskBackgroundWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private const int ONE_HOUR = 3600000;

    public AppointmentBackgroundWorker(ILogger<TaskBackgroundWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AppointmentWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IAppointmentService appointmentService = scope.ServiceProvider.GetRequiredService<IAppointmentService>();
                await appointmentService.StartAppointment();
                await appointmentService.CompleteAppointment();
            }

            _logger.LogInformation("Appointment check completed. Waiting for next execution...");

            // Wait for the specified interval before executing again
            await Task.Delay(ONE_HOUR, stoppingToken);
        }

        _logger.LogInformation("AppointmentWorker stopping.");
    }
}
    
