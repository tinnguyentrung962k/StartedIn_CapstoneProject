using AutoMapper;
using Microsoft.Extensions.Hosting;
using StartedIn.CrossCutting.DTOs.RequestDTO.Auth;
using StartedIn.CrossCutting.DTOs.RequestDTO.Project;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Contract;
using StartedIn.CrossCutting.DTOs.ResponseDTO.DealOffer;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Project;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectCharter;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;

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
            CreateMap<Milestone, MilestoneResponseDTO>().ReverseMap();
            CreateMap<Milestone, MilestoneAndTaskResponseDTO>()
                .ForMember(rms => rms.Milestone, opt => opt.MapFrom(ms => ms))
                .ForMember(rms => rms.Tasks, opt => opt.MapFrom(ms => ms.Tasks));
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
        }
        private void ProjectCharterMappingProfile()
        {
            CreateMap<ProjectCharterResponseDTO, ProjectCharter>().ReverseMap()
                .ForMember(dto => dto.Milestones, opt
                    => opt.MapFrom(projectCharter => projectCharter.Milestones));
        }
        private void DealOfferMappingProfile()
        {
            CreateMap<DealOffer, DealOfferForProjectResponseDTO>()
                .ForMember(dr => dr.InvestorId, opt => opt.MapFrom(de => de.Investor.Id))
                .ForMember(dr => dr.InvestorName, opt => opt.MapFrom(de => de.Investor.FullName))
                .ForMember(dr => dr.EquityShareOffer, opt => opt.MapFrom(de => de.EquityShareOffer.ToString()))
                .ForMember(dr => dr.Amount, opt => opt.MapFrom(de => de.Amount.ToString()))
                .ReverseMap();
        }
        private void DisbursementMappingProfile()
        {
            CreateMap<Disbursement, DisbursementInContractResponseDTO>().ReverseMap();
        }
    }
}
