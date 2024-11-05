using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class DealOfferForInvestorResponseDTO : IdentityResponseDTO
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Amount { get; set; }
        public string EquityShareOffer { get; set; }
        public string TermCondition { get; set; }
        public DealStatusEnum DealStatus { get; set; }
    }
}
