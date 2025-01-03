﻿using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
using StartedIn.CrossCutting.Enum;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.DealOffer
{
    public class DealOfferForProjectResponseDTO : IdentityResponseDTO
    {
        public string InvestorId { get; set; }
        public string InvestorName { get; set; }
        public string Amount { get; set; }
        public string EquityShareOffer { get; set; }
        public string TermCondition { get; set; }
        public DealStatusEnum DealStatus { get; set; }
        public List<DisbursementInDealOfferDTO>? Disbursements { get; set; }
    }
}
