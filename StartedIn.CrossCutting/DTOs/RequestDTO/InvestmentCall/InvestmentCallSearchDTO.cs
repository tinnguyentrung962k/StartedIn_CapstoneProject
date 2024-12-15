using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.InvestmentCall
{
    public class InvestmentCallSearchDTO
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public InvestmentCallStatus? Status { get; set; }
        public decimal? FromAmountRaised { get; set; }
        public decimal? ToAmountRaised { get; set; }
        public decimal? FromEquityShareCall { get; set; }
        public decimal? ToEquityShareCall { get; set; }
        public decimal? FromTargetCall { get; set; }
        public decimal? ToTargetCall { get; set; }
    }
}
