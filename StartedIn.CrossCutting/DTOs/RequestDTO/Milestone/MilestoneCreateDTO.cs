using StartedIn.CrossCutting.Enum;
using System.ComponentModel.DataAnnotations;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Milestone
{
    public class MilestoneCreateDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tên cột mốc")]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public string? PhaseId { get; set; }
    }
}
