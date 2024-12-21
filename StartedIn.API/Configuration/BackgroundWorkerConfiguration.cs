using StartedIn.Service.Services.BackgroundWorkerServices;

namespace StartedIn.API.Configuration;

public static class BackgroundWorkerConfiguration
{
    public static IServiceCollection AddBackgroundWorkerConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<ContractBackgroundWorker>();
        services.AddHostedService<DisbursementBackgroundWorker>();
        services.AddHostedService<TaskBackgroundWorker>();
        services.AddHostedService<AppointmentBackgroundWorker>();
        return services;
    }
}