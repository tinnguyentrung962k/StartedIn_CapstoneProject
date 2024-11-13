using StartedIn.Service.Services.BackgroundWorkerServices;

namespace StartedIn.API.Configuration;

public static class BackgroundWorkerConfiguration
{
    public static IServiceCollection AddBackgroundWorkerConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<ContractCancelBackgroundWorker>();
        services.AddHostedService<DisbursementReminderBackgroundWorker>();
        return services;
    }
}