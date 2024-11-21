using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement
{
    public class SelfDisbursmentOfInvestorDTO
    {
        public string? SelfRemainingDisbursement { get; set; }
        public string? SelfDisbursedAmount { get; set; }
    }
}
