using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Asset;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
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
        public string FromID { get; set; }
        public string FromUserName { get; set; }
        public string? FromUserProfilePicture { get; set; }
        public string ToID { get; set; }
        public string ToUserName { get; set; }
        public string? ToUserProfilePicture { get; set; }
        public TransactionType Type { get; set; }
        public string Content { get; set; }
        public string EvidenceUrl { get; set; }
        public bool IsInFlow { get; set; }
        public string? DisbursementId { get; set; }
        public DateTimeOffset LastUpdatedTime { get; set; }
        public DisbursementDetailInATransactionResponseDTO? Disbursement { get; set; }
        public List<AssetResponseDTO>? Assets { get; set; }

    }
}
