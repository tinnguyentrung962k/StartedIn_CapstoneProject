using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories;

public class ApplicationFileRepository : GenericRepository<ApplicationFile, string>, IApplicationFileRepository
{
    private readonly AppDbContext _appDbContext;
    public ApplicationFileRepository(AppDbContext context) : base(context)
    {
        _appDbContext = context;
    }
}