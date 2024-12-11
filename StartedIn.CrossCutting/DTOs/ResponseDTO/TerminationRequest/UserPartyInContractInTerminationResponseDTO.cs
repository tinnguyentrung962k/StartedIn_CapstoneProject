using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationRequest
{
    public class UserPartyInContractInTerminationResponseDTO
    {
        public string ToId { get; set; }
        public string ToName { get; set; }
        public bool? IsAgreed { get; set; }
    }
}
