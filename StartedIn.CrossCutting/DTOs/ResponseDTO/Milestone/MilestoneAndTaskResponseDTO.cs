namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone
{
    public class MilestoneAndTaskResponseDTO
    {
        public MilestoneResponseDTO Milestone { get; set; }
        public List<TaskResponseDTO> Tasks { get; set; }
    }
}
