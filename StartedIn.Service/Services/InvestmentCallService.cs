using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.EquityShare;
using StartedIn.CrossCutting.DTOs.RequestDTO.InvestmentCall;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.DealOffer;
using StartedIn.CrossCutting.DTOs.ResponseDTO.InvestmentCall;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories;
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
    private readonly IShareEquityService _shareEquityService;
    
    public InvestmentCallService(IUserService userService, IProjectRepository projectRepository, IInvestmentCallRepository investmentCallRepository,
        ILogger<InvestmentCall> logger, IUnitOfWork unitOfWork, IShareEquityService shareEquityService)
    {
        _userService = userService;
        _projectRepository = projectRepository;
        _investmentCallRepository = investmentCallRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _shareEquityService = shareEquityService;
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
        if (project.ProjectStatus != ProjectStatusEnum.ACTIVE)
        {
            throw new ProjectStatusException(MessageConstant.ProjectNotVerifiedError);
        }
        
        if (investmentCallCreateDto.EquityShareCall > project.RemainingPercentOfShares || investmentCallCreateDto.EquityShareCall > 49)
        {
            throw new InvalidInputException(MessageConstant.InvalidEquityShare);
        }
        
        try
        {
            _unitOfWork.BeginTransaction();
            InvestmentCall call = new InvestmentCall
            {
                ProjectId = projectId,
                TargetCall = investmentCallCreateDto.TargetCall,
                ValuePerPercentage = investmentCallCreateDto.ValuePerPercentage,
                AmountRaised = 0,
                TotalInvestor = 0,
                EquityShareCall = investmentCallCreateDto.EquityShareCall,
                StartDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddHours(7).Date),
                EndDate = investmentCallCreateDto.EndDate,
                Status = InvestmentCallStatus.Open,
                RemainAvailableEquityShare = investmentCallCreateDto.EquityShareCall
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

    public async Task CloseOverdueInvestmentCalls()
    {
        var tomorrowDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(1).UtcDateTime);
        var investmentCalls = await _investmentCallRepository.QueryHelper()
            .Filter(c => c.Status == InvestmentCallStatus.Open && (c.EndDate != null && c.EndDate < tomorrowDate))
            .GetAllAsync();
        foreach (var call in investmentCalls)
        {
            call.Status = InvestmentCallStatus.Closed;
            _investmentCallRepository.Update(call);
            
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<InvestmentCall> GetInvestmentCallById(string projectId, string callId)
    {
        var project = await _projectRepository.GetProjectById(projectId);
        
        if (project is null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }
        
        var investmentCall = await _investmentCallRepository.QueryHelper()
            .Include(p => p.DealOffers)
            .Filter(ic => ic.Id.Equals(callId)).GetOneAsync();
        
        if (investmentCall == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundInvestmentCall);
        }

        return investmentCall;
    }

    public async Task<PaginationDTO<InvestmentCallResponseDTO>> GetInvestmentCallByProjectId(string userId, string projectId, InvestmentCallSearchDTO investmentCallSearchDTO, int page, int size)
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        var searchResult = _investmentCallRepository.GetInvestmentCallByProjectId(projectId);

        //ToDo: add filter category
        if (investmentCallSearchDTO.Status.HasValue)
            searchResult = searchResult.Where(ic => ic.Status == investmentCallSearchDTO.Status.Value);

        if (investmentCallSearchDTO.FromAmountRaised.HasValue)
            searchResult = searchResult.Where(ic => ic.AmountRaised >= investmentCallSearchDTO.FromAmountRaised.Value);

        if (investmentCallSearchDTO.ToAmountRaised.HasValue)
            searchResult = searchResult.Where(ic => ic.AmountRaised <= investmentCallSearchDTO.ToAmountRaised.Value);

        if (investmentCallSearchDTO.FromEquityShareCall.HasValue)
            searchResult = searchResult.Where(ic => ic.EquityShareCall >= investmentCallSearchDTO.FromEquityShareCall.Value);

        if (investmentCallSearchDTO.ToEquityShareCall.HasValue)
            searchResult = searchResult.Where(ic => ic.EquityShareCall <= investmentCallSearchDTO.ToEquityShareCall.Value);

        if (investmentCallSearchDTO.FromTargetCall.HasValue)
            searchResult = searchResult.Where(ic => ic.TargetCall >= investmentCallSearchDTO.FromTargetCall.Value);

        if (investmentCallSearchDTO.ToTargetCall.HasValue)
            searchResult = searchResult.Where(ic => ic.TargetCall <= investmentCallSearchDTO.ToTargetCall.Value);

        if (investmentCallSearchDTO.StartDate.HasValue)
            searchResult = searchResult.Where(ic => ic.StartDate >= investmentCallSearchDTO.StartDate.Value);

        if (investmentCallSearchDTO.EndDate.HasValue)
            searchResult = searchResult.Where(ic => ic.EndDate <= investmentCallSearchDTO.EndDate.Value);

        // Get total count after filtering

        int totalCount = await searchResult.CountAsync();
        var pagedResult = await searchResult
                .Skip((page - 1) * size)
                .Take(size)
                .Include(p => p.DealOffers.Where(x=>x.DeletedTime == null))
                .ThenInclude(d => d.Investor)
                .OrderByDescending(p => p.CreatedTime)
                .ToListAsync();
        var investmentCallSearchResponse = pagedResult.Select(investmentCall => new InvestmentCallResponseDTO
        {
            Id = investmentCall.Id,
            AmountRaised = investmentCall.AmountRaised.ToString(),
            EndDate = investmentCall.EndDate,
            EquityShareCall = investmentCall.EquityShareCall.ToString(),
            ProjectId = investmentCall.ProjectId,
            RemainAvailableEquityShare = investmentCall.RemainAvailableEquityShare.ToString(),
            StartDate = investmentCall.StartDate,
            Status = investmentCall.Status,
            TargetCall = investmentCall.TargetCall.ToString(),
            TotalInvestor = investmentCall.TotalInvestor,
            DealOffers = investmentCall.DealOffers.Select(dealOffer => new DealOfferForProjectResponseDTO
            {
                Id = dealOffer.Id,
                Amount = dealOffer.Amount.ToString(),
                DealStatus = dealOffer.DealStatus,
                EquityShareOffer = dealOffer.EquityShareOffer.ToString(),
                InvestorId = dealOffer.InvestorId,
                InvestorName = dealOffer.Investor.FullName,
                TermCondition = dealOffer.TermCondition

            }).ToList()
        });

        var response = new PaginationDTO<InvestmentCallResponseDTO>
        {
            Data = investmentCallSearchResponse,
            Total = totalCount,
            Page = page,
            Size = size
        };

        return response;
    }


}