using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories;

public class MeetingNoteRepository : GenericRepository<MeetingNote, string>, IMeetingNoteRepository
{
    private readonly AppDbContext _context;
    public MeetingNoteRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }
}