using AutoMapper;
using Microsoft.Extensions.Hosting;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.Domain.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
        }


        private void TaskMappingProfile()
        {
            CreateMap<TaskEntity, TaskResponseDTO>().ReverseMap();
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
        }
        private void ProjectMappingProfile()
        {
            CreateMap<ProjectCreateDTO, Project>().ReverseMap();
            CreateMap<Project, ProjectResponseDTO>().ReverseMap();
            CreateMap<Project, ProjectWithMembersResponseDTO>()
                .ForMember(dest => dest.Project, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.MemberWithRoleInProject, opt => opt.MapFrom(src =>
                    src.UserProjects.Select(tu => new MemberWithRoleInProjectResponseDTO
                    {
                        Id = tu.User.Id,
                        FullName = tu.User.FullName,
                        RoleInTeam = tu.RoleInTeam.ToString()
                    }).ToList()));
            
        }
        private void ContractMappingProfile() 
        {
            CreateMap<Contract, ContractResponseDTO>()
                .ForMember(dest => dest.UsersInContract, opt => opt.MapFrom(
                    src => src.UserContracts.Select(uc => new UserInContractResponseDTO
                    {
                        Id = uc.UserId,
                        PartyFullName = uc.User.FullName,
                        PartyEmail = uc.User.Email,
                        PartyPhoneNumber = uc.User.PhoneNumber
                    }).ToList()));
        }
        private void ProjectCharterMappingProfile()
        {
            CreateMap<ProjectCharterResponseDTO, ProjectCharter>().ReverseMap()
                .ForMember(dto => dto.Milestones, opt
                    => opt.MapFrom(projectCharter => projectCharter.Milestones));
        }
        private void DealOfferMappingProfile()
        {
            CreateMap<DealOffer, DealOfferResponseDTO>()
                .ForMember(dr => dr.InvestorName, opt => opt.MapFrom(de => de.Investor.FullName))
                .ForMember(dr => dr.ProjectName, opt => opt.MapFrom(de => de.Project.ProjectName))
                .ForMember(dr => dr.EquityShareOffer, opt => opt.MapFrom(de => de.EquityShareOffer.ToString()))
                .ForMember(dr => dr.Amount, opt => opt.MapFrom(de => de.Amount.ToString()))
                .ReverseMap();
        }
    }
}
