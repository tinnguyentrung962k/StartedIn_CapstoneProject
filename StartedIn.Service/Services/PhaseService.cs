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
    
    public async Task<Phase> CreateNewPhase(string userId, string projectId, string charterId, CreatePhaseDTO createPhaseDto)
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
            _unitOfWork.BeginTransaction();
            Phase phase = new Phase
            {
                PhaseName = createPhaseDto.PhaseName,
                StartDate = createPhaseDto.StartDate,
                EndDate = createPhaseDto.EndDate,
                ProjectCharterId = charterId
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

    public async Task<Phase> GetPhaseByPhaseId(string charterId, string phaseId)
    {
        var projectCharter =
            await _projectCharterRepository.QueryHelper().Filter(p => p.Id.Equals(charterId)).GetOneAsync();
        if (projectCharter == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectCharterError);
        }
        var phase = await _phaseRepository.QueryHelper().Filter(p => p.Id.Equals(phaseId)).GetOneAsync();
        if (phase == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundPhaseError);
        }

        return phase;
    }
}