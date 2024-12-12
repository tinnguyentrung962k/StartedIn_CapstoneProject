using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationConfirmation
{
    public class TerminationConfirmationResponseDTO : IdentityResponseDTO
    {
        public string TerminationRequestId { get; set; }
        public string ContractId { get; set; }
        public string ContractIdNumber { get; set; }
        public string FromId { get; set; }
        public string FromName { get; set; }
        public bool? IsAgreed { get; set; }
        public DateTimeOffset LastUpdatedTime { get; set; }
    }
}
