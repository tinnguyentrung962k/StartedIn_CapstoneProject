using Microsoft.AspNetCore.Http;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;

public class UploadMeetingNoteDTO
{
    public List<IFormFile> Notes { get; set; }
}