using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement
{
    public class DisbursementInClosingProjectDTO : IdentityResponseDTO
    {
        public string Title { get; set; }
        public string Amount { get; set; }
        public string ContractIdNumber { get; set; }
    }
}
