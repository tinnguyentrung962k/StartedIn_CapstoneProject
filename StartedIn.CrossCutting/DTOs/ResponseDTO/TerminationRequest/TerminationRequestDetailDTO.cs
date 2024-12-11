using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationRequest
{
    public class TerminationRequestDetailDTO
    {
        public string ContractId { get; set; }
        public string ContractIdNumber { get; set; }
        public string FromId { get; set; }
        public string FromName { get; set; }
        public string Reason { get; set; }
        public List<UserPartyInContractInTerminationResponseDTO> UserParties { get; set; }
    }
}
