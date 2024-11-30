using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services;

public class RecruitmentService : IRecruitmentService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IAzureBlobService _azureBlobService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRecruitmentRepository _recruitmentRepository;
    private readonly IRecruitmentImageRepository _recruitmentImageRepository;
    private readonly ILogger<RecruitmentService> _logger;
    private readonly IUserService _userService;
    public RecruitmentService(IProjectRepository projectRepository, IAzureBlobService azureBlobService, IUnitOfWork unitOfWork,
        IRecruitmentRepository recruitmentRepository, IRecruitmentImageRepository recruitmentImageRepository,
        ILogger<RecruitmentService> logger, IUserService userService)
    {
        _projectRepository = projectRepository;
        _azureBlobService = azureBlobService;
        _unitOfWork = unitOfWork;
        _recruitmentRepository = recruitmentRepository;
        _recruitmentImageRepository = recruitmentImageRepository;
        _logger = logger;
        _userService = userService;
    }
    public async Task<Recruitment> CreateRecruitment(string projectId, string userId, CreateRecruitmentDTO createRecruitmentDto)
    {
        var project = await _projectRepository.QueryHelper().Filter(p => p.Id.Equals(projectId)).GetOneAsync();
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }
        
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
        if (projectRole != RoleInTeam.Leader)
        {
            throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
        }

        try
        {
            _unitOfWork.BeginTransaction();
            var recruitment = new Recruitment
            {
                ProjectId = projectId,
                Content = createRecruitmentDto.Content,
                Title = createRecruitmentDto.Title
            };
            var recruitmentEntity = _recruitmentRepository.Add(recruitment);
            foreach (var recruitFile in createRecruitmentDto.recruitFiles)
            {
                string url = await _azureBlobService.UploadRecruitmentImage(recruitFile);
                var recruitmentImg = new RecruitmentImg
                {
                    RecruitmentId = recruitmentEntity.Id,
                    FileName = Path.GetFileName(recruitFile.FileName),
                    ImageUrl = url
                };
                var recruitmentImgEntity = _recruitmentImageRepository.Add(recruitmentImg);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return recruitmentEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating recruitment post.");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Recruitment> GetRecruitmentPostById(string projectId, string recruitmentId)
    {
        var recruitment = await _recruitmentRepository.GetRecruitmentPostById(projectId, recruitmentId);
        if (recruitment == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundRecruitmentPost);
        }

        return recruitment;
    }
}