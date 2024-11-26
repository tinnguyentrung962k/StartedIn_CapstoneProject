using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories;

public class TaskAttachmentRepository : GenericRepository<TaskAttachment, string>, ITaskAttachmentRepository
{
    private readonly AppDbContext _appDbContext;
    public TaskAttachmentRepository(AppDbContext context) : base(context)
    {
        _appDbContext = context;
    }
}