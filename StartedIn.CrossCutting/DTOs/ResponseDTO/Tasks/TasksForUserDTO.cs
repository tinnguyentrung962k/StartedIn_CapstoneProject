namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;

public class TasksForUserDTO
{
    public string UserId { get; set; }
    public float? ActualManHourInProject { get; set; }
    public int NotStartedTasks { get; set; }
    public int DoneTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
}