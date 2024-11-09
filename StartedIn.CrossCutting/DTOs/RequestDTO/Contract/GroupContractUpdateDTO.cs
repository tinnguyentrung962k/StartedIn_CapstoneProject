using StartedIn.CrossCutting.DTOs.RequestDTO.EquityShare;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Contract
{
    public class GroupContractUpdateDTO
    {
        public ContractUpdateDTO Contract { get; set; }
        public List<EquityShareUpdateForMemberDTO> ShareEquitiesOfMembers { get; set; }
    }
}
