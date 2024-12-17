using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories;

public interface IApplicationFileRepository : IGenericRepository<ApplicationFile, string>
{
    
}