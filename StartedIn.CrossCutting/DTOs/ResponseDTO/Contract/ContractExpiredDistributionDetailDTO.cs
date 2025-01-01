using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Contract
{
    public class ContractExpiredDistributionDetailDTO : IdentityResponseDTO
    {
        public decimal TotalProfitOfProject { get; set; }
        public List<ShareHolderDetailInExpiredContractDTO> ShareHolderDetails { get; set; }
    }

    public class ShareHolderDetailInExpiredContractDTO 
    {
        public string HolderId { get; set; }
        public string HolderName { get; set; }
        public decimal EquityDeposit { get; set; }
        public decimal EquityActual { get; set; }
        public decimal ReceivedMoneyDeposit { get; set; }
        public decimal ReceivedMoneyActual { get; set; }
        public decimal? DisbursedAmount { get; set; }
    }
}
