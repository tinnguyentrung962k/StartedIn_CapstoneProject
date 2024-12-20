using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;

public class UserTaskInTaskHistoryDTO
{
    public string Title { get; set; }
    public float ActualManHour { get; set; }
    public TaskEntityStatus Status { get; set; }
}