using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface IAppointmentRepository : IGenericRepository<Appointment,string>
    {
        IQueryable<Appointment> GetAppointmentsByProjectId(string projectId);
        Task<List<Appointment>> GetAppointmentsByContractId(string projectId, string contractId);
        Task AddUserToAppointment(string userId, string appointmentId);
    }
}
