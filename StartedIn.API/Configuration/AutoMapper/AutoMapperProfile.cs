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
using StartedIn.CrossCutting.DTOs.ResponseDTO.ShareEquity;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Transaction;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;

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
            ContractMappingProfile();
            ProjectCharterMappingProfile();
            DealOfferMappingProfile();
            DisbursementMappingProfile();
            ShareEquityMappingProfile();
            InvestmentCallMappingProfile();
            TransactionMappingProfile();
            AssetProfileMapping();
            PhaseProfileMapping();
        }


        private void TaskMappingProfile()
        {
            CreateMap<TaskEntity, TaskResponseDTO>().ReverseMap();
            CreateMap<TaskEntity, TaskDetailDTO>()
                .ForMember(dest => dest.Assignees, opt => opt.MapFrom(src => src.UserTasks.Select(ut => ut.User)))
                .ForMember(dest => dest.Milestone, opt => opt.MapFrom(src => src.Milestone))
                .ForMember(dest => dest.ParentTask, opt => opt.MapFrom(src => src.ParentTask))
                .ForMember(dest => dest.SubTasks, opt => opt.MapFrom(src => src.SubTasks))
                .ReverseMap();
        }

        private void MilestoneMappingProfile()
        {
            CreateMap<Milestone, MilestoneResponseDTO>()
                .ForMember(dest => dest.PhaseName, opt => opt.MapFrom(src => src.Phase.PhaseName))
                .ReverseMap();
            CreateMap<Milestone, MilestoneDetailsResponseDTO>()
                .ForMember(rms => rms.AssignedTasks, opt => opt.MapFrom(ms => ms.Tasks));
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
            CreateMap<UserProject, MemberWithRoleInProjectResponseDTO>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.User.Id))
                .ForMember(x => x.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(x => x.RoleInTeam, opt => opt.MapFrom(src => src.RoleInTeam))
                .ForMember(x => x.Email, opt => opt.MapFrom(src => src.User.Email));
            CreateMap<User, MemberWithRoleInProjectResponseDTO>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ReverseMap();
            CreateMap<User, ProjectInviteUserDTO>().ReverseMap();
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
                .ReverseMap();
            CreateMap<Project, ExploreProjectDTO>()
                .ForMember(dest => dest.LeaderId,
                    opt => opt.MapFrom(src =>
                        src.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).UserId))
                .ForMember(dest => dest.LeaderFullName,
                    opt => opt.MapFrom(src =>
                        src.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).User.FullName))
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
                .ReverseMap();
            
            CreateMap<Project, ProjectDetailForAdminDTO>()
                .ForMember(p => p.ProjectCharterResponseDto, opt => opt.MapFrom(src => src.ProjectCharter))
                .ForMember(p => p.NewestInternalContractId, opt => opt.MapFrom(src =>
                    src.Contracts.FirstOrDefault(c => c.ProjectId.Equals(src.Id) 
                                                      && c.ContractStatus == ContractStatusEnum.COMPLETED
                                                      && c.ContractType == ContractTypeEnum.INTERNAL).Id))
                .ReverseMap();

            CreateMap<Project, ProjectInviteOverviewDTO>().ReverseMap();
        }
        private void ContractMappingProfile()
        {
            CreateMap<Contract, ContractResponseDTO>();
            CreateMap<Contract, GroupContractDetailResponseDTO>()
                .ForMember(dest => dest.UserShareEquityInContract, opt => opt.MapFrom(
                    src => src.ShareEquities));
            CreateMap<Contract, ContractSearchResponseDTO>()
                .ForMember(dest => dest.Parties, opt => opt.MapFrom(
                    src => src.UserContracts.Select(uc => new UserInContractResponseDTO
                    {
                        Id = uc.UserId,
                        FullName = uc.User.FullName,
                        Email = uc.User.Email,
                        PhoneNumber = uc.User.PhoneNumber
                    }).ToList()));
            CreateMap<Contract, InvestmentContractDetailResponseDTO>()
                .ForMember(dest => dest.Disbursements, opt => opt.MapFrom(
                    src => src.Disbursements.OrderBy(x => x.StartDate)))
                .ForMember(dest => dest.SharePercentage, opt => opt.MapFrom(
                    src => src.ShareEquities.FirstOrDefault(x => x.ContractId.Equals(src.Id)).Percentage))
                .ForMember(dest => dest.BuyPrice, opt => opt.MapFrom(
                    src => src.ShareEquities.FirstOrDefault(x => x.ContractId.Equals(src.Id)).SharePrice))
                .ForMember(dest => dest.InvestorId, opt => opt.MapFrom(
                    src => src.UserContracts
                    .Where(uc => uc.ContractId == src.Id &&
                     uc.Contract.ShareEquities.Any(se => se.ContractId == src.Id && se.StakeHolderType == RoleInTeam.Investor)).FirstOrDefault().UserId));
        }
        private void ShareEquityMappingProfile()
        {
            CreateMap<ShareEquity, UserShareEquityInContractResponseDTO>().ReverseMap();
            CreateMap<ShareEquitySummaryDTO, ShareEquitiesOfMemberInAProject>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.UserFullName))
                .ForMember(dest => dest.Percentage, opt => opt.MapFrom(src => src.TotalPercentage))
                .ForMember(dest => dest.DateAssigned, opt => opt.MapFrom(src => src.LatestShareDate))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.StakeHolderType, opt => opt.MapFrom(src => src.StakeHolderType));
        }
        private void ProjectCharterMappingProfile()
        {
            CreateMap<ProjectCharterResponseDTO, ProjectCharter>().ReverseMap();
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
                .ForMember(dr => dr.InvestorName, opt => opt.MapFrom(de => de.Investor.FullName));
            CreateMap<Disbursement, DisbursementForInvestorInInvestorMenuResponseDTO>()
                .ForMember(dr => dr.ContractIdNumber, opt => opt.MapFrom(de => de.Contract.ContractIdNumber))
                .ForMember(dr => dr.Amount, opt => opt.MapFrom(de => de.Amount.ToString()))
                .ForMember(dr => dr.ProjectName, opt => opt.MapFrom(de => de.Contract.Project.ProjectName))
                .ForMember(dr => dr.LogoUrl, opt => opt.MapFrom(de => de.Contract.Project.LogoUrl));

            CreateMap<Disbursement, DisbursementDetailForLeaderInProjectResponseDTO>()
                .ForMember(dr => dr.DisbursementAttachments, opt => opt.MapFrom(de => de.DisbursementAttachments))
                .ForMember(dr => dr.ContractIdNumber, opt => opt.MapFrom(de => de.Contract.ContractIdNumber))
                .ForMember(dr => dr.Amount, opt => opt.MapFrom(de => de.Amount.ToString()))
                .ForMember(dr => dr.InvestorName, opt => opt.MapFrom(de => de.Investor.FullName));
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
            
        }
        private void AssetProfileMapping()
        {
            CreateMap<Asset, AssetResponseDTO>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.ToString()));  
        }

        private void PhaseProfileMapping()
        {
            CreateMap<Phase, PhaseResponseDTO>().ReverseMap();
        }
        
    }
}
