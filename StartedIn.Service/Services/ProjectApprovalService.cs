using AutoMapper;
using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.ProjectApproval;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectApproval;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services;

public class ProjectApprovalService : IProjectApprovalService
{
    private readonly IProjectApprovalRepository _projectApprovalRepository;
    private readonly IProjectService _projectService;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAzureBlobService _azureBlobService;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<ProjectApprovalService> _logger;
    private readonly IMapper _mapper;
    private readonly IInvestmentCallRepository _investmentCallRepository;

    public ProjectApprovalService(IProjectApprovalRepository projectApprovalRepository, IProjectService projectService, IUserService userService, IUnitOfWork unitOfWork,
        IAzureBlobService azureBlobService, IDocumentRepository documentRepository, ILogger<ProjectApprovalService> logger, IMapper mapper, IProjectRepository projectRepository,
        IInvestmentCallRepository investmentCallRepository)
    {
        _projectApprovalRepository = projectApprovalRepository;
        _projectService = projectService;
        _userService = userService;
        _unitOfWork = unitOfWork;
        _azureBlobService = azureBlobService;
        _documentRepository = documentRepository;
        _logger = logger;
        _mapper = mapper;
        _projectRepository = projectRepository;
        _investmentCallRepository = investmentCallRepository;
    }

    public async Task<ProjectApprovalResponseDTO> CreateProjectApprovalRequest(string userId, string projectId, CreateProjectApprovalDTO createProjectApprovalDto)
    {
        var userProject = await _userService.CheckIfUserInProject(userId, projectId);
        var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
        if (projectRole != RoleInTeam.Leader)
        {
            throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
        }

        var existingApproval = await _projectApprovalRepository.QueryHelper()
            .Filter(a => a.ProjectId.Equals(projectId) && (a.Status == ProjectApprovalStatus.PENDING || a.Status == ProjectApprovalStatus.ACCEPTED)).GetOneAsync();
        if (existingApproval != null)
        {

            if (existingApproval.Status == ProjectApprovalStatus.PENDING)
            {
                throw new ExistedRecordException(MessageConstant.NoMoreThanOnePendingApproval);
            }
            else
            {
                throw new ExistedRecordException(MessageConstant.AcceptedApprovalExisted);
            }
        }

        if (createProjectApprovalDto.CompanyIdNumber != null)
        {
            var project = await _projectRepository.QueryHelper()
                .Filter(x => x.CompanyIdNumber.Equals(createProjectApprovalDto.CompanyIdNumber) && x.ProjectStatus != ProjectStatusEnum.CLOSED)
                .GetOneAsync();
            if (project != null) {
                throw new ExistedRecordException(MessageConstant.CompanyIdNumberExisted);
            }
        }

        try
        {
            _unitOfWork.BeginTransaction();
            var projectApproval = new ProjectApproval
            {
                ProjectId = projectId,
                Reason = createProjectApprovalDto.Reason,
                CreatedTime = DateTimeOffset.UtcNow,
                CreatedBy = userProject.User.FullName,
                Status = ProjectApprovalStatus.PENDING
            };
            var entity = _projectApprovalRepository.Add(projectApproval);
            if (!string.IsNullOrWhiteSpace(createProjectApprovalDto.CompanyIdNumber))
            {
                userProject.Project.CompanyIdNumber = createProjectApprovalDto.CompanyIdNumber;
                _projectRepository.Update(userProject.Project);
            }
            var newInvestmentCall = new InvestmentCall
            {
                AmountRaised = 0,
                EndDate = createProjectApprovalDto.EndDate,
                EquityShareCall = createProjectApprovalDto.EquityShareCall,
                ProjectApprovalId = entity.Id,
                RemainAvailableEquityShare = createProjectApprovalDto.EquityShareCall,
                ProjectId = projectId,
                Status = InvestmentCallStatus.Pending,
                TargetCall = createProjectApprovalDto.EquityShareCall * createProjectApprovalDto.ValuePerPercentage,
                ValuePerPercentage = createProjectApprovalDto.ValuePerPercentage,
                Project = userProject.Project,
                CreatedBy = userProject.User.FullName,
                TotalInvestor = 0
            };
            
            var newInvestmentCallEntity = _investmentCallRepository.Add(newInvestmentCall);

            foreach (var document in createProjectApprovalDto.Documents)
            {
                var link = await _azureBlobService.UploadMeetingNoteAndProjectDocuments(document);
                var createdDocument = new Document
                {
                    ProjectId = projectId,
                    ProjectApprovalId = entity.Id,
                    AttachmentLink = link,
                    DocumentName = document.FileName,
                    CreatedTime = DateTimeOffset.UtcNow
                };
                _documentRepository.Add(createdDocument);
            }

            var response = _mapper.Map<ProjectApprovalResponseDTO>(entity);
            response.EndDate = newInvestmentCallEntity.EndDate;
            response.EquityShareCall = newInvestmentCallEntity.EquityShareCall.ToString();
            response.TargetCall = newInvestmentCallEntity.TargetCall.ToString();
            response.CompanyIdNumber = userProject.Project.CompanyIdNumber;
            await _unitOfWork.CommitAsync();
            await _unitOfWork.SaveChangesAsync();
            return response;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error while creating project approval request for project {projectId} by user {userId}: {exception}");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<List<ProjectApprovalResponseDTO>> GetProjectApprovalRequestByProjectId(string userId, string projectId)
    {
        var userProject = await _userService.CheckIfUserInProject(userId, projectId);
        var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
        if (projectRole != RoleInTeam.Leader)
        {
            throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
        }
        var approval = await _projectApprovalRepository.GetProjectApprovalsQuery().Where(a => a.ProjectId.Equals(projectId)).ToListAsync();
        if (!approval.Any())
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectApprovalRequest);
        }

        var response = _mapper.Map<List<ProjectApprovalResponseDTO>>(approval);
        foreach (var approvalItem in response)
        {
            var investmentCall = await _investmentCallRepository.QueryHelper()
            .Filter(x => x.ProjectApprovalId.Equals(approvalItem.Id))
            .GetOneAsync();
            approvalItem.TargetCall = investmentCall.TargetCall.ToString();
            approvalItem.EquityShareCall = investmentCall.EquityShareCall.ToString();
            approvalItem.EndDate = investmentCall.EndDate;
            approvalItem.CompanyIdNumber = userProject.Project.CompanyIdNumber;
        }

        return response;
    }

    public async Task ApproveProjectRequest(string approvalId)
    {
        var approval = await _projectApprovalRepository.QueryHelper()
            .Filter(pa => pa.Id.Equals(approvalId))
            .GetOneAsync();
        
        if (approval == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectApprovalRequest);
        }

        var project = await _projectRepository.QueryHelper()
            .Filter(p => p.Id.Equals(approval.ProjectId))
            .GetOneAsync();

        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }

        var investmentCall = await _investmentCallRepository.QueryHelper()
            .Filter(x => x.ProjectApprovalId.Equals(approvalId))
            .GetOneAsync();
        if (investmentCall != null)
        {
            investmentCall.Status = InvestmentCallStatus.Open;
            investmentCall.StartDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddHours(7).Date);
            _investmentCallRepository.Update(investmentCall);
            project.ActiveCallId = investmentCall.Id;
        }
        approval.Status = ProjectApprovalStatus.ACCEPTED;
        project.ProjectStatus = ProjectStatusEnum.ACTIVE;
        approval.LastUpdatedTime = DateTimeOffset.UtcNow;
        _projectApprovalRepository.Update(approval);
        _projectRepository.Update(project);

        
        
        

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectProjectRequest(string approvalId, CancelReasonApprovalDTO cancelReasonDTO)
    {
        var approval = await _projectApprovalRepository.QueryHelper().Filter(pa => pa.Id.Equals(approvalId)).GetOneAsync();
        if (approval == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectApprovalRequest);
        }
        approval.Status = ProjectApprovalStatus.REJECTED;
        approval.LastUpdatedTime = DateTimeOffset.UtcNow;
        approval.RejectReason = cancelReasonDTO.CancelReason;
        _projectApprovalRepository.Update(approval);

        var investmentCall = await _investmentCallRepository.QueryHelper()
            .Filter(x => x.ProjectApprovalId.Equals(approvalId))
            .GetOneAsync();

        if (investmentCall != null)
        {
            investmentCall.Status = InvestmentCallStatus.Cancel;
            _investmentCallRepository.Update(investmentCall);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PaginationDTO<ProjectApprovalResponseDTO>> GetAllProjectApprovals(ProjectApprovalFilterDTO filter, int page, int size)
    {
        var projectApprovals = _projectApprovalRepository.GetProjectApprovalsQuery();
        if (filter.Status != null)
        {
            projectApprovals = projectApprovals.Where(pa => pa.Status == filter.Status);
        }
        
        if (filter.PeriodFrom.HasValue)
        {
            var fromDate = filter.PeriodFrom.Value;
            projectApprovals = projectApprovals.Where(a => DateOnly.FromDateTime(a.CreatedTime.UtcDateTime.AddHours(7).Date) >= fromDate);
        }
        if (filter.PeriodTo.HasValue)
        {
            var toDate = filter.PeriodTo.Value;
            projectApprovals = projectApprovals.Where(a => DateOnly.FromDateTime(a.CreatedTime.UtcDateTime.AddHours(7).Date) <= toDate);
        }

        var pagedResults = await projectApprovals.Skip((page - 1) * size).Take(size).ToListAsync();
        var pagedResultResponse = _mapper.Map<List<ProjectApprovalResponseDTO>>(pagedResults);
        foreach (var pagedResult in pagedResultResponse)
        {
            var investmentCall = await _investmentCallRepository.QueryHelper().Filter(x => x.ProjectApprovalId.Equals(pagedResult.Id)).GetOneAsync();
            pagedResult.EquityShareCall = investmentCall.EquityShareCall.ToString();
            pagedResult.TargetCall = investmentCall.TargetCall.ToString();
            pagedResult.EndDate = investmentCall.EndDate;
            var project = await _projectRepository.GetOneAsync(pagedResult.ProjectId);
            pagedResult.CompanyIdNumber = project.CompanyIdNumber;
        }

        var pagination = new PaginationDTO<ProjectApprovalResponseDTO>()
        {
            Data = pagedResultResponse,
            Total = await projectApprovals.CountAsync(),
            Page = page,
            Size = size
        };

        return pagination;
    }

    public async Task<ProjectApprovalResponseDTO> GetProjectApprovalRequestByApprovalId(string userId, string projectId, string approvalId)
    {
        var userProject = await _userService.CheckIfUserInProject(userId, projectId);
        var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
        if (projectRole != RoleInTeam.Leader)
        {
            throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
        }
        var approval = await _projectApprovalRepository.QueryHelper()
            .Filter(a => a.ProjectId.Equals(projectId) && a.Id.Equals(approvalId)).Include(x=>x.Project).GetOneAsync();
        if (approval == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectApprovalRequest);
        }
        var investmentCall = await _investmentCallRepository.QueryHelper().Filter(x => x.ProjectApprovalId.Equals(approvalId)).GetOneAsync();
        var response = _mapper.Map<ProjectApprovalResponseDTO>(approval);
        response.EquityShareCall = investmentCall.EquityShareCall.ToString();
        response.TargetCall = investmentCall.TargetCall.ToString();
        response.EndDate = investmentCall.EndDate;
        response.CompanyIdNumber = userProject.Project.CompanyIdNumber;
        return response;
    }

    public async Task<ProjectApprovalResponseDTO> GetProjectApprovalRequestByApprovalIdForAdmin(string approvalId)
    {
        var approval = await _projectApprovalRepository.QueryHelper()
            .Filter(a => a.Id.Equals(approvalId)).Include(x=>x.Project).GetOneAsync();
        if (approval == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectApprovalRequest);
        }
        var investmentCall = await _investmentCallRepository.QueryHelper().Filter(x => x.ProjectApprovalId.Equals(approvalId)).GetOneAsync();
        var response = _mapper.Map<ProjectApprovalResponseDTO>(approval);
        response.EquityShareCall = investmentCall.EquityShareCall.ToString();
        response.TargetCall = investmentCall.TargetCall.ToString();
        response.EndDate = investmentCall.EndDate;
        response.CompanyIdNumber = approval.Project.CompanyIdNumber;
        return response;
    }
}