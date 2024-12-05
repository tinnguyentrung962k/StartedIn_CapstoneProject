using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IRecruitmentRepository _recruitmentRepository;
    private readonly IAzureBlobService _azureBlobService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApplicationService> _logger;

    public ApplicationService(IApplicationRepository applicationRepository, IProjectRepository projectRepository,
        IRecruitmentRepository recruitmentRepository, IAzureBlobService azureBlobService, IUnitOfWork unitOfWork,
        ILogger<ApplicationService> logger)
    {
        _applicationRepository = applicationRepository;
        _projectRepository = projectRepository;
        _recruitmentRepository = recruitmentRepository;
        _azureBlobService = azureBlobService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<Application> CreateApplication(CreateApplicationDTO createApplicationDto, string userId, string projectId)
    {
        var project = await _projectRepository.QueryHelper().Filter(p => p.Id.Equals(projectId)).Include(p => p.UserProjects).GetOneAsync();
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }
        var memberList = project.UserProjects.Count();
        if (memberList == project.MaxMember)
        {
            throw new InvalidInputException(MessageConstant.ProjectMaxMemberExceeded);
        }
        var recruitment = await _recruitmentRepository.QueryHelper().Filter(r => r.ProjectId.Equals(projectId)).GetOneAsync();
        if (recruitment == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundRecruitmentPost);
        }
        try
        {
            _unitOfWork.BeginTransaction();
            var cvUrl = await _azureBlobService.UploadCv(createApplicationDto.CV);
            var application = new Application
            {
                CandidateId = userId,
                RecruitmentId = recruitment.Id,
                ProjectId = projectId,
                CVUrl = cvUrl,
                Type = ApplicationTypeEnum.APPLY,
                Status = ApplicationStatus.PENDING,
                Role = RoleInTeam.Member
            };

            var applicationEntity = _applicationRepository.Add(application);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return applicationEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while creating the application: {ex.Message}");
            await _unitOfWork.RollbackAsync();
            throw;
        }
        
    }
}