﻿using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement
{
    public class DisbursementDetailForInvestorResponseDTO : DisbursementForInvestorInInvestorMenuResponseDTO
    {
        public string ProjectId { get; set; }
        public string ContractId { get; set; }
        public string DeclineReason { get; set; }
        public string Condition { get; set; }
        public List<DisbursementAttachmentResponseDTO> DisbursementAttachments { get; set; }
    }
}
