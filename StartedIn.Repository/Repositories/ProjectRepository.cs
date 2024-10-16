using Microsoft.EntityFrameworkCore;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories;

public class ProjectRepository : GenericRepository<Project, string>, IProjectRepository
{
    private readonly AppDbContext _appDbContext;
    
    public ProjectRepository(AppDbContext context) : base(context)
    {
        _appDbContext = context;
    }
    
    public async Task<Project> GetProjectById(string id)
    {
        var project = await _appDbContext.Projects.Where(p => p.Id.Equals(id))
                .FirstOrDefaultAsync();
        return project;
    }
}