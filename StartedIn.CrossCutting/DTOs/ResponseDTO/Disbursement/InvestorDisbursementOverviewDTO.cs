using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement
{
    public class InvestorDisbursementOverviewDTO
    {
        public string TotalDisbursement { get; set; }
        public string TotalDisbursedAmount { get; set; }
        public string NotDisbursedAmount { get; set; }
        public string DisbursedLastMonth { get; set; }
        public string DisbursedCurrentMonth { get; set; }
        public string DisburseNextMonth { get; set; }
    }
}
