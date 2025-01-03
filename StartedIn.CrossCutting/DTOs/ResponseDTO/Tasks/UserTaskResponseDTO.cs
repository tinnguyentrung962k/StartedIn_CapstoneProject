namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;

public class UserTaskResponseDTO
{
    public string UserId { get; set; }
    public string TaskId { get; set; }
    public string TaskName { get; set; }
    public string FullName { get; set; }
    public float ActualManHour { get; set; }
    public DateTimeOffset? LastUpdatedTime { get; set; }
}