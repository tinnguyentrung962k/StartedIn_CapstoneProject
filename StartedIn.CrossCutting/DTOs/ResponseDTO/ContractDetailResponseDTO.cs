using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class ContractDetailResponseDTO : ContractResponseDTO
    {
        public List<DisbursementInContractResponseDTO> Disbursements { get; set; }
    }
}
