using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Contract
{
    public class LiquidationNoteDetailResponseDTO : IdentityResponseDTO
    {
        public string ContractName { get; set; }
        public ContractStatusEnum ContractStatus { get; set; }
        public string ContractPolicy { get; set; }
        public string ContractIdNumber { get; set; }
        public string ProjectName { get; set; }
        public string ParentContractId { get; set; }
        public List<UserInContractResponseDTO> Parties { get; set; }
    }
}
