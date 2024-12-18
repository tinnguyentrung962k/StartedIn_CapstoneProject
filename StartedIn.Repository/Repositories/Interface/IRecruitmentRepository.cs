using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Interface;

public interface IRecruitmentRepository : IGenericRepository<Recruitment, string>
{
    Task<Recruitment> GetRecruitmentPostByProjectId(string projectId);
    Task<Recruitment> GetRecruitmentPostByRecruitmentId(string recruitmentId);
    IQueryable<Recruitment> GetRecruitmentWithLeader();
}