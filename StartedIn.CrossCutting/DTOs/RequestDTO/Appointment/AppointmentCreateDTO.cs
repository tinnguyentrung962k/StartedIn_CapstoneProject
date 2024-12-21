using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Appointment
{
    public class AppointmentCreateDTO
    {
        public string? MilestoneId { get; set; }

        [Required(ErrorMessage = "Vui lòng điền tiêu đề cuộc họp")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng điền thời gian bắt đầu cuộc họp")]
        public DateTimeOffset AppointmentTime { get; set; }
        [Required(ErrorMessage = "Vui lòng điền thời gian kết thúc cuộc họp")]
        public DateTimeOffset AppointmentEndTime { get; set; }

        public string? Description { get; set; }
        [Required(ErrorMessage = "Vui lòng điền link của cuộc họp")]
        public string MeetingLink { get; set; }
    }
}
