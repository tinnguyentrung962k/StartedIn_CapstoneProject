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
        public decimal Equity { get; set; }
        public decimal ReceivedMoney { get; set; }
        public decimal? DisbursedAmount { get; set; }
    }
}
