using AutoMapper;
using Microsoft.AspNetCore.Identity;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Project;
using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
using StartedIn.CrossCutting.DTOs.ResponseDTO.RecruitInvite;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Extensions;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services
{
    public class RecruitInviteService : IRecruitInviteService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IEmailService _emailService;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public RecruitInviteService(
            IProjectRepository projectRepository,
            IEmailService emailService,
            IUserRepository userRepository,
            UserManager<User> userManager,
            IUserService userService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IApplicationRepository applicationRepository)
        {
            _projectRepository = projectRepository;
            _emailService = emailService;
            _userRepository = userRepository;
            _userManager = userManager;
            _userService = userService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _applicationRepository = applicationRepository;
        }

        public async Task SendJoinProjectInvitation(string userId, List<ProjectInviteEmailAndRoleDTO> inviteUsers, string projectId)
        {
            var project = await _projectRepository.GetProjectAndMemberByProjectId(projectId);
            if (project == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new InviteException(MessageConstant.RolePermissionError);
            }
            var currentMemberInProject = project.UserProjects.Where(x => x.RoleInTeam == RoleInTeam.Leader || x.RoleInTeam == RoleInTeam.Member).Count();
            if (currentMemberInProject == project.MaxMember)
            {
                throw new InviteException(MessageConstant.FullMembersOfTeam);
            }

            _unitOfWork.BeginTransaction();
            foreach (var inviteUser in inviteUsers)
            {
                var existedUser = await _userManager.FindByEmailAsync(inviteUser.Email);
                if (existedUser == null)
                {
                    throw new NotFoundException(MessageConstant.NotFoundUserError + $"\n{inviteUser.Email}");
                }
                else
                {
                    // Get the user with their roles in the system
                    var userWithRole = await _userManager.GetAUserWithSystemRole(existedUser.Id);

                    // Check if the user has the 'INVESTOR' role
                    if (userWithRole != null && userWithRole.UserRoles.Any(ur => ur.Role.Name == RoleConstants.INVESTOR))
                    {
                        throw new InviteException($"{MessageConstant.UserHasInvestorSystemRole}{existedUser.FullName},{existedUser.Email}");
                    }

                    // Check if the user is already part of the project
                    var existedUserInProject = project.UserProjects.FirstOrDefault(up => up.User.Equals(existedUser));
                    if (existedUserInProject != null)
                    {
                        throw new InviteException(MessageConstant.UserExistedInProject + $"\n{existedUser.FullName}, {existedUser.Email}");
                    }
                }

                var existedInvite = await _applicationRepository.QueryHelper()
                    .Filter(x => x.CandidateId.Equals(existedUser.Id) && x.RecruitmentId.Equals(projectId) && x.Type == ApplicationTypeEnum.INVITE)
                    .GetOneAsync();

                if (existedInvite != null)
                {
                    // If invite existed, update the role
                    existedInvite.Role = inviteUser.RoleInTeam;
                    existedInvite.Status = ApplicationStatus.PENDING;
                    _applicationRepository.Update(existedInvite);
                }
                else
                {
                    // Create application for the invited user
                    _applicationRepository.Add(new Application
                    {
                        CandidateId = existedUser.Id,
                        Status = ApplicationStatus.PENDING,
                        Type = ApplicationTypeEnum.INVITE,
                        Role = inviteUser.RoleInTeam
                    });
                }
            }

            // Only after finish checking validity of all invited users email, then send email to them
            foreach (var inviteUser in inviteUsers)
            {
                // Send invitation email
                await _emailService.SendInvitationToProjectAsync(inviteUser.Email, projectId, userInProject.User.FullName, project.ProjectName, inviteUser.RoleInTeam);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }

        public async Task AddUserToProject(string projectId, string userId, RoleInTeam roleInTeam)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundUserError);
            }
            var project = await _projectRepository.QueryHelper().Include(x => x.UserProjects).Filter(x => x.Id.Equals(projectId)).GetOneAsync();
            if (project == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }
            var existingLeader = project.UserProjects.FirstOrDefault(x => x.RoleInTeam == RoleInTeam.Leader);
            if (roleInTeam == RoleInTeam.Leader && existingLeader != null)
            {
                throw new InviteException(MessageConstant.JoinGroupWithLeaderRoleError);
            }
            var userInTeam = await _userRepository.GetAUserInProject(projectId, user.Id);
            if (userInTeam != null)
            {
                return;
            }
            await _userRepository.AddUserToProject(userId, projectId, roleInTeam);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ProjectInviteOverviewDTO> GetProjectInviteOverview(string projectId)
        {
            var projectWithOnlyLeader = await _projectRepository.GetProjectWithOnlyLeader(projectId);
            if (projectWithOnlyLeader == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }

            var projectInviteOverview = new ProjectInviteOverviewDTO
            {
                Id = projectId,
                ProjectName = projectWithOnlyLeader.ProjectName,
                Description = projectWithOnlyLeader.Description,
                LogoUrl = projectWithOnlyLeader.LogoUrl,
                ProjectStatus = projectWithOnlyLeader.ProjectStatus,
                StartDate = projectWithOnlyLeader.StartDate,
                EndDate = projectWithOnlyLeader.EndDate,
                Leader = _mapper.Map<ProjectInviteUserDTO>(projectWithOnlyLeader.UserProjects.FirstOrDefault(up => up.RoleInTeam == RoleInTeam.Leader).User),
            };

            return projectInviteOverview;
        }

        public async Task AcceptProjectInvitation(string userId, string projectId, AcceptInviteDTO acceptInviteDTO)
        {
            var invite = await _applicationRepository.QueryHelper()
                .Filter(x => x.CandidateId.Equals(userId) && x.Type == acceptInviteDTO.Type && x.Status == ApplicationStatus.PENDING && x.Role == acceptInviteDTO.Role)
                .GetOneAsync();

            if (invite == null) { throw new NotFoundException(MessageConstant.NotInvitedError); }

            try
            {
                _unitOfWork.BeginTransaction();
                invite.Status = ApplicationStatus.ACCEPTED;
                _applicationRepository.Update(invite);

                await _userRepository.AddUserToProject(userId, projectId, invite.Role);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}
