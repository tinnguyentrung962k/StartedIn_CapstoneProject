using StartedIn.Repository.Repositories.Interface;
using StartedIn.Repository.Repositories;

namespace StartedIn.API.Configuration
{
    public static class RepositoryConfiguration
    {
        public static IServiceCollection AddRepositoryConfiguration(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPhaseRepository, PhaseRepository>();
            services.AddScoped<IMilestoneRepository, MilestoneRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ITaskboardRepository, TaskboardRepository>();
            services.AddScoped<IMilestoneHistoryRepository, MilestoneHistoryRepository>();
            return services;
        }
    }
}
