using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.Domain.Context
{
    public class AppDbContext : IdentityDbContext<User, Role, string,
        IdentityUserClaim<string>,
        UserRole,
        IdentityUserLogin<string>,
        IdentityRoleClaim<string>,
        IdentityUserToken<string>>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<ProjectCharter> ProjectCharters { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Milestone> Milestones { get; set; }
        public DbSet<MilestoneHistory> MilestoneHistories { get; set; }
        public DbSet<TaskEntity> TaskEntities { get; set; }
        public DbSet<TaskAttachment> TaskAttachments { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<TaskHistory> TaskHistories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<UserContract> UserContracts { get; set; }
        public DbSet<ShareEquity> ShareEquities { get; set; }
        public DbSet<DealOffer> DealOffers { get; set; }
        public DbSet<DealOfferHistory> DealOfferHistories { get; set; }
        public DbSet<Disbursement> Disbursements { get; set; }
        public DbSet<DisbursementAttachment> DisbursementAttachments { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentComment> DocumentComments { get; set; }
        public DbSet<MeetingNote> MeetingNotes { get; set; }
        public DbSet<Finance> Finances { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<InvestmentCall> InvestmentCalls { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Recruitment> Recruitments { get; set; }
        public DbSet<RecruitmentImg> RecruitmentImgs { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Phase> Phases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");
            });

            modelBuilder.Entity<UserProject>()
                .HasKey(up => new { up.UserId, up.ProjectId });

            modelBuilder.Entity<UserProject>()
                .HasOne(up => up.Project)
                .WithMany(p => p.UserProjects)
                .HasForeignKey(up => up.ProjectId);

            modelBuilder.Entity<UserProject>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserProjects)
                .HasForeignKey(up => up.UserId);

            modelBuilder.Entity<UserProject>()
                .Property(u => u.RoleInTeam)
                .HasConversion(
                    v => v.ToString(),
                    v => (RoleInTeam)Enum.Parse(typeof(RoleInTeam), v));

            modelBuilder.Entity<UserContract>()
               .HasKey(up => new { up.UserId, up.ContractId });

            modelBuilder.Entity<UserContract>()
                .HasOne(up => up.Contract)
                .WithMany(p => p.UserContracts)
                .HasForeignKey(up => up.ContractId);

            modelBuilder.Entity<UserTask>()
                .HasKey(up => new { up.UserId, up.TaskId });

            modelBuilder.Entity<UserTask>()
                .HasOne(up => up.Task)
                .WithMany(p => p.UserTasks)
                .HasForeignKey(up => up.TaskId);

            modelBuilder.Entity<Contract>()
                .ToTable("Contract");

            modelBuilder.Entity<Contract>()
            .Property(u => u.ContractStatus)
            .HasConversion(
            v => v.ToString(),
                v => (ContractStatusEnum)Enum.Parse(typeof(ContractStatusEnum), v));

            modelBuilder.Entity<Contract>()
            .Property(u => u.ContractType)
            .HasConversion(
            v => v.ToString(),
                v => (ContractTypeEnum)Enum.Parse(typeof(ContractTypeEnum), v));

            modelBuilder.Entity<UserContract>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserContracts)
                .HasForeignKey(up => up.UserId);
            modelBuilder.Entity<Appointment>()
                .ToTable("Appointment");
            modelBuilder.Entity<Appointment>()
            .Property(u => u.Status)
            .HasConversion(
            v => v.ToString(),
                v => (MeetingStatus)Enum.Parse(typeof(MeetingStatus), v));
            modelBuilder.Entity<Document>()
                .ToTable("Document");
            modelBuilder.Entity<DocumentComment>()
                .ToTable("DocumentComment");
            modelBuilder.Entity<MeetingNote>()
                .ToTable("MeetingNote");
            modelBuilder.Entity<DealOffer>()
                .ToTable("DealOffer");
            modelBuilder.Entity<InvestmentCall>()
                .ToTable("InvestmentCall");

            modelBuilder.Entity<InvestmentCall>()
            .Property(u => u.Status)
            .HasConversion(
            v => v.ToString(),
                v => (InvestmentCallStatus)Enum.Parse(typeof(InvestmentCallStatus), v));

            modelBuilder.Entity<DealOffer>()
            .Property(u => u.DealStatus)
            .HasConversion(
            v => v.ToString(),
                v => (DealStatusEnum)Enum.Parse(typeof(DealStatusEnum), v));

            modelBuilder.Entity<Disbursement>()
                .ToTable("Disbursement");

            modelBuilder.Entity<Disbursement>()
            .Property(u => u.DisbursementStatus)
            .HasConversion(
            v => v.ToString(),
                v => (DisbursementStatusEnum)Enum.Parse(typeof(DisbursementStatusEnum), v));

            modelBuilder.Entity<DisbursementAttachment>()
                .ToTable("DisbursementAttachment");

            modelBuilder.Entity<DealOfferHistory>()
                .ToTable("DealOfferHistory");

            modelBuilder.Entity<ShareEquity>()
                .ToTable("ShareEquity");

            modelBuilder.Entity<ShareEquity>()
            .Property(u => u.StakeHolderType)
            .HasConversion(
            v => v.ToString(),
                v => (RoleInTeam)Enum.Parse(typeof(RoleInTeam), v));

            modelBuilder.Entity<ProjectCharter>()
                .ToTable("ProjectCharter");

            modelBuilder.Entity<ShareEquity>()
                .Property(u => u.Percentage)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<UserProject>()
                .ToTable("UserProject");

            modelBuilder.Entity<Milestone>()
                .ToTable("Milestone");
            
            modelBuilder.Entity<MilestoneHistory>()
                .ToTable("MilestoneHistory");

            modelBuilder.Entity<Project>()
                .ToTable("Project");

            modelBuilder.Entity<Project>()
            .Property(u => u.ProjectStatus)
            .HasConversion(
            v => v.ToString(),
                v => (ProjectStatusEnum)Enum.Parse(typeof(ProjectStatusEnum), v));

            modelBuilder.Entity<TaskAttachment>()
                .ToTable("TaskAttachment");

            modelBuilder.Entity<TaskHistory>()
                .ToTable("TaskHistory");

            modelBuilder.Entity<TaskEntity>()
                .ToTable("Task");

            modelBuilder.Entity<TaskEntity>()
            .Property(u => u.Status)
            .HasConversion(
            v => v.ToString(),
                v => (TaskEntityStatus)Enum.Parse(typeof(TaskEntityStatus), v));

            modelBuilder.Entity<TaskComment>()
                .ToTable("TaskComment");

            modelBuilder.Entity<Project>()
            .Property(p => p.RemainingPercentOfShares)
            .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<Finance>()
                .ToTable("Finance");
            modelBuilder.Entity<Transaction>()
                .ToTable("Transaction");
            modelBuilder.Entity<Transaction>()
            .Property(u => u.Type)
            .HasConversion(
            v => v.ToString(),
                v => (TransactionType)Enum.Parse(typeof(TransactionType), v));

            modelBuilder.Entity<Application>()
                .ToTable("Application");
            modelBuilder.Entity<Application>()
            .Property(u => u.Status)
            .HasConversion(
            v => v.ToString(),
                v => (ApplicationStatus)Enum.Parse(typeof(ApplicationStatus), v));
            modelBuilder.Entity<Recruitment>()
                .ToTable("Recruitment");
            modelBuilder.Entity<RecruitmentImg>()
                .ToTable("RecruitmentImg");
            modelBuilder.Entity<Asset>()
                .ToTable("Asset");
            modelBuilder.Entity<Asset>()
            .Property(u => u.Status)
            .HasConversion(
            v => v.ToString(),
                v => (AssetStatus)Enum.Parse(typeof(AssetStatus), v));
            
            modelBuilder.Entity<Phase>()
                .ToTable("Phase");

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName!.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }
        }
    }
}
