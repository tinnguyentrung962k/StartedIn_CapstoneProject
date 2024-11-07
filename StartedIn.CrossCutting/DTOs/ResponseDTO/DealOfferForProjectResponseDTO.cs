using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class DealOfferForProjectResponseDTO : IdentityResponseDTO
    {
        public string InvestorId { get; set; }
        public string InvestorName { get; set; }
        public string Amount { get; set; }
        public string EquityShareOffer { get; set; }
        public string TermCondition { get; set; }
        public DealStatusEnum DealStatus { get; set; }
    }
}
