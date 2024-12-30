using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Transaction;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Asset
{
    public class AssetResponseDTO : IdentityResponseDTO
    {
        public string ProjectId { get; set; }
        public string AssetName { get; set; }
        public string? Price { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public int? Quantity { get; set; }
        public AssetStatus Status { get; set; }
        public string SerialNumber { get; set; }
        public int? RemainQuantity { get; set; }
        public string? TransactionId { get; set; }
        public List<TransactionDetailInAssetDTO>? Transactions { get; set; }
    }
}
