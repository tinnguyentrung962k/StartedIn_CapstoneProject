using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
using StartedIn.CrossCutting.Enum;
using System.ComponentModel.DataAnnotations;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Milestone
{
    public class MilestoneCreateDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tên cột mốc")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string Description { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public string? PhaseId { get; set; }
        public IList<AppointmentCreateDTO> meetingList { get; set; }
    }
}
