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
            services.AddScoped<IPayOsService, PayOsService>();
            services.AddScoped<IDisbursementService, DisbursementService>();
            services.AddScoped<IShareEquityService, ShareEquityService>();
            services.AddScoped<IInvestmentCallService, InvestmentCallService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IPhaseService, PhaseService>();
            services.AddScoped<IRecruitInviteService, RecruitInviteService>();
            services.AddScoped<ITaskCommentService, TaskCommentService>();
            services.AddScoped<ITaskAttachmentService, TaskAttachmentService>();
            services.AddScoped<ITaskHistoryService, TaskHistoryService>();
            services.AddScoped<IRecruitmentService, RecruitmentService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IRecruitmentImageService, RecruitmentImageService>();
            return services;
        }
    }
}
