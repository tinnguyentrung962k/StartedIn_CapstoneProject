using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Appointment;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.TransferLeaderRequest
{
    public class TransferLeaderRequestDetailDTO : IdentityResponseDTO
    {
        public string ProjectId { get; set; }
        public string FormerLeaderId { get; set; }
        public string FormerLeaderName { get; set; }
        public bool? IsAgreed { get; set; }
        public string AppointmentId { get; set; }
        public string AppointmentTitle { get; set; }
        public DateTimeOffset AppointmentTime { get; set; }
        public string Description { get; set; }
        public MeetingStatus MeetingStatus { get; set; }
        public List<MeetingNoteResponseDTO> MeetingNotes { get; set; }
    }
}
