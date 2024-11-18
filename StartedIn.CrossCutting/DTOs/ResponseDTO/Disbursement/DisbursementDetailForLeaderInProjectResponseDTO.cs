using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement
{
    public class DisbursementDetailForLeaderInProjectResponseDTO : DisbursementForLeaderInProjectResponseDTO
    {
        public string InvestorId { get; set; }
        public string ContractId { get; set; }
        public string DeclineReason { get; set; }
        public List<DisbursementAttachmentResponseDTO> DisbursementAttachments { get; set; }
    }
}
