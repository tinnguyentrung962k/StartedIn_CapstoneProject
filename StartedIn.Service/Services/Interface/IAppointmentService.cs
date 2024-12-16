using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Appointment;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.Service.Services.Interface
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetAppointmentsInProject(string userId, string projectId, int year);
        Task<Appointment> CreateAnAppointment(string userId, string projectId, AppointmentCreateDTO appointmentCreateDTO);
        Task<Appointment> GetAppointmentsById(string userId, string projectId, string appointmentId);
        Task<PaginationDTO<AppointmentResponseDTO>> GetAppointmentsByProjectId(string userId, string projectId, int page, int size);
        Task UpdateAppointmentStatus(string userId, string projectId, string appointmentId, MeetingStatus status);
    }
}
