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
            services.AddScoped<IShareEquityRepository,ShareEquityRepository>();
            services.AddScoped<IDisbursementRepository, DisbursementRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IFinanceRepository, FinanceRepository>();
            services.AddScoped<IInvestmentCallRepository, InvestmentCallRepository>();
            services.AddScoped<IAssetRepository, AssetRepository>();
            services.AddScoped<IPhaseRepository, PhaseRepository>();
            services.AddScoped<IApplicationRepository, ApplicationRepository>();
            services.AddScoped<ITaskCommentRepository, TaskCommentRepository>();
            services.AddScoped<ITaskAttachmentRepository, TaskAttachmentRepository>();
            services.AddScoped<IRecruitmentRepository, RecruitmentRepository>();
            services.AddScoped<IRecruitmentImageRepository, RecruitmentImageRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<ILeavingRequestRepository, LeavingRequestRepository>();
            services.AddScoped<ITerminationRequestRepository, TerminationRequestRepository>();
            services.AddScoped<IMeetingNoteRepository, MeetingNoteRepository>();
            services.AddScoped<ITransferLeaderRequestRepository, TransferLeaderRequestRepository>();
            return services;
        }
    }
}
