using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Appointment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.TransferLeaderRequest
{
    public class TransferLeaderHistoryResponseDTO : IdentityResponseDTO
    {
        public string ProjectId { get; set; }
        public string FormerLeaderId { get; set; }
        public string FormerLeaderName { get; set; }
        public string NewLeaderId { get; set; }
        public string NewLeaderName { get; set; }
        public DateOnly? TransferDate { get; set; }
        public bool? IsAgreed { get; set; }
        public string AppointmentId { get; set; }
        public string AppointmentName { get; set; }
        public DateTimeOffset AppointmentTime { get; set; }
        public List<MeetingNoteResponseDTO>? MeetingNotes { get; set; }
    }
}
