using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Transaction
{
    public class TransactionFilterDTO
    {
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        public string? FromName { get; set; }
        public string? ToName { get; set; }
        public TransactionType? Type { get; set; }
        public bool? IsInFlow { get; set; }
    }
}
