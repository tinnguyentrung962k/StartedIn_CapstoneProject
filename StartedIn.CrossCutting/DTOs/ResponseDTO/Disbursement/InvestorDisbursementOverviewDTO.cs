using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement
{
    public class InvestorDisbursementOverviewDTO
    {
        public DisbursementOverviewItem Overall { get; set; }
        public DisbursementOverviewItem LastMonth { get; set; }
        public DisbursementOverviewItem CurrentMonth { get; set; }
        public DisbursementOverviewItem NextMonth { get; set; }
    }

    public class DisbursementOverviewItem 
    {
        public string TotalDisbursement { get; set; }
        public string DisbursedAmount { get; set; }
        public string NotDisbursedAmount { get; set; }
    }
}
