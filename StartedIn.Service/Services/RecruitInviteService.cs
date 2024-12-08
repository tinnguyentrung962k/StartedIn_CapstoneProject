﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
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
        private readonly IAzureBlobService _azureBlobService;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public RecruitInviteService(
            IProjectRepository projectRepository,
            IEmailService emailService,
            IAzureBlobService azureBlobService,
            IUserRepository userRepository,
            UserManager<User> userManager,
            IUserService userService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IApplicationRepository applicationRepository)
        {
            _projectRepository = projectRepository;
            _emailService = emailService;
            _azureBlobService = azureBlobService;
            _userRepository = userRepository;
            _userManager = userManager;
            _userService = userService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _applicationRepository = applicationRepository;
        }

        public async Task SendJoinProjectInvitation(string userId, List<string> inviteUserEmails, string projectId)
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

            var currentMemberInProject = project.UserProjects
                .Where(x => (x.RoleInTeam == RoleInTeam.Leader || x.RoleInTeam == RoleInTeam.Member) && x.Status == UserStatusInProject.Active)
                .Count();

            if (currentMemberInProject == project.MaxMember)
            {
                throw new InviteException(MessageConstant.FullMembersOfTeam);
            }

            var currentMentorCount = project.UserProjects.Count(x => x.RoleInTeam == RoleInTeam.Mentor && x.Status == UserStatusInProject.Active);

            _unitOfWork.BeginTransaction();

            foreach (var inviteUserEmail in inviteUserEmails)
            {
                var existedUser = await _userManager.FindByEmailAsync(inviteUserEmail);
                if (existedUser == null)
                {
                    throw new NotFoundException(MessageConstant.NotFoundUserError + $"\n{inviteUserEmail}");
                }

                var userWithRole = await _userManager.GetAUserWithSystemRole(existedUser.Id);

                if (userWithRole.UserRoles.Any(ur => ur.Role.Name == RoleConstants.INVESTOR))
                {
                    throw new InviteException($"{MessageConstant.UserHasInvestorSystemRole}{existedUser.FullName}, {existedUser.Email}");
                }

                var existedUserInProject = project.UserProjects.FirstOrDefault(up => up.User.Equals(existedUser) && up.Status == UserStatusInProject.Active);
                if (existedUserInProject != null)
                {
                    throw new InviteException(MessageConstant.UserExistedInProject + $"\n{existedUser.FullName}, {existedUser.Email}");
                }

                var userInOtherProjects = await _projectRepository.GetAProjectByUserId(existedUser.Id);
                if (userInOtherProjects != null)
                {
                    throw new InviteException(MessageConstant.UserInOtherProjectError + $"\n{existedUser.Email}");
                }

                RoleInTeam assignedRole;
                if (userWithRole.UserRoles.Any(ur => ur.Role.Name == RoleConstants.MENTOR))
                {
                    // Kiểm tra nếu vượt quá số lượng mentor
                    if (currentMentorCount >= 2)
                    {
                        throw new InviteException(MessageConstant.GreaterThan2MentorError);
                    }
                    assignedRole = RoleInTeam.Mentor;
                    currentMentorCount++; // Tăng số lượng mentor sau khi gán vai trò
                }
                else if (userWithRole.UserRoles.Any(ur => ur.Role.Name == RoleConstants.USER))
                {
                    assignedRole = RoleInTeam.Member;
                }
                else
                {
                    throw new InviteException(MessageConstant.InvalidRoleForInvitation + $"\n{existedUser.Email}");
                }

                var existedInvite = await _applicationRepository.QueryHelper()
                    .Filter(x => x.CandidateId.Equals(existedUser.Id)
                    && x.ProjectId.Equals(projectId)
                    && x.Type == ApplicationTypeEnum.INVITE
                    && x.Status == ApplicationStatus.PENDING)
                    .GetOneAsync();

                if (existedInvite != null)
                {
                    throw new InviteException(MessageConstant.YouHaveSentInvitationForUser + $"{existedUser.Email}");
                }
                else
                {
                    // Create a new invitation
                    _applicationRepository.Add(new Application
                    {
                        CandidateId = existedUser.Id,
                        Status = ApplicationStatus.PENDING,
                        Type = ApplicationTypeEnum.INVITE,
                        Role = assignedRole,
                        ProjectId = projectId
                    });
                }
            }

            // Send emails only after all validations pass
            foreach (var inviteUserEmail in inviteUserEmails)
            {
                var invitedUser = await _userManager.FindByEmailAsync(inviteUserEmail);
                RoleInTeam assignedRole;
                var userWithRole = await _userManager.GetAUserWithSystemRole(invitedUser.Id);
                if (userWithRole.UserRoles.Any(ur => ur.Role.Name == RoleConstants.MENTOR))
                {
                    assignedRole = RoleInTeam.Mentor;
                }
                else if (userWithRole.UserRoles.Any(ur => ur.Role.Name == RoleConstants.USER))
                {
                    assignedRole = RoleInTeam.Member;
                }
                else
                {
                    throw new InviteException(MessageConstant.InvalidRoleForInvitation + $"\n{invitedUser.Email}");
                }

                await _emailService.SendInvitationToProjectAsync(
                    invitedUser.Email,
                    projectId,
                    userInProject.User.FullName,
                    project.ProjectName,
                    assignedRole
                );
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
                .Filter(x => x.CandidateId.Equals(userId)
                && x.Type == acceptInviteDTO.Type
                && x.Status == ApplicationStatus.PENDING
                && x.Role == acceptInviteDTO.Role)
                .GetOneAsync();

            if (invite == null) { throw new NotFoundException(MessageConstant.NotInvitedError); }

            try
            {
                _unitOfWork.BeginTransaction();
                invite.Status = ApplicationStatus.ACCEPTED;
                _applicationRepository.Update(invite);
                var project = await _projectRepository.GetProjectById(projectId);
                var userInProject = project.UserProjects.FirstOrDefault(up => up.UserId.Equals(userId) 
                && up.Status != UserStatusInProject.Active);
                if (userInProject != null) 
                {
                    userInProject.Status = UserStatusInProject.Active;
                    await _userRepository.UpdateUserInProject(userInProject);
                }
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

        public async Task<Application> ApplyRecruitment(string userId, string projectId, string recruitmentId, IFormFile file)
        {
            var existedApplication = await _applicationRepository.QueryHelper()
                .Filter(x => x.CandidateId.Equals(userId)
                               && x.ProjectId.Equals(projectId)
                                              && x.RecruitmentId.Equals(recruitmentId)
                                                             && x.Type == ApplicationTypeEnum.APPLY
                                                                            && x.Status == ApplicationStatus.PENDING)
                .GetOneAsync();
            if (existedApplication != null)
            {
                throw new InviteException(MessageConstant.YouHaveAppliedForRecruitment);
            }

            var userInProject = await _userRepository.GetAUserInProject(projectId, userId);
            if (userInProject != null)
            {
                throw new InviteException(MessageConstant.ApplicantAlreadyInProject);
            }

            var userInOtherProjects = await _projectRepository.GetAProjectByUserId(userId);
            if (userInOtherProjects != null)
            {
                throw new InviteException(MessageConstant.UserInOtherProjectError);
            }

            try
            {
                // Upload CV to get the URL string
                var cvUrl = await _azureBlobService.UploadCVFileApplication(file);
                var application = new Application
                {
                    CandidateId = userId,
                    ProjectId = projectId,
                    RecruitmentId = recruitmentId,
                    Status = ApplicationStatus.PENDING,
                    Type = ApplicationTypeEnum.APPLY,
                    Role = RoleInTeam.Member,
                    CVUrl = cvUrl
                };

                _applicationRepository.Add(application);
                await _unitOfWork.SaveChangesAsync();
                return application;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<Application>> GetApplicationsOfProject(string userId, string projectId)
        {
            // Get list of application from project, by type apply and status pending
            // TODO: make it pagination
            var applications = await _applicationRepository.GetApplicationsWithCandidate(projectId);
            // Remove applications of the user
            foreach (var application in applications.ToList())
            {
                application.Candidate.Applications = null;
            }
            return applications;
        }

        public async Task AcceptApplication(string userId, string projectId, string applicationId)
        {
            // check if user is leader of project
            var userInProject = await _userRepository.GetAUserInProject(projectId, userId);
            if (userInProject.RoleInTeam != RoleInTeam.Leader)
            {
                throw new InviteException(MessageConstant.RolePermissionError);
            }

            var application = await _applicationRepository.QueryHelper()
                .Filter(x => x.Id.Equals(applicationId)
                    && x.ProjectId.Equals(projectId)
                    && x.Type == ApplicationTypeEnum.APPLY
                    && x.Status == ApplicationStatus.PENDING)
                .GetOneAsync();

            var userInOtherProjects = await _projectRepository.GetAProjectByUserId(application.CandidateId);
            if (userInOtherProjects != null)
            {
                throw new InviteException(MessageConstant.UserInOtherProjectError);
            }

            if (application == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundRecruitmentApplication);
            }

            try
            {
                _unitOfWork.BeginTransaction();
                application.Status = ApplicationStatus.ACCEPTED;
                _applicationRepository.Update(application);

                await _userRepository.AddUserToProject(application.CandidateId, projectId, application.Role);

                await _unitOfWork.CommitAsync();
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task RejectApplication(string userId, string projectId, string applicationId)
        {
            // check if user is leader of project
            var userInProject = await _userRepository.GetAUserInProject(projectId, userId);
            if (userInProject.RoleInTeam != RoleInTeam.Leader)
            {
                throw new InviteException(MessageConstant.RolePermissionError);
            }

            var application = await _applicationRepository.QueryHelper()
                .Filter(x => x.Id.Equals(applicationId)
                    && x.ProjectId.Equals(projectId)
                    && x.Type == ApplicationTypeEnum.APPLY
                    && x.Status == ApplicationStatus.PENDING)
                .GetOneAsync();

            if (application == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundRecruitmentApplication);
            }

            try
            {
                _unitOfWork.BeginTransaction();
                application.Status = ApplicationStatus.REJECTED;
                _applicationRepository.Update(application);

                await _unitOfWork.CommitAsync();
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}
