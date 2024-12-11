namespace StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;

public class MeetRequestDTO
{
    public string Summary { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}