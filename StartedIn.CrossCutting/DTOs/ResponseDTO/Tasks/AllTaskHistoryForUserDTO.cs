using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;

public class AllTaskHistoryForUserDTO
{
    public string ProjectName { get; set; }
    public UserStatusInProject UserStatusInProject { get; set; }
    public string TotalManHoursInProject { get; set; }
    public List<UserTaskInTaskHistoryDTO> UserTasks { get; set; }
}