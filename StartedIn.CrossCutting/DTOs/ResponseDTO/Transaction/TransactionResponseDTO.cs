using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Transaction
{
    public class TransactionResponseDTO : IdentityResponseDTO
    {
        public string ProjectId { get; set; }
        public string Amount { get; set; }
        public string Budget { get; set; }
        public string FromID { get; set; }
        public string FromUserName { get; set; }
        public string ToID { get; set; }
        public string ToUserName { get; set; }
        public TransactionType Type { get; set; }
        public string Content { get; set; }
        public string EvidenceURl { get; set; }
        public bool IsInFlow { get; set; }
        public string? DisbursementId { get; set; }
        public string? AssetId { get; set; }

    }
}
