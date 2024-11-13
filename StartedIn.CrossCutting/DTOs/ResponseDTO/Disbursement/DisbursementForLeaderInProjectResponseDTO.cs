using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement
{
    public class DisbursementForLeaderInProjectResponseDTO : IdentityResponseDTO
    {
        public string Title { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Amount { get; set; }
        public DisbursementStatusEnum DisbursementStatus { get; set; }
        public string InvestorName { get; set; }
        public string ContractIdNumber { get; set; }
    }
}
