using Microsoft.AspNetCore.Http;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;

public class UploadMeetingNoteDTO
{
    public IFormFile Note { get; set; }
}