using StartedIn.CrossCutting.DTOs.RequestDTO.Disbursement;
using System.ComponentModel.DataAnnotations;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Contract
{
    public class InvestmentContractFromDealCreateDTO
    {
        [Required]
        public string DealId { get; set; }
        public ContractCreateDTO Contract { get; set; }
    }
}
