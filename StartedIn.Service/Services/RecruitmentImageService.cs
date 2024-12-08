using DocumentFormat.OpenXml.Spreadsheet;
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
    private readonly IUserService _userService;
    private readonly ILogger<RecruitmentImageService> _logger;

    public RecruitmentImageService(IRecruitmentRepository recruitmentRepository,
        IRecruitmentImageRepository recruitmentImageRepository,
        IAzureBlobService azureBlobService,
        IUserService userService,
        IUnitOfWork unitOfWork)
    {
        _recruitmentImageRepository = recruitmentImageRepository;
        _recruitmentRepository = recruitmentRepository;
        _azureBlobService = azureBlobService;
        _userService = userService;
        _unitOfWork = unitOfWork;
    }

    public async Task<RecruitmentImg> AddImageToRecruitmentPost(string userId, string projectId, IFormFile recruitFile)
    {
        var recruitment = await _recruitmentRepository.QueryHelper().Filter(r => r.ProjectId.Equals(projectId)).GetOneAsync();
        if (recruitment == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundRecruitmentPost);
        }

        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

        try
        {
            _unitOfWork.BeginTransaction();
            var url = await _azureBlobService.UploadRecruitmentImage(recruitFile);
            var newImage = new RecruitmentImg
            {
                FileName = recruitFile.FileName,
                ImageUrl = url,
                RecruitmentId = recruitment.Id
            };
            var image = _recruitmentImageRepository.Add(newImage);

            await _unitOfWork.CommitAsync();
            await _unitOfWork.SaveChangesAsync();
            return image;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating recruitment post.");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task RemoveImageFromRecruitmentPost(string userId, string projectId, string recruitFileId)
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

        var recruitFile = await _recruitmentImageRepository.QueryHelper().Filter(r => r.Id.Equals(recruitFileId))
            .GetOneAsync();

        if (recruitFile == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundRecruitmentImg);
        }

        try
        {
            _unitOfWork.BeginTransaction();
            await _azureBlobService.DeleteImageFromRecruitmentBlob(recruitFile.ImageUrl);
            await _recruitmentImageRepository.SoftDeleteById(recruitFileId);

            await _unitOfWork.CommitAsync();
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while removing recruitment image.");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}