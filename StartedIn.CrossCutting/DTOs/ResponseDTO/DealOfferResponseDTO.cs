﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class DealOfferResponseDTO : IdentityResponseDTO
    {
        public string ProjectName { get; set; }
        public string InvestorName { get; set; }
        public string Amount { get; set; }
        public string EquityShareOffer { get; set; }
        public string TermCondition { get; set; }
        public string DealStatus { get; set; }
    }
}