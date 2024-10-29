using StartedIn.Repository.Repositories.Interface;
using StartedIn.Repository.Repositories;
using StartedIn.Service.Services.Interface;
using StartedIn.Service.Services;

namespace StartedIn.API.Configuration
{
    public static class RepositoryConfiguration
    {
        public static IServiceCollection AddRepositoryConfiguration(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IMilestoneRepository, MilestoneRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<IMilestoneHistoryRepository, MilestoneHistoryRepository>();
            services.AddScoped<ITaskHistoryRepository, TaskHistoryRepository>();
            services.AddScoped<IContractRepository, ContractRepository>();
            services.AddScoped<IProjectCharterRepository, ProjectCharterRepository>();
            services.AddScoped<IDealOfferRepository, DealOfferRepository>();
            services.AddScoped<IDealOfferHistoryRepository, DealOfferHistoryRepository>();
            return services;
        }
    }
}
