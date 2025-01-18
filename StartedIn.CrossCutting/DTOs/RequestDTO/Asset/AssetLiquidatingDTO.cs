using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Asset
{
    public class AssetLiquidatingDTO
    {
        public decimal SellPrice { get; set; }
        public int SellQuantity { get; set; }
        public string? FromId { get; set; }
        public string? FromName { get; set; }
        public IFormFile EvidenceFile { get; set; }
    }
}
