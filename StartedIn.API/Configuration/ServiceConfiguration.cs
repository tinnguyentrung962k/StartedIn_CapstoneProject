using StartedIn.Service.Services.Interface;
using StartedIn.Service.Services;
using StartedIn.CrossCutting.DTOs.RequestDTO;

namespace StartedIn.API.Configuration
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection AddServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IMilestoneService, MilestoneService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IAzureBlobService, AzureBlobService>();
            services.AddScoped<ISignNowService, SignNowService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IProjectCharterService, ProjectCharterService>();
            services.AddScoped<IDealOfferService, DealOfferService>();
            services.AddScoped<IDocumentFormatService, DocumentFormatService>();
            return services;
        }
    }
}
