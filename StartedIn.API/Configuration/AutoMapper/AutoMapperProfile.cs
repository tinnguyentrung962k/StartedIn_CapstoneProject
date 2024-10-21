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
    }
}
