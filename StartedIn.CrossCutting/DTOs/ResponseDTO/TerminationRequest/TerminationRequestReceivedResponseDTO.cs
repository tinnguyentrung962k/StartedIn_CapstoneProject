using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationRequest
{
    public class TerminationRequestReceivedResponseDTO : IdentityResponseDTO
    {
        public string ContractId { get; set; }
        public string ContractIdNumber { get; set; }
        public string Reason { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public string FromId { get; set; }
        public string FromName { get; set; }
        public bool? IsAgreed { get; set; }
        public DateTimeOffset LastUpdatedTime { get; set; }
    }
}
