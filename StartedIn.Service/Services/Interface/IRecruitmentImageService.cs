using Microsoft.AspNetCore.Http;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IRecruitmentImageService
{
    Task AddImageToRecruitmentPost(string recruitmentId, List<IFormFile> recruitFiles);
    Task RemoveImageFromRecruitmentPost(string recruitmentId, string recruitFileId);
}