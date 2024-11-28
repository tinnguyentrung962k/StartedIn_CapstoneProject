using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossCutting.Exceptions;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.DTOs.RequestDTO.ProjectCharter;
using System.ComponentModel.DataAnnotations;

namespace StartedIn.Service.Services
{
    public class ProjectCharterService : IProjectCharterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProjectCharterRepository _projectCharterRepository;
        private readonly IMilestoneRepository _milestoneRepository;
        private readonly ILogger<ProjectCharterService> _logger;
        private readonly IMilestoneService _milestoneService;
        private readonly IProjectRepository _projectRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly IPhaseRepository _phaseRepository;

        public ProjectCharterService(IUnitOfWork unitOfWork, IProjectCharterRepository projectCharterRepository,
            IMilestoneRepository milestoneRepository, ILogger<ProjectCharterService> logger, IMilestoneService milestoneService,
            IProjectRepository projectRepository, UserManager<User> userManager, IUserService userService,
            IPhaseRepository phaseRepository)
        {
            _unitOfWork = unitOfWork;
            _projectCharterRepository = projectCharterRepository;
            _milestoneRepository = milestoneRepository;
            _logger = logger;
            _milestoneService = milestoneService;
            _projectRepository = projectRepository;
            _userManager = userManager;
            _userService = userService;
            _phaseRepository = phaseRepository;
        }

        public async Task<ProjectCharter> CreateNewProjectCharter(string userId, string projectId, ProjectCharterCreateDTO projectCharter)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundUserError);
            }
            var project = await _projectRepository.GetProjectById(projectId);
            if (project is null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }

            var existing = await _projectCharterRepository.GetProjectCharterByProjectId(projectId);
            if (existing != null)
            {
                throw new ExistedRecordException(MessageConstant.CharterExistedError);
            }
            if (projectCharter.ListCreatePhaseDtos != null)
            {
                foreach (var createPhaseDto in projectCharter.ListCreatePhaseDtos)
                {
                    // Check if start date is after project's start date
                    if (createPhaseDto.StartDate < project.StartDate)
                    {
                        throw new ValidationException($"Giai đoạn '{createPhaseDto.PhaseName}' phải bắt đầu sau hoặc bằng ngày bắt đầu dự án {project.StartDate}");
                    }
                    if (createPhaseDto.StartDate > createPhaseDto.EndDate)
                    {
                        throw new ValidationException($"Giai đoạn '{createPhaseDto.PhaseName}' ngày bắt đầu không được lớn hơn ngày kết thúc");
                    }
                    var overlapsWithNewPhases = projectCharter.ListCreatePhaseDtos
                        .Where(p => p != createPhaseDto) // Exclude the current phase being validated
                        .Any(p => createPhaseDto.StartDate < p.EndDate && createPhaseDto.EndDate > p.StartDate);
                    if (overlapsWithNewPhases)
                    {
                        throw new ValidationException($"Giai đoạn '{createPhaseDto.PhaseName}' bị trùng lặp với một giai đoạn khác trong danh sách.");
                    }
                }
            }
            try
            {
                _unitOfWork.BeginTransaction();
                ProjectCharter newProjectCharter = new ProjectCharter
                {
                    ProjectId = projectId,
                    BusinessCase = projectCharter.BusinessCase,
                    Goal = projectCharter.Goal,
                    Objective = projectCharter.Objective,
                    Scope = projectCharter.Scope,
                    Constraints = projectCharter.Constraints,
                    Assumptions = projectCharter.Assumptions,
                    Deliverables = projectCharter.Deliverables,
                };
                var projectCharterEntity = _projectCharterRepository.Add(newProjectCharter);

                if (projectCharter.ListCreatePhaseDtos != null)
                {
                    foreach (var createPhaseDto in projectCharter.ListCreatePhaseDtos)
                    {
                        var newPhase = new Phase
                        {
                            PhaseName = createPhaseDto.PhaseName,
                            StartDate = createPhaseDto.StartDate,
                            EndDate = createPhaseDto.EndDate,
                            ProjectCharterId = newProjectCharter.Id
                        };
                        _phaseRepository.Add(newPhase);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return newProjectCharter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating project charter.");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<ProjectCharter> GetProjectCharterByProjectId(string projectId)
        {
            var projectCharter = await _projectCharterRepository.GetProjectCharterByProjectId(projectId);
            if (projectCharter == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundCharterError);
            }
            return projectCharter;
        }
        
        public async Task<ProjectCharter> UpdateProjectCharterInfo(string userId, string projectId, EditProjectCharterDTO editProjectCharterDto)
        {
            var loginUser = await _userService.CheckIfUserInProject(userId, projectId);
            
            var chosenProjectCharter = await _projectCharterRepository.QueryHelper()
                .Include(x=>x.Project)
                .Filter(x=>x.ProjectId.Equals(projectId))
                .GetOneAsync();
            
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            
            if (chosenProjectCharter == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundCharterError);
            }
            
            if (chosenProjectCharter.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.CharterNotBelongToProjectError);
            }
           
            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            try
            {
                chosenProjectCharter.BusinessCase = editProjectCharterDto.BusinessCase;
                chosenProjectCharter.Goal = editProjectCharterDto.Goal;
                chosenProjectCharter.Objective = editProjectCharterDto.Objective;
                chosenProjectCharter.Scope = editProjectCharterDto.Scope;
                chosenProjectCharter.Constraints = editProjectCharterDto.Constraints;
                chosenProjectCharter.Assumptions = editProjectCharterDto.Assumptions;
                chosenProjectCharter.Deliverables = editProjectCharterDto.Deliverables;
                chosenProjectCharter.LastUpdatedTime = DateTimeOffset.UtcNow;
                chosenProjectCharter.LastUpdatedBy = loginUser.User.FullName;
                _projectCharterRepository.Update(chosenProjectCharter);
                await _unitOfWork.SaveChangesAsync();
                return chosenProjectCharter;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Failed while update project charter");
            }
        }
    }
}
