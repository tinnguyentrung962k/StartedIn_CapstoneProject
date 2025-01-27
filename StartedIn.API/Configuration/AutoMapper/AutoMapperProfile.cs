﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using StartedIn.CrossCutting.DTOs.RequestDTO.Auth;
using StartedIn.CrossCutting.DTOs.RequestDTO.EquityShare;
using StartedIn.CrossCutting.DTOs.RequestDTO.Project;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Asset;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Contract;
using StartedIn.CrossCutting.DTOs.ResponseDTO.DealOffer;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.ResponseDTO.InvestmentCall;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Phase;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Project;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectCharter;
using StartedIn.CrossCutting.DTOs.ResponseDTO.RecruitInvite;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Recruitment;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ShareEquity;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TaskComment;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TaskAttachment;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Transaction;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Appointment;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Document;
using StartedIn.CrossCutting.DTOs.ResponseDTO.LeavingRequest;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectApproval;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationRequest;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TransferLeaderRequest;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TaskHistory;
using StartedIn.CrossCutting.DTOs.RequestDTO.User;

namespace StartedIn.API.Configuration.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            UserMappingProfile();
            ProjectMappingProfile();
            MilestoneMappingProfile();
            TaskMappingProfile();
            TaskCommentProfile();
            ContractMappingProfile();
            ProjectCharterMappingProfile();
            DealOfferMappingProfile();
            DisbursementMappingProfile();
            ShareEquityMappingProfile();
            InvestmentCallMappingProfile();
            TransactionMappingProfile();
            AssetProfileMapping();
            PhaseProfileMapping();
            TaskAttachmentMappingProfile();
            RecruitmentMappingProfile();
            RecruitmentImgMappingProfile();
            AppointmentMappingProfile();
            LeavingRequestMappingProfile();
            ApplicationMappingProfile();
            TerminationRequestMappingProfile();
            MeetingNoteMappingProfile();
            TransferRequestMappingProfile();
            UserTaskMappingProfile();
            ProjectApprovalMappingProfile();
            DocumentMappingProfile();
        }


        private void TaskMappingProfile()
        {
            CreateMap<TaskEntity, TaskResponseDTO>()
                .ForMember(dest => dest.Assignees, opt => opt.MapFrom(src => src.UserTasks))
                .ForMember(dest => dest.ExpectedManHour, opt => opt.MapFrom(src => src.ManHour))
                .ForMember(dest => dest.ActualManHour, opt => opt.MapFrom(src => src.UserTasks.Sum(x => x.ActualManHour)))
                .ReverseMap();
            CreateMap<TaskEntity, TaskDetailDTO>()
                .ForMember(dest => dest.Assignees, opt => opt.MapFrom(src => src.UserTasks.Select(ut => ut.User)))
                .ForMember(dest => dest.ExpectedManHour, opt => opt.MapFrom(src => src.ManHour))
                .ForMember(dest => dest.Milestone, opt => opt.MapFrom(src => src.Milestone))
                .ForMember(dest => dest.ParentTask, opt => opt.MapFrom(src => src.ParentTask))
                .ForMember(dest => dest.SubTasks, opt => opt.MapFrom(src => src.SubTasks))
                .ForMember(dest => dest.UserTasks, opt => opt.MapFrom(src => src.UserTasks))
                .ForMember(dest => dest.ActualManHour, opt => opt.MapFrom(src => src.UserTasks.Sum(x => x.ActualManHour)))
                .ReverseMap();
            CreateMap<TaskHistory, TaskHistoryResponseDTO>().ReverseMap();
        }

        private void MilestoneMappingProfile()
        {
            CreateMap<Milestone, MilestoneResponseDTO>()
                .ForMember(dest => dest.PhaseName, opt => opt.MapFrom(src => src.Phase.PhaseName))
                .ReverseMap();
            CreateMap<Milestone, MilestoneDetailsResponseDTO>()
                .ForMember(rms => rms.AssignedTasks, opt => opt.MapFrom(ms => ms.Tasks));
            CreateMap<Milestone, MilestoneInPhaseResponseDTO>()
                .ReverseMap();
            CreateMap<MilestoneHistory, MilestoneHistoryResponseDTO>()
                .ReverseMap();
        }

        private void UserMappingProfile()
        {
            CreateMap<UserInContractResponseDTO, User>().ReverseMap();
            CreateMap<User, RegisterDTO>().ReverseMap();
            CreateMap<User, HeaderProfileDTO>()
                .ForMember(userDto => userDto.UserRoles,
                    opt => opt.MapFrom(user
                        => user.UserRoles.Select(iur => iur.Role.Name).ToHashSet()))
                .ReverseMap()
                .ForPath(user => user.UserRoles, opt
                    => opt.MapFrom(userDto => userDto.UserRoles.Select(role => new UserRole { Role = new Role { Name = role } }).ToHashSet()));
            CreateMap<User, FullProfileDTO>()
                .ForMember(userDto => userDto.UserRoles,
                    opt => opt.MapFrom(user
                        => user.UserRoles.Select(iur => iur.Role.Name).ToHashSet()))
                .ReverseMap()
                .ForPath(user => user.UserRoles, opt
                    => opt.MapFrom(userDto => userDto.UserRoles.Select(role => new UserRole { Role = new Role { Name = role } }).ToHashSet()));
            CreateMap<User, UpdateProfileDTO>().ReverseMap();
            CreateMap<UserProject, MemberWithRoleInProjectResponseDTO>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.User.Id))
                .ForMember(x => x.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(x => x.RoleInTeam, opt => opt.MapFrom(src => src.RoleInTeam))
                .ForMember(x => x.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(x => x.ProfilePicture, opt => opt.MapFrom(src => src.User.ProfilePicture));
            CreateMap<User, MemberWithRoleInProjectResponseDTO>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ReverseMap();
            CreateMap<User, ProjectInviteUserDTO>().ReverseMap();
            CreateMap<User, AssigneeInTaskDTO>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                    .ReverseMap();
            CreateMap<UserTask, AssigneeInTaskDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.User.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.ActualManHour, opt => opt.MapFrom(src => src.ActualManHour));
        }
        private void ProjectMappingProfile()
        {
            CreateMap<ProjectCreateDTO, Project>().ReverseMap();
            CreateMap<Project, ProjectResponseDTO>().ForMember(dest => dest.LeaderId,
                    opt => opt.MapFrom(src =>
                        src.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).UserId))
                .ForMember(dest => dest.LeaderFullName,
                    opt => opt.MapFrom(src =>
                        src.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).User.FullName))
                .ForMember(dest => dest.LeaderProfilePicture,
                    opt => opt.MapFrom(src =>
                        src.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).User.ProfilePicture))
                .ForMember(dest => dest.CurrentMember, opt => opt.MapFrom(src => src.UserProjects.Count(x => x.RoleInTeam == RoleInTeam.Leader || x.RoleInTeam == RoleInTeam.Member)))
                .ReverseMap();
            CreateMap<Project, ExploreProjectDTO>()
                .ForMember(dest => dest.LeaderId,
                    opt => opt.MapFrom(src =>
                        src.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).UserId))
                .ForMember(dest => dest.LeaderFullName,
                    opt => opt.MapFrom(src =>
                        src.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).User.FullName))
                .ForMember(dest => dest.LeaderProfilePicture,
                    opt => opt.MapFrom(src =>
                        src.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).User.ProfilePicture))
                .ReverseMap();
            CreateMap<UserProject, UserRoleInATeamResponseDTO>()
                .ForMember(dest => dest.RoleInTeam, opt => opt.MapFrom(src => src.RoleInTeam));
            CreateMap<Project, ProjectDetailDTO>()
                .ForMember(p => p.InvestmentCallResponseDto, opt => opt.MapFrom(src => src.InvestmentCalls.FirstOrDefault(ic => ic.Id.Equals(src.ActiveCallId))))
                .ForMember(p => p.ProjectCharterResponseDto, opt => opt.MapFrom(src => src.ProjectCharter))
                .ForMember(dest => dest.LeaderId,
                    opt => opt.MapFrom(src =>
                        src.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).UserId))
                .ForMember(dest => dest.LeaderFullName,
                    opt => opt.MapFrom(src =>
                        src.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).User.FullName))
                .ForMember(dest => dest.CurrentMember, opt => opt.MapFrom(src => src.UserProjects.Count(x=>x.RoleInTeam == RoleInTeam.Leader || x.RoleInTeam == RoleInTeam.Member)))
                .ReverseMap();

            CreateMap<Project, ProjectDetailForAdminDTO>()
                .ForMember(dest => dest.PendingApproval, opt => opt.MapFrom(src => src.ProjectApprovals.FirstOrDefault(x => x.Status == ProjectApprovalStatus.PENDING)))
                .AfterMap((src, dest) =>
                {
                    if (dest.PendingApproval != null)
                    {
                        dest.PendingApproval.EquityShareCall = src.InvestmentCalls.FirstOrDefault(x => x.Status == InvestmentCallStatus.Pending).EquityShareCall.ToString();
                        dest.PendingApproval.TargetCall = src.InvestmentCalls.FirstOrDefault(x => x.Status == InvestmentCallStatus.Pending).TargetCall.ToString();
                    }
                })
                .ForMember(p => p.ProjectCharterResponseDto, opt => opt.MapFrom(src => src.ProjectCharter))
                .ForMember(dest => dest.LeaderId,
                    opt => opt.MapFrom(src =>
                        src.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).UserId))
                .ForMember(dest => dest.LeaderFullName,
                    opt => opt.MapFrom(src =>
                        src.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).User.FullName))
                .ForMember(dest => dest.LeaderProfilePicture,
                    opt => opt.MapFrom(src =>
                        src.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).User.ProfilePicture))
                .ForMember(dest => dest.IsSignedInternalContract, opt => opt.MapFrom(src => src.Contracts.Any(c => c.ContractType == ContractTypeEnum.INTERNAL && c.ContractStatus == ContractStatusEnum.COMPLETED)))
                .ForMember(dest => dest.CurrentMember, opt => opt.MapFrom(src => src.UserProjects.Count(x => x.RoleInTeam == RoleInTeam.Leader || x.RoleInTeam == RoleInTeam.Member)))
                .ReverseMap();

            CreateMap<Project, ProjectInviteOverviewDTO>().ReverseMap();
        }
        private void ContractMappingProfile()
        {
            CreateMap<Contract, ContractResponseDTO>();
            CreateMap<Contract, GroupContractDetailResponseDTO>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(
                    src => src.Project.ProjectName))
                .ForMember(dest => dest.UserShareEquityInContract, opt => opt.MapFrom(
                    src => src.ShareEquities.OrderByDescending(x=>x.Percentage)));
            CreateMap<Contract, ContractSearchResponseDTO>()
                .ForMember(dest => dest.Parties, opt => opt.MapFrom(
                    src => src.UserContracts.Select(uc => new UserInContractResponseDTO
                    {
                        Id = uc.UserId,
                        FullName = uc.User.FullName,
                        Email = uc.User.Email,
                        PhoneNumber = uc.User.PhoneNumber,
                        ProfilePicture = uc.User.ProfilePicture
                        
                    }).ToList()));
            CreateMap<Contract, InvestmentContractDetailResponseDTO>()
                .ForMember(dest => dest.Disbursements, opt => opt.MapFrom(
                    src => src.Disbursements.OrderBy(x => x.StartDate)))
                .ForMember(dest => dest.SharePercentage, opt => opt.MapFrom(
                    src => src.ShareEquities.FirstOrDefault(x => x.ContractId.Equals(src.Id)).Percentage))
                .ForMember(dest => dest.BuyPrice, opt => opt.MapFrom(
                    src => src.ShareEquities.FirstOrDefault(x => x.ContractId.Equals(src.Id)).SharePrice))
                .ForMember(dest => dest.InvestorId, opt => opt.MapFrom(src =>src.ShareEquities.FirstOrDefault(x=>x.ContractId.Equals(src.Id) && x.StakeHolderType == RoleInTeam.Investor).UserId))
                .ForMember(dest => dest.InvestorName, opt => opt.MapFrom(src => src.ShareEquities.FirstOrDefault(x => x.ContractId.Equals(src.Id) && x.StakeHolderType == RoleInTeam.Investor).User.FullName))
                .ForMember(dest => dest.InvestorPhoneNumber, opt => opt.MapFrom(src => src.ShareEquities.FirstOrDefault(x => x.ContractId.Equals(src.Id) && x.StakeHolderType == RoleInTeam.Investor).User.PhoneNumber))
                .ForMember(dest => dest.InvestorEmail, opt => opt.MapFrom(src => src.ShareEquities.FirstOrDefault(x => x.ContractId.Equals(src.Id) && x.StakeHolderType == RoleInTeam.Investor).User.Email))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(
                    src => src.Project.ProjectName));
            CreateMap<Contract, ContractInClosingProjectDTO>();
            CreateMap<UserContract, UserInContractHistoryResponseDTO>()
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.User.ProfilePicture))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));
            CreateMap<Contract, LiquidationNoteDetailResponseDTO>()
                .ForMember(dest => dest.ParentContractType, opt => opt.MapFrom(src => src.ParentContract.ContractType))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(
                    src => src.Project.ProjectName))
                .ForMember(dest => dest.Parties, opt => opt.MapFrom(
                    src => src.UserContracts.Select(uc => new UserInContractResponseDTO
                    {
                        Id = uc.UserId,
                        FullName = uc.User.FullName,
                        Email = uc.User.Email,
                        PhoneNumber = uc.User.PhoneNumber,
                        ProfilePicture = uc.User.ProfilePicture

                    }).ToList()));

        }
        private void ShareEquityMappingProfile()
        {
            CreateMap<ShareEquity, UserShareEquityInContractResponseDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName));
            CreateMap<ShareEquitySummaryDTO, ShareEquitiesOfMemberInAProject>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.UserFullName))
                .ForMember(dest => dest.Percentage, opt => opt.MapFrom(src => src.TotalPercentage))
                .ForMember(dest => dest.DateAssigned, opt => opt.MapFrom(src => src.LatestShareDate))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.StakeHolderType, opt => opt.MapFrom(src => src.StakeHolderType));
        }
        private void ProjectCharterMappingProfile()
        {
            CreateMap<ProjectCharter, ProjectCharterResponseDTO>()
                .ForMember(dest => dest.Phases, opt => opt.MapFrom(src => src.Phases))
                .ReverseMap();
        }
        private void DealOfferMappingProfile()
        {
            CreateMap<DealOffer, DealOfferForProjectResponseDTO>()
                .ForMember(dr => dr.InvestorId, opt => opt.MapFrom(de => de.Investor.Id))
                .ForMember(dr => dr.InvestorName, opt => opt.MapFrom(de => de.Investor.FullName))
                .ForMember(dr => dr.EquityShareOffer, opt => opt.MapFrom(de => de.EquityShareOffer.ToString()))
                .ForMember(dr => dr.Amount, opt => opt.MapFrom(de => de.Amount.ToString()))
                .ReverseMap();
            CreateMap<DealOffer, DealOfferForInvestorResponseDTO>()
                .ForMember(dr => dr.ProjectName, opt => opt.MapFrom(de => de.Project.ProjectName))
                .ForMember(dr => dr.ProjectId, opt => opt.MapFrom(de => de.ProjectId))
                .ForMember(dr => dr.LeaderId, opt => opt.MapFrom(de => de.Project.UserProjects.FirstOrDefault(x => x.RoleInTeam == RoleInTeam.Leader).UserId))
                .ForMember(dr => dr.LeaderName, opt => opt.MapFrom(de => de.Project.UserProjects.FirstOrDefault(x => x.RoleInTeam == RoleInTeam.Leader).User.FullName))
                .ForMember(dr => dr.Amount, opt => opt.MapFrom(de => de.Amount.ToString()))
                .ForMember(dr => dr.EquityShareOffer, opt => opt.MapFrom(de => de.EquityShareOffer.ToString()));
        }
        private void DisbursementMappingProfile()
        {
            CreateMap<Disbursement, DisbursementInContractResponseDTO>().ReverseMap();
            CreateMap<Disbursement, DisbursementForLeaderInProjectResponseDTO>()
                .ForMember(dr => dr.ContractIdNumber, opt => opt.MapFrom(de => de.Contract.ContractIdNumber))
                .ForMember(dr => dr.Amount, opt => opt.MapFrom(de => de.Amount.ToString()))
                .ForMember(dr => dr.InvestorName, opt => opt.MapFrom(de => de.Investor.FullName))
                .ForMember(dr => dr.InvestorProfilePicture, opt => opt.MapFrom(de => de.Investor.ProfilePicture));
            CreateMap<Disbursement, DisbursementForInvestorInInvestorMenuResponseDTO>()
                .ForMember(dr => dr.ContractIdNumber, opt => opt.MapFrom(de => de.Contract.ContractIdNumber))
                .ForMember(dr => dr.Amount, opt => opt.MapFrom(de => de.Amount.ToString()))
                .ForMember(dr => dr.ProjectName, opt => opt.MapFrom(de => de.Contract.Project.ProjectName))
                .ForMember(dr => dr.LogoUrl, opt => opt.MapFrom(de => de.Contract.Project.LogoUrl));

            CreateMap<Disbursement, DisbursementDetailForLeaderInProjectResponseDTO>()
                .ForMember(dr => dr.DisbursementAttachments, opt => opt.MapFrom(de => de.DisbursementAttachments))
                .ForMember(dr => dr.ContractIdNumber, opt => opt.MapFrom(de => de.Contract.ContractIdNumber))
                .ForMember(dr => dr.Amount, opt => opt.MapFrom(de => de.Amount.ToString()))
                .ForMember(dr => dr.InvestorName, opt => opt.MapFrom(de => de.Investor.FullName))
                .ForMember(dr => dr.InvestorProfilePicture, opt => opt.MapFrom(de => de.Investor.ProfilePicture));
            CreateMap<Disbursement, DisbursementDetailForInvestorResponseDTO>()
                .ForMember(dr => dr.DisbursementAttachments, opt => opt.MapFrom(de => de.DisbursementAttachments))
                .ForMember(dr => dr.ContractIdNumber, opt => opt.MapFrom(de => de.Contract.ContractIdNumber))
                .ForMember(dr => dr.Amount, opt => opt.MapFrom(de => de.Amount.ToString()))
                .ForMember(dr => dr.ProjectName, opt => opt.MapFrom(de => de.Contract.Project.ProjectName))
                .ForMember(dr => dr.ProjectId, opt => opt.MapFrom(de => de.Contract.Project.Id))
                .ForMember(dr => dr.LogoUrl, opt => opt.MapFrom(de => de.Contract.Project.LogoUrl));
            CreateMap<DisbursementAttachment, DisbursementAttachmentResponseDTO>().ReverseMap();
            CreateMap<Disbursement, DisbursementDetailInATransactionResponseDTO>()
                .ForMember(dr => dr.ContractIdNumber, opt => opt.MapFrom(de => de.Contract.ContractIdNumber))
                .ForMember(dr => dr.DisbursementAttachments, opt => opt.MapFrom(de => de.DisbursementAttachments))
                .ForMember(dr => dr.Amount, opt => opt.MapFrom(de => de.Amount.ToString()))
                .ForMember(dr => dr.TransactionId, opt => opt.MapFrom(de => de.Transaction.Id));
            CreateMap<Disbursement, DisbursementInClosingProjectDTO>()
                .ForMember(dr => dr.Amount, opt => opt.MapFrom(de => de.Amount.ToString()))
                .ForMember(dr => dr.ContractIdNumber, opt => opt.MapFrom(de => de.Contract.ContractIdNumber));
            CreateMap<Disbursement, DisbursementInDealOfferDTO>().ReverseMap();
        }

        private void InvestmentCallMappingProfile()
        {
            CreateMap<InvestmentCall, InvestmentCallResponseDTO>()
                .ForMember(dest => dest.AmountRaised, opt => opt.MapFrom(src => src.AmountRaised.ToString()))
                .ForMember(dest => dest.EquityShareCall, opt => opt.MapFrom(src => src.EquityShareCall.ToString()))
                .ForMember(dest => dest.RemainAvailableEquityShare, opt => opt.MapFrom(src => src.RemainAvailableEquityShare.ToString()))
                .ForMember(dest => dest.TargetCall, opt => opt.MapFrom(src => src.TargetCall.ToString()));
        }
        private void TransactionMappingProfile()
        {
            CreateMap<Transaction, TransactionResponseDTO>()
                    .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.ToString()))
                    .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.Finance.ProjectId))
                    .ForMember(dest => dest.FromUserName, opt => opt.MapFrom(src => src.FromName))
                    .ForMember(dest => dest.ToUserName, opt => opt.MapFrom(src => src.ToName));
            CreateMap<Transaction, TransactionDetailInAssetDTO>()
                    .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.ToString()))
                    .ForMember(dest => dest.FromUserName, opt => opt.MapFrom(src => src.FromName))
                    .ForMember(dest => dest.ToUserName, opt => opt.MapFrom(src => src.ToName));
        }
        private void AssetProfileMapping()
        {
            CreateMap<Asset, AssetResponseDTO>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.ToString()));
            CreateMap<Asset, AssetInClosingProjectDTO>()
                .ReverseMap();
        }

        private void PhaseProfileMapping()
        {
            CreateMap<Phase, PhaseResponseDTO>()
                .ForMember(dest => dest.Milestones, opt => opt.MapFrom(src => src.Milestones.OrderBy(x=>x.StartDate)))
                .ReverseMap();
        }

        private void TaskCommentProfile()
        {
            CreateMap<TaskComment, TaskCommentDTO>().ReverseMap();
        }

        private void TaskAttachmentMappingProfile()
        {
            CreateMap<TaskAttachment, TaskAttachmentResponseDTO>()
                .ReverseMap();
        }

        private void RecruitmentMappingProfile()
        {
            CreateMap<Recruitment, RecruitmentResponseDTO>()
                .ForMember(dest => dest.RecruitmentImgs, opt => opt.MapFrom(src => src.RecruitmentImgs.Where(ri => ri.RecruitmentId.Equals(src.Id))))
                .ForMember(dest => dest.LeaderId, opt => opt.MapFrom(src => src.Project.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).UserId))
                .ForMember(dest => dest.LeaderName, opt => opt.MapFrom(src => src.Project.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).User.FullName))
                .ForMember(dest => dest.LeaderAvatarUrl, opt => opt.MapFrom(src => src.Project.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).User.ProfilePicture))
                .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.Project.LogoUrl))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ReverseMap();
            
            CreateMap<Recruitment, RecruitmentListDTO>()
                .ForMember(dest => dest.LeaderId,
                    opt => opt.MapFrom(src =>
                        src.Project.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).UserId))
                .ForMember(dest => dest.LeaderName,
                    opt => opt.MapFrom(src =>
                        src.Project.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).User.FullName))
                .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.Project.LogoUrl))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ForMember(dest => dest.LeaderAvatarUrl, opt => opt.MapFrom(src => src.Project.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).User.ProfilePicture))
                .ReverseMap();

            CreateMap<Recruitment, RecruitmentInProjectDTO>().ReverseMap();
        }

        private void RecruitmentImgMappingProfile()
        {
            CreateMap<RecruitmentImg, RecruitmentImgResponseDTO>()
                .ReverseMap();
        }

        private void ApplicationMappingProfile()
        {
            CreateMap<Application, ApplicationDTO>()
                .ForMember(dest => dest.CVFiles, opt => opt.MapFrom(src => src.ApplicationFiles))
                .ReverseMap();
            CreateMap<ApplicationFile, ApplicationApplyFileDTO>();
            CreateMap<Application, UserInvitationInProjectDTO>()
                .ForMember(dest => dest.CandidatePhoneNumber, opt => opt.MapFrom(src => src.Candidate.PhoneNumber))
                .ForMember(dest => dest.CandidateName, opt => opt.MapFrom(src => src.Candidate.FullName))
                .ForMember(dest => dest.CandidateEmail, opt => opt.MapFrom(src => src.Candidate.Email))
                .ForMember(dest => dest.CandidateProfilePicture, opt => opt.MapFrom(src => src.Candidate.ProfilePicture));
        }

        private void AppointmentMappingProfile()
        {
            CreateMap<Appointment, AppointmentInCalendarResponseDTO>().ReverseMap();
            CreateMap<Appointment, AppointmentResponseDTO>()
                .ForMember(dest => dest.MilestoneName, opt => opt.MapFrom(src => src.Milestone.Title))
                .ForMember(dest => dest.MeetingNotes, opt => opt.MapFrom(src => src.MeetingNotes))
                .ForMember(dest => dest.ContractName, opt => opt.MapFrom(src => src.Contract.ContractName))
                .ForMember(dest => dest.ContractType, opt => opt.MapFrom(src => src.Contract.ContractType))
                .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.Documents))
                .ReverseMap();
        }
        private void LeavingRequestMappingProfile()
        {
            CreateMap<LeavingRequest, LeavingRequestResponseDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.User.ProfilePicture)); 
        }
        private void TerminationRequestMappingProfile()
        {
            CreateMap<TerminationRequest, TerminationRequestSentResponseDTO>()
                .ForMember(dest => dest.ContractIdNumber, opt => opt.MapFrom(src => src.Contract.ContractIdNumber))
                .ForMember(dest => dest.ToName, opt => opt.MapFrom(src => src.Contract.UserContracts.FirstOrDefault(uc => uc.UserId == src.ToId).User.FullName));
            CreateMap<TerminationRequest, TerminationRequestReceivedResponseDTO>()
                .ForMember(dest => dest.ContractIdNumber, opt => opt.MapFrom(src => src.Contract.ContractIdNumber))
                .ForMember(dest => dest.FromName, opt => opt.MapFrom(src => src.Contract.UserContracts.FirstOrDefault(uc => uc.UserId == src.FromId).User.FullName));
        }

        private void MeetingNoteMappingProfile()
        {
            CreateMap<MeetingNote, MeetingNoteResponseDTO>().ReverseMap();
        }

        private void TransferRequestMappingProfile() 
        {
            CreateMap<TransferLeaderRequest, TransferLeaderRequestDetailDTO>()
                .ForMember(dest => dest.MeetingNotes, opt => opt.MapFrom(src => src.Appointment.MeetingNotes))
                .ForMember(dest => dest.MeetingStatus, opt => opt.MapFrom(src => src.Appointment.Status))
                .ForMember(dest => dest.AppointmentTime, opt => opt.MapFrom(src => src.Appointment.AppointmentTime))
                .ForMember(dest => dest.AppointmentTitle, opt => opt.MapFrom(src => src.Appointment.Title))
                .ForMember(dest => dest.FormerLeaderName, opt => opt.MapFrom(src => src.FormerLeader.FullName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Appointment.Description));
            CreateMap<TransferLeaderRequest, TransferLeaderHistoryResponseDTO>()
                .ForMember(dest => dest.NewLeaderName, opt => opt.MapFrom(src => src.NewLeader.FullName))
                .ForMember(dest => dest.NewLeaderEmail, opt => opt.MapFrom(src => src.NewLeader.Email))
                .ForMember(dest => dest.NewLeaderProfilePicture, opt => opt.MapFrom(src => src.NewLeader.ProfilePicture))
                .ForMember(dest => dest.FormerLeaderName, opt => opt.MapFrom(src => src.FormerLeader.FullName))
                .ForMember(dest => dest.FormerLeaderEmail, opt => opt.MapFrom(src => src.FormerLeader.Email))
                .ForMember(dest => dest.FormerLeaderProfilePicture, opt => opt.MapFrom(src => src.FormerLeader.ProfilePicture))
                .ForMember(dest => dest.AppointmentName, opt => opt.MapFrom(src => src.Appointment.Title))
                .ForMember(dest => dest.AppointmentTime, opt => opt.MapFrom(src => src.Appointment.AppointmentTime))
                .ForMember(dest => dest.MeetingNotes, opt => opt.MapFrom(src => src.Appointment.MeetingNotes));
        }

        private void UserTaskMappingProfile()
        {
            CreateMap<UserTask, UserTaskResponseDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.Task.Title))
                .ReverseMap();
        }

        private void ProjectApprovalMappingProfile()
        {
            CreateMap<ProjectApproval, ProjectApprovalResponseDTO>()
                .ForMember(dest => dest.LeaderName,
                    opt => opt.MapFrom(src =>src.CreatedBy))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ForMember(dest => dest.SentDate, opt => opt.MapFrom(src => src.CreatedTime))
                .ForMember(dest => dest.ApprovalDate, opt => opt.MapFrom(src => src.LastUpdatedTime))
                .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.Documents))
                .ReverseMap();
            CreateMap<ProjectApproval, ApprovalRequestsResponseDTO>()
                .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.Documents))
                .ReverseMap();
        }

        private void DocumentMappingProfile()
        {
            CreateMap<Document, DocumentResponseDTO>().ReverseMap();
        }
    }
}
