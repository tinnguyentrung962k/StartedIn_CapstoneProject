using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Disbursement
{
    public class DisbursementFilterDTO
    {
        public string? Title { get; set; }
        public DateOnly? PeriodFrom { get; set; }
        public DateOnly? PeriodTo { get; set; }
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        public DisbursementStatusEnum? DisbursementStatus { get; set; }
        public string? InvestorId { get; set; }
        public string? ContractId { get; set; }
    }
}
