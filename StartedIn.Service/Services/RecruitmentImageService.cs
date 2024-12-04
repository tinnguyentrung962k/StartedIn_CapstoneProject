using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services;

public class RecruitmentImageService : IRecruitmentImageService
{
    private readonly IRecruitmentRepository _recruitmentRepository;
    private readonly IRecruitmentImageRepository _recruitmentImageRepository;
    private readonly IAzureBlobService _azureBlobService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecruitmentImageService> _logger;

    public RecruitmentImageService(IRecruitmentRepository recruitmentRepository,
        IRecruitmentImageRepository recruitmentImageRepository,
        IAzureBlobService azureBlobService,
        IUnitOfWork unitOfWork)
    {
        _recruitmentImageRepository = recruitmentImageRepository;
        _recruitmentRepository = recruitmentRepository;
        _azureBlobService = azureBlobService;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<List<RecruitmentImg>> AddImageToRecruitmentPost(string projectId, List<IFormFile> recruitFiles)
    {
        var recruitment = await _recruitmentRepository.QueryHelper().Filter(r => r.ProjectId.Equals(projectId)).GetOneAsync();
        if (recruitment == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundRecruitmentPost);
        }

        List<RecruitmentImg> listImages = new List<RecruitmentImg>();
        try
        {
            _unitOfWork.BeginTransaction();
            foreach (var recruitFile in recruitFiles)
            {
                var url = await _azureBlobService.UploadRecruitmentImage(recruitFile);
                var newImage = new RecruitmentImg
                {
                    FileName = recruitFile.FileName,
                    ImageUrl = url,
                    RecruitmentId = recruitment.Id
                };
                var image = _recruitmentImageRepository.Add(newImage);
                listImages.Add(image);
            }

            return listImages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating recruitment post.");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task RemoveImageFromRecruitmentPost(string recruitmentId, string recruitFileId)
    {
        var recruitment = await _recruitmentRepository.QueryHelper().Filter(r => r.Id.Equals(recruitmentId)).GetOneAsync();
        if (recruitment == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundRecruitmentPost);
        }

        var recruitFile = await _recruitmentImageRepository.QueryHelper().Filter(r => r.Id.Equals(recruitFileId))
            .GetOneAsync();
        if (recruitFile == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundRecruitmentImg);
        }
        await _azureBlobService.DeleteImageFromRecruitmentBlob(recruitFile.ImageUrl);
        await _recruitmentImageRepository.SoftDeleteById(recruitmentId);
    }
}