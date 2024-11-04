using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public ProjectCharterService(IUnitOfWork unitOfWork, IProjectCharterRepository projectCharterRepository,
            IMilestoneRepository milestoneRepository, ILogger<ProjectCharterService> logger, IMilestoneService milestoneService,
            IProjectRepository projectRepository, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _projectCharterRepository = projectCharterRepository;
            _milestoneRepository = milestoneRepository;
            _logger = logger;
            _milestoneService = milestoneService;
            _projectRepository = projectRepository;
            _userManager = userManager;
        }

        public async Task<ProjectCharter> CreateNewProjectCharter(string userId, ProjectCharterCreateDTO projectCharter)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new NotFoundException("Người dùng không tồn tại");
                }
                var project = await _projectRepository.GetProjectById(projectCharter.ProjectId);
                if (project is null)
                {
                    throw new NotFoundException("Không tìm thấy dự án");
                }
                var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectCharter.ProjectId);
                if (projectRole != CrossCutting.Enum.RoleInTeam.Leader)
                {
                    throw new UnauthorizedProjectRoleException("Bạn không phải nhóm trưởng");
                }
                _unitOfWork.BeginTransaction();
                ProjectCharter newProjectCharter = new ProjectCharter
                {
                    ProjectId = projectCharter.ProjectId,
                    BusinessCase = projectCharter.BusinessCase,
                    Goal = projectCharter.Goal,
                    Objective = projectCharter.Objective,
                    Scope = projectCharter.Scope,
                    Constraints = projectCharter.Constraints,
                    Assumptions = projectCharter.Assumptions,
                    Deliverables = projectCharter.Deliverables,
                };
                var projectCharterEntity = _projectCharterRepository.Add(newProjectCharter);

                if (projectCharter.ListMilestoneCreateDto != null)
                {
                    foreach (var milestoneDto in projectCharter.ListMilestoneCreateDto)
                    {
                        var newMilestone = new Milestone
                        {
                            ProjectId = projectCharter.ProjectId,
                            CharterId = newProjectCharter.Id,
                            PhaseName = milestoneDto.PhaseEnum,
                            Title = milestoneDto.MilstoneTitle,
                            Description = milestoneDto.Description,
                            DueDate = milestoneDto.DueDate,
                            ProjectCharter = projectCharterEntity
                        };
                        _milestoneRepository.Add(newMilestone);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return newProjectCharter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating project.");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<ProjectCharter> GetProjectCharterByCharterId(string id)
        {
            var projectCharter = await _projectCharterRepository.QueryHelper()
                .Include(x => x.Milestones)
                .Filter(x => x.Id.Equals(id))
                .GetOneAsync();
            if (projectCharter == null)
            {
                throw new NotFoundException("Không tìm thấy cột mốc");
            }
            return projectCharter;
        }

        public async Task<ProjectCharter> GetProjectCharterByProjectId(string projectId)
        {
            var projectCharter = await _projectCharterRepository.QueryHelper()
                .Include(x => x.Milestones)
                .Filter(x => x.ProjectId.Equals(projectId))
                .GetOneAsync(); ;
            if (projectCharter == null)
            {
                throw new NotFoundException("Không tìm thấy cột mốc");
            }
            return projectCharter;
        }
    }
}
