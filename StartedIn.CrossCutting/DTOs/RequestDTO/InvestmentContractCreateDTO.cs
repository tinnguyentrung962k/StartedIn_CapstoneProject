using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class InvestmentContractCreateDTO
    {
        public ContractCreateDTO Contract { get; set; }
        public EquityShareCreateDTO InvestorInfo { get; set; }
        public List<DisbursementCreateDTO>? Disbursements { get; set; } 
    }
}
