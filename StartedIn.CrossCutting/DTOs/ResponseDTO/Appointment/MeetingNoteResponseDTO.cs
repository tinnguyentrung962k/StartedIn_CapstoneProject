using StartedIn.CrossCutting.DTOs.BaseDTO;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Appointment;

public class MeetingNoteResponseDTO : IdentityResponseDTO
{
    public string AppointmentId { get; set; }
    public string MeetingNoteLink { get; set; }
    public string FileName { get; set; }
}