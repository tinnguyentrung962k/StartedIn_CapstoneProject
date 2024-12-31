using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace StartedIn.Repository.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment, string>, IAppointmentRepository
    {
        private readonly AppDbContext _appDbContext;
        public AppointmentRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public IQueryable<Appointment> GetAppointmentsByProjectId(string projectId)
        {
            var appointments = _appDbContext.Appointments.Where(a => a.ProjectId.Equals(projectId))
                .Include(a => a.Milestone)
                .Include(a => a.MeetingNotes)
                .Include(a => a.Contract)
                .OrderBy(a => a.AppointmentTime);
            return appointments;
        }

        public async Task<List<Appointment>> GetAppointmentsByContractId(string projectId, string contractId)
        {
            var appointments = await _appDbContext.Appointments.Where(x=>x.ProjectId.Equals(projectId) && x.TerminationRequest.Contract.Id.Equals(contractId))
                .Include(a => a.TerminationRequest)
                .ThenInclude(t => t.Contract)
                .Include(a => a.MeetingNotes)
                .OrderBy(a => a.AppointmentTime).ToListAsync();
            return appointments;
        }
        
        public async Task AddUserToAppointment(string userId, string appointmentId)
        {
            var userAppointment = new UserAppointment
            {
                UserId = userId,
                AppointmentId = appointmentId,
            };
            await _appDbContext.Set<UserAppointment>().AddAsync(userAppointment);
        }
    }
}
