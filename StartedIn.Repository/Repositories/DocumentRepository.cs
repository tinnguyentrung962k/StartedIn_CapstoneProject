using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories;

public class DocumentRepository : GenericRepository<Document, string>, IDocumentRepository
{
    private readonly AppDbContext _appDbContext;
    public DocumentRepository(AppDbContext context) : base(context)
    {
        _appDbContext = context;
    }
}