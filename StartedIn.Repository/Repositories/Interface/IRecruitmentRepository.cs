using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Interface;

public interface IRecruitmentRepository : IGenericRepository<Recruitment, string>
{
    Task<Recruitment> GetRecruitmentPostById(string recruitmentId);
    IQueryable<Recruitment> GetRecruitmentWithLeader();
}