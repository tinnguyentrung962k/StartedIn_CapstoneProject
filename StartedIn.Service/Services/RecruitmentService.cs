using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
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
    public RecruitmentService(IProjectRepository projectRepository, IAzureBlobService azureBlobService, IUnitOfWork unitOfWork,
        IRecruitmentRepository recruitmentRepository, IRecruitmentImageRepository recruitmentImageRepository,
        ILogger<RecruitmentService> logger)
    {
        _projectRepository = projectRepository;
        _azureBlobService = azureBlobService;
        _unitOfWork = unitOfWork;
        _recruitmentRepository = recruitmentRepository;
        _recruitmentImageRepository = recruitmentImageRepository;
        _logger = logger;
    }
    public async Task<Recruitment> CreateRecruitment(string projectId, CreateRecruitmentDTO createRecruitmentDto)
    {
        var project = await _projectRepository.QueryHelper().Filter(p => p.Id.Equals(projectId)).GetOneAsync();
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
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
}