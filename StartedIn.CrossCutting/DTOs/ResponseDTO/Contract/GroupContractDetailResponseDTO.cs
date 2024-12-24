using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Contract
{
    public class GroupContractDetailResponseDTO : ContractResponseDTO
    {
        public string ProjectName { get; set; }
        public List<UserShareEquityInContractResponseDTO> UserShareEquityInContract { get; set; }
    }
}
