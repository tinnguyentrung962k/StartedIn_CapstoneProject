using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class InvestmentContractUpdateDTO
    {
        public ContractUpdateDTO Contract { get; set; }
        public EquityShareUpdateDTO InvestorInfo { get; set; }
        public List<DisbursementUpdateDTO>? Disbursements { get; set; }
    }
}
