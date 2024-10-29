using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class DealOfferCreateDTO
    {
        public string ProjectId { get; set; }
        public decimal Amount { get; set; }
        public decimal EquityShareOffer { get; set; }
        public string TermCondition { get; set; }
    }
}
