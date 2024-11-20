using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Asset
{
    public class AssetCreateDTO
    {
        [Required]
        public string AssetName { get; set; }
        public decimal? Price { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public int? Quantity { get; set; }
        public string? SerialNumber { get; set; }
    }
}
