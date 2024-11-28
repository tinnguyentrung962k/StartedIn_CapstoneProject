using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IRecruitmentService
{
    Task<Recruitment> CreateRecruitment(string projectId, CreateRecruitmentDTO createRecruitmentDto);
}