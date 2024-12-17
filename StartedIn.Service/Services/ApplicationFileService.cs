using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services;

public class ApplicationFileService : IApplicationFileService
{
    private readonly IRecruitInviteService _recruitInviteService;
    private readonly IApplicationFileRepository _applicationFileRepository;
    private readonly IAzureBlobService _azureBlobService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    private readonly ILogger<ApplicationFileService> _logger;

    public ApplicationFileService(IRecruitInviteService recruitInviteService, IApplicationFileRepository applicationFileRepository, IAzureBlobService azureBlobService, IUnitOfWork unitOfWork, IUserService userService, ILogger<ApplicationFileService> logger)
    {
        _recruitInviteService = recruitInviteService;
        _applicationFileRepository = applicationFileRepository;
        _azureBlobService = azureBlobService;
        _unitOfWork = unitOfWork;
        _userService = userService;
        _logger = logger;
    }

    public Task<ApplicationFile> AddFileToApplication(string userId, string applicationId, List<IFormFile> applicationFiles)
    {
        throw new NotImplementedException();
    }

    public Task RemoveFileFromApplication(string userId, string applicationId, string applicationFileId)
    {
        throw new NotImplementedException();
    }
}