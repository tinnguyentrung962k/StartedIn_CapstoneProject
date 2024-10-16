using StartedIn.Repository.Repositories.Interface;
using StartedIn.Repository.Repositories;

namespace StartedIn.API.Configuration
{
    public static class RepositoryConfiguration
    {
        public static IServiceCollection AddRepositoryConfiguration(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            //services.AddScoped<IPostRepository, PostRepository>();
            //services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            //services.AddScoped<IUserRepository, UserRepository>();
            //services.AddScoped<IConnectionRepository, ConnectionRepository>();
            services.AddScoped<IPhaseRepository, PhaseRepository>();
            services.AddScoped<IMilestoneRepository, MilestoneRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ITaskboardRepository, TaskboardRepository>();
            return services;
        }
    }
}
