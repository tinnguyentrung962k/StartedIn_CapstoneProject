﻿using AutoMapper;
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
            PostMappingProfile();
            TeamMappingProfile();
            ProjectMappingProfile();
            ConnectionMappingProfile();
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
            //CreateMap<MinorTask, AssignableMinorTaskResponseDTO>();
        }

        private void MilestoneMappingProfile()
        {
            CreateMap<Milestone, MilestoneResponseDTO>().ReverseMap();
            CreateMap<Milestone, MilestoneAndTaskboardResponseDTO>()
                .ForMember(rms => rms.Milestone, opt => opt.MapFrom(ms => ms))
                .ForMember(rms => rms.Taskboards, opt => opt.MapFrom(ms => ms.Taskboards));
            //CreateMap<MajorTask, AssignableMajorTaskResponseDTO>();
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
            //CreateMap<User, UpdateProfileDTO>().ReverseMap();
            //CreateMap<User, ProjectLeaderResponseDTO>().ReverseMap();
            //CreateMap<User, TeamLeaderResponseDTO>().ReverseMap();
        }
        private void PostMappingProfile()
        {
            //CreateMap<Post, PostResponseDTO>().
            //    ForMember(postResponse => postResponse.ImgUrls,
            //    opt => opt.MapFrom(post => post.PostImages.Select(pi => pi.ImageLink).ToHashSet()))
            //    .ForMember(postResponse => postResponse.UserFullName,
            //    opt => opt.MapFrom(post => post.User.FullName))
            //    .ForMember(postResponse => postResponse.UserImgUrl,
            //    opt => opt.MapFrom(post => post.User.ProfilePicture))
            //    .ReverseMap();
            //CreateMap<PostRequestDTO, Post>().ReverseMap();
        }
        private void TeamMappingProfile()
        {
            //CreateMap<Team, TeamCreateRequestDTO>().ReverseMap();
            //CreateMap<Team, TeamResponseDTO>()
            //   .ForMember(tr => tr.Users,
            //       opt => opt.MapFrom(t
            //           => t.TeamUsers.Select(tu => tu.User.FullName).ToHashSet()))
            //   .ForMember(tr => tr.Projects, opt => opt.MapFrom(t => t.Projects))
            //   .ReverseMap()
            //   .ForPath(t => t.TeamUsers, opt
            //       => opt.MapFrom(tr => tr.Users.Select(name => new TeamUser { User = new User { FullName = name } }).ToHashSet()));

            //CreateMap<Team, TeamInvitationResponseDTO>()
            //    .ForMember(dest => dest.Leader, opt => opt.MapFrom(src => src.TeamLeader))
            //    .ReverseMap();

            //CreateMap<Team, TeamWithMembersResponseDTO>()
            //.ForMember(dest => dest.TeamMembers, opt => opt.MapFrom(src =>
            //    src.TeamUsers.Select(tu => new TeamMemberResponseDTO
            //    {
            //        Id = tu.User.Id,
            //        FullName = tu.User.FullName,
            //        Email = tu.User.Email,
            //        ProfilePicture = tu.User.ProfilePicture,
            //        RoleInTeam = tu.RoleInTeam.ToString()
            //    }).ToList()));


        }
        private void ProjectMappingProfile()
        {
            //CreateMap<Project, ProjectCreateDTO>().ReverseMap();
            //CreateMap<Project, ResponseProjectForListInTeamDTO>().ReverseMap();
            //CreateMap<Project, ProjectResponseDTO>()
            //    .ForMember(dest => dest.Leader,
            //        opt => opt.MapFrom(src => src.Team.TeamLeader))
            //    .ReverseMap();
            //CreateMap<ProjectResponseDTO, ProjectCreateDTO>().ReverseMap();
        }

        private void ConnectionMappingProfile()
        {
            //CreateMap<Connection, PendingConnectionDTO>()
            //    .ForMember(c => c.SenderName,
            //        opt => opt.MapFrom(con => con.Sender.FullName))
            //    .ForMember(c => c.ProfilePicture,
            //        opt => opt.MapFrom(con => con.Sender.ProfilePicture))
            //    .ForMember(c => c.Time,
            //        opt => opt.MapFrom(con => con.CreatedTime))
            //    .ReverseMap();
            //CreateMap<Connection, PendingSendingRequestDTO>()
            //    .ForMember(c => c.ReceiverName,
            //        opt => opt.MapFrom(con => con.Receiver.FullName))
            //    .ForMember(c => c.ProfilePicture,
            //        opt => opt.MapFrom(con => con.Receiver.ProfilePicture))
            //    .ForMember(c => c.Time,
            //        opt => opt.MapFrom(con => con.CreatedTime))
            //    .ReverseMap();

            //CreateMap<Connection, ConnectionDTO>()
            //.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            //.ForMember(dest => dest.UserId, opt => opt.Ignore())
            //.ForMember(dest => dest.ConnectedUserName, opt => opt.Ignore())
            //.ForMember(dest => dest.ProfilePicture, opt => opt.Ignore());

            //CreateMap<User, ConnectionDTO>()
            //.ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            //.ForMember(dest => dest.ConnectedUserName, opt => opt.MapFrom(src => src.FullName))
            //.ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.ProfilePicture))
            //.ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

        }
    }
}
