using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Recruitment;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IRecruitmentService
{
    Task<Recruitment> CreateRecruitment(string userId, string projectId, CreateRecruitmentDTO createRecruitmentDto);

    // Get recruitment details in the recruitment list of guests / outside users
    Task<Recruitment> GetRecruitmentPostById(string recruitmentId);

    Task<Recruitment> GetRecruitmentPostInProject(string userId, string projectId);

    Task<Recruitment> UpdateRecruitment(string userId, string projectId, UpdateRecruitmentDTO updateRecruitmentDto);

    Task<PaginationDTO<RecruitmentListDTO>> GetRecruitmentListWithLeader(int page, int size);
}