using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.ProjectApproval;
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
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAzureBlobService _azureBlobService;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<ProjectApprovalService> _logger;

    public ProjectApprovalService(IProjectApprovalRepository projectApprovalRepository, IProjectService projectService, IUserService userService, IUnitOfWork unitOfWork,
        IAzureBlobService azureBlobService, IDocumentRepository documentRepository, ILogger<ProjectApprovalService> logger)
    {
        _projectApprovalRepository = projectApprovalRepository;
        _projectService = projectService;
        _userService = userService;
        _unitOfWork = unitOfWork;
        _azureBlobService = azureBlobService;
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<ProjectApproval> CreateProjectApprovalRequest(string userId, string projectId, CreateProjectApprovalDTO createProjectApprovalDto)
    {
        var userProject = await _userService.CheckIfUserInProject(userId, projectId);
        try
        {
            _unitOfWork.BeginTransaction();
            var projectApproval = new ProjectApproval
            {
                ProjectId = projectId,
                CreatedTime = DateTimeOffset.Now,
            };
            var entity = _projectApprovalRepository.Add(projectApproval);

            foreach (var document in createProjectApprovalDto.Documents)
            {
                var link = await _azureBlobService.UploadMeetingNoteAndProjectDocuments(document);
                var createdDocument = new Document
                {
                    ProjectId = projectId,
                    ProjectApprovalId = entity.Id,
                    AttachmentLink = link,
                    DocumentName = document.FileName,
                    CreatedTime = DateTimeOffset.Now
                };
                _documentRepository.Add(createdDocument);
            }

            await _unitOfWork.CommitAsync();
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error while creating project approval request for project {projectId} by user {userId}: {exception}");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<ProjectApproval> GetProjectApprovalRequestByProjectId(string projectId)
    {
        var approval = await _projectApprovalRepository.QueryHelper().Filter(pa => pa.ProjectId.Equals(projectId)).GetOneAsync();
        if (approval == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectApprovalRequest);
        }

        return approval;
    }

    public async Task ApproveProjectRequest(string projectId)
    {
        var approval = await _projectApprovalRepository.QueryHelper().Filter(pa => pa.ProjectId.Equals(projectId)).GetOneAsync();
        if (approval == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectApprovalRequest);
        }

        approval.Status = ProjectApprovalStatus.ACCEPTED;
        _projectApprovalRepository.Update(approval);
    }

    public async Task RejectProjectRequest(string projectId, string rejectReason)
    {
        var approval = await _projectApprovalRepository.QueryHelper().Filter(pa => pa.ProjectId.Equals(projectId)).GetOneAsync();
        if (approval == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectApprovalRequest);
        }

        approval.Status = ProjectApprovalStatus.REJECTED;
        approval.RejectReason = rejectReason;
        _projectApprovalRepository.Update(approval);
    }
}