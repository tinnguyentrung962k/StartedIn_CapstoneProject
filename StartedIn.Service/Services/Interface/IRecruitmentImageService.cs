using Microsoft.AspNetCore.Http;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IRecruitmentImageService
{
    Task<RecruitmentImg> AddImageToRecruitmentPost(string userId, string recruitmentId, IFormFile recruitFile);
    Task RemoveImageFromRecruitmentPost(string userId, string recruitmentId, string recruitFileId);
}