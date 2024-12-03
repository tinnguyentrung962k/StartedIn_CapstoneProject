using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment, string>, IAppointmentRepository
    {
        public AppointmentRepository(AppDbContext context) : base(context)
        {
        }
    }
}
