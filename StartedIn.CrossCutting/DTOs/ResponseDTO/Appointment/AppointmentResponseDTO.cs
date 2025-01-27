﻿using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Document;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Appointment
{
    public class AppointmentResponseDTO : IdentityResponseDTO
    {
        public string ProjectId { get; set; }
        public string? MilestoneId { get; set; }
        public string? MilestoneName { get; set; }
        public string? ContractId { get; set; }
        public string? ContractName { get; set; }
        public ContractTypeEnum ContractType { get; set; }
        public string Title { get; set; }
        public DateTimeOffset? AppointmentTime { get; set; }
        public DateTimeOffset? AppointmentEndTime { get; set; }
        public string? Description { get; set; }
        public string MeetingLink { get; set; }
        public MeetingStatus Status { get; set; }
        public List<MeetingNoteResponseDTO> MeetingNotes { get; set; }
        public List<DocumentResponseDTO> Documents { get; set; }
    }
}
