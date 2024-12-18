using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Appointment
{
    public class AppointmentFilterDTO
    {
        public string? MilestoneId { get; set; }
        public string? Title { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public MeetingStatus? MeetingStatus { get; set; }
        public bool? IsDescending { get; set; }
    }
}
