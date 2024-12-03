using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Contract
{
    public class ContractInClosingProjectDTO : IdentityResponseDTO
    {
        public string ContractName { get; set; }
        public string ContractIdNumber { get; set; }
        public ContractTypeEnum ContractType { get; set; }
    }
}
