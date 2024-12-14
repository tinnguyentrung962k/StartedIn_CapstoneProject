using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IMeetingNoteService
{
    Task<MeetingNote> UploadMeetingNote(string userId, string projectId, string appointmentId, UploadMeetingNoteDTO uploadMeetingNoteDto);
    Task<MeetingNote> GetMeetingNoteById(string projectId, string appointmentId, string meetingNoteId);
    Task<List<MeetingNote>> GetMeetingNoteByAppointmentId(string projectId, string appointmentId);
}