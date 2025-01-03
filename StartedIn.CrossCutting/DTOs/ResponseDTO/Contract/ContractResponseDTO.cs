﻿using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Appointment;
using StartedIn.CrossCutting.Enum;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Contract
{
    public class ContractResponseDTO : IdentityResponseDTO
    {
        public string ProjectId { get; set; }
        public string ContractName { get; set; }
        public ContractTypeEnum ContractType { get; set; }
        public ContractStatusEnum ContractStatus { get; set; }
        public string? SignNowDocumentId { get; set; }
        public string? AzureLink { get; set; }
        public string ContractPolicy { get; set; }
        public string ContractIdNumber { get; set; }
        public DateOnly? ValidDate { get; set; }
        public DateOnly? ExpiredDate { get; set; }
        public string ParentContractId { get; set; }
        public string? LiquidationNoteId { get; set; }
        public string? CurrentTerminationRequestId { get; set; }
        public MeetingStatus? MeetingStatus { get; set; }
        public List<AppointmentResponseDTO>? Appointments { get; set; }
    }
}
