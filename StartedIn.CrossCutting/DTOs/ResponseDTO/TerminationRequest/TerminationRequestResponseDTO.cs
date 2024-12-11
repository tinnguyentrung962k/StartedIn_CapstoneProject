using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationRequest
{
    public class TerminationRequestResponseDTO : IdentityResponseDTO
    {
        public string ContractId { get; set; }
        public string ContractIdNumber { get; set; }
        public string FromId { get; set; }
        public string FromName { get; set; }
        public string ToId { get; set; }
        public string ToName { get; set; }
        public string Reason { get; set; }
        public bool? IsAgreed { get; set; }

    }
}
