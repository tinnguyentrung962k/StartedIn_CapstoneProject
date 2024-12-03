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
        public int SellAmount { get; set; }
        public string? ToId { get; set; }
        public string? ToName { get; set; }
        public IFormFile EvidenceFile { get; set; }
    }
}
