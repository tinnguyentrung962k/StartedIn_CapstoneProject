using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Recruitment;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IRecruitmentService
{
    Task<Recruitment> CreateRecruitment(string userId, string projectId, CreateRecruitmentDTO createRecruitmentDto);

    Task<Recruitment> GetRecruitmentPostById(string projectId, string recruitmentId);

    Task<Recruitment> UpdateRecruitment(string userId, string projectId, string recruitmentId,
        UpdateRecruitmentDTO updateRecruitmentDto);

    Task<PaginationDTO<RecruitmentListDTO>> GetRecruitmentListWithLeader(int page, int size);
}