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
            PhaseMappingProfile();
            MilestoneMappingProfile();
            TaskboardMappingProfile();
            TaskMappingProfile();
        }

        private void TaskboardMappingProfile()
        {
            CreateMap<Taskboard, TaskboardResponseDTO>()
                .ForMember(dest => dest.TasksList,
                    opt => opt.MapFrom(src => src.TasksList))
                .ReverseMap();
        }

        private void TaskMappingProfile()
        {
            CreateMap<TaskEntity, TaskInTaskboardResponseDTO>().ReverseMap();
            CreateMap<TaskEntity, TaskResponseDTO>().ReverseMap();
        }

        private void MilestoneMappingProfile()
        {
            CreateMap<Milestone, MilestoneResponseDTO>().ReverseMap();
            CreateMap<Milestone, MilestoneAndTaskboardResponseDTO>()
                .ForMember(rms => rms.Milestone, opt => opt.MapFrom(ms => ms))
                .ForMember(rms => rms.Taskboards, opt => opt.MapFrom(ms => ms.Taskboards));
        }

        private void PhaseMappingProfile()
        {
            //CreateMap<Phase, PhaseResponseDTO>();
            CreateMap<Phase, PhaseDetailResponseDTO>()
                .ForMember(dest => dest.Milestones,
                    opt => opt.MapFrom(src => src.Milestones))
                .ReverseMap();
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
    }
}
