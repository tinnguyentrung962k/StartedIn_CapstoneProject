﻿using StartedIn.Service.Services.Interface;
using StartedIn.Service.Services;

namespace StartedIn.API.Configuration
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection AddServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPhaseService, PhaseService>();
            services.AddScoped<IMilestoneService, MilestoneService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IProjectService, ProjectService>();
            return services;
        }
    }
}
