using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetAppointmentsInProject(string userId, string projectId, int year);
        Task<Appointment> CreateAnAppointment(string userId, string projectId, AppointmentCreateDTO appointmentCreateDTO);
        Task<Appointment> GetAppointmentsById(string userId, string projectId, string appointmentId);
        Task<IEnumerable<Appointment>> GetAppointmentsByProjectId(string userId, string projectId);
    }
}
