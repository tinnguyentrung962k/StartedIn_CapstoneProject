using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Appointment
{
    public class AppointmentResponseDTO : IdentityResponseDTO
    {
        public string ProjectId { get; set; }
        public string? MilestoneId { get; set; }
        public string? MilestoneName { get; set; }
        public string Title { get; set; }
        public DateTimeOffset AppointmentTime { get; set; }
        public string? Description { get; set; }
        public string MeetingLink { get; set; }
        public MeetingStatus Status { get; set; }
    }
}
