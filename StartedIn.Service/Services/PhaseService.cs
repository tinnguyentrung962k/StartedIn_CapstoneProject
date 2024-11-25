using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Phase;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services;

public class PhaseService : IPhaseService
{
    private readonly IUserService _userService;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPhaseRepository _phaseRepository;
    private readonly ILogger<Phase> _logger;
    private readonly IProjectCharterRepository _projectCharterRepository;
    public PhaseService(IUserService userService, IProjectRepository projectRepository, IUnitOfWork unitOfWork, IPhaseRepository phaseRepository,
        ILogger<Phase> logger, IProjectCharterRepository projectCharterRepository)
    {
        _userService = userService;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _phaseRepository = phaseRepository;
        _logger = logger;
        _projectCharterRepository = projectCharterRepository;
    }
    
    public async Task<Phase> CreateNewPhase(string userId, string projectId, CreatePhaseDTO createPhaseDto)
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
        if (projectRole != RoleInTeam.Leader)
        {
            throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
        }

        if (createPhaseDto.EndDate < createPhaseDto.StartDate)
        {
            throw new InvalidInputException(MessageConstant.StartDateLaterThanEndDate);
        }

        try
        {
            var project = await _projectRepository.GetProjectById(projectId);
            _unitOfWork.BeginTransaction();
            Phase phase = new Phase
            {
                PhaseName = createPhaseDto.PhaseName,
                StartDate = createPhaseDto.StartDate,
                EndDate = createPhaseDto.EndDate,
                ProjectCharterId = project.ProjectCharter.Id
            };
            _phaseRepository.Add(phase);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return phase;
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while creating the phase: {ex.Message}");
            await _unitOfWork.RollbackAsync();
            throw;
        }
        
    }

    public async Task<Phase> GetPhaseByPhaseId(string projectId, string phaseId)
    {
        var project = await _projectRepository.GetProjectById(projectId);
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }
        var phase = await _phaseRepository.QueryHelper()
            .Filter(p => p.Id.Equals(phaseId) && p.ProjectCharter.ProjectId.Equals(projectId))
            .GetOneAsync();
        if (phase == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundPhaseError);
        }
        return phase;
    }

    public async Task<List<Phase>> GetPhasesByProjectId(string projectId)
    {
        var projectCharter = await _projectCharterRepository.QueryHelper().Filter(pc => pc.ProjectId.Equals(projectId)).GetOneAsync();
        if (projectCharter == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundCharterError);
        }
        var phases = await _phaseRepository.QueryHelper().Filter(p => p.ProjectCharterId.Equals(projectCharter.Id)).Include(p => p.Milestones)
            .GetAllAsync();
        return phases.ToList();
    }

    public async Task<Phase> UpdatePhase(string userId, string projectId, string id, UpdatePhaseDTO updatePhaseDto)
    {
        var loginUser = await _userService.CheckIfUserInProject(userId, projectId);

        var chosenPhase = await _phaseRepository.QueryHelper()
            .Include(x => x.Milestones)
            .Filter(x => x.Id.Equals(id))
            .GetOneAsync();

        var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);

        if (chosenPhase == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundMilestoneError);
        }

        if (projectRole != RoleInTeam.Leader)
        {
            throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
        }

        try
        {
            _unitOfWork.BeginTransaction();
            chosenPhase.PhaseName = updatePhaseDto.PhaseName;
            chosenPhase.StartDate = updatePhaseDto.StartDate;
            chosenPhase.EndDate = updatePhaseDto.EndDate;
            chosenPhase.ProjectCharterId = updatePhaseDto.ProjectCharterId;
            _phaseRepository.Update(chosenPhase);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return chosenPhase;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new Exception("Failed while update milestone");
        }
    }
}