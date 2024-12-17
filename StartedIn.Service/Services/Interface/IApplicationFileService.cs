using Microsoft.AspNetCore.Http;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IApplicationFileService
{
    Task<ApplicationFile> AddFileToApplication(string userId, string recruitmentId, List<IFormFile> applicationFiles);
    Task RemoveFileFromApplication(string userId, string recruitmentId, string applicationFileId);
}