using StartedIn.API.Hubs;

namespace StartedIn.API.Configuration
{
    public static class SignalRHubConfiguration
    {
        public static IServiceCollection AddSignalRHub(this IServiceCollection services)
        {
            services.AddSignalR();
            services.AddSingleton<ProjectHub, ProjectHub>();
            return services;
        }
    }
}
