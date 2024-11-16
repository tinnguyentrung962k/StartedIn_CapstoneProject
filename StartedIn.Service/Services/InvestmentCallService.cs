using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.InvestmentCall;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services;

public class InvestmentCallService : IInvestmentCallService
{
    private readonly IUserService _userService;
    private readonly IProjectRepository _projectRepository;
    private readonly IInvestmentCallRepository _investmentCallRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InvestmentCall> _logger;
    
    public InvestmentCallService(IUserService userService, IProjectRepository projectRepository, IInvestmentCallRepository investmentCallRepository,
        ILogger<InvestmentCall> logger)
    {
        _userService = userService;
        _projectRepository = projectRepository;
        _investmentCallRepository = investmentCallRepository;
        _logger = logger;
    }

    public async Task<InvestmentCall> CreateNewInvestmentCall(string userId, string projectId,
        InvestmentCallCreateDTO investmentCallCreateDto)
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        
        var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);

        if (projectRole != RoleInTeam.Leader)
        {
            throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
        }

        var project = await _projectRepository.GetProjectById(projectId);

        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }
        
        try
        {
            _unitOfWork.BeginTransaction();
            InvestmentCall call = new InvestmentCall
            {
                ProjectId = projectId,
                TargetCall = investmentCallCreateDto.TargetCall,
                AmountRaised = 0,
                TotalInvestor = 0,
                StartDate = investmentCallCreateDto.StartDate,
                EndDate = investmentCallCreateDto.EndDate,
                Status = InvestmentCallStatus.Open
            };
            _investmentCallRepository.Add(call);
            
            if (!string.IsNullOrEmpty(project.ActiveCallId))
            {
                var activeCall = await _investmentCallRepository.QueryHelper()
                    .Filter(c => c.Id.Equals(project.ActiveCallId)).GetOneAsync();
                activeCall.Status = InvestmentCallStatus.Closed;
                _investmentCallRepository.Update(activeCall);
            }
            
            project.ActiveCallId = call.Id;
            _projectRepository.Update(project);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return call;
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while creating the investment call: {ex.Message}");
            await _unitOfWork.RollbackAsync();
            throw;
        }
        
    }

    public async Task<InvestmentCall> GetInvestmentCallById(string projectId, string callId)
    {
        var project = await _projectRepository.GetProjectById(projectId);
        
        if (project is null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }
        
        var investmentCall = await _investmentCallRepository.QueryHelper()
            .Filter(ic => ic.Id.Equals(callId)).GetOneAsync();
        
        if (investmentCall == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundInvestmentCall);
        }

        return investmentCall;
    }
}