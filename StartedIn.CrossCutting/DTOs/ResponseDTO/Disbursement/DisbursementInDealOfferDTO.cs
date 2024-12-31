using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement
{
    public class DisbursementInDealOfferDTO : IdentityResponseDTO
    {
        public string Title { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal Amount { get; set; }
        public string Condition { get; set; }
    }
}
