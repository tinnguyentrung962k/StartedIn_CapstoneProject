using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.RequestDTO.Asset;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Transaction
{
    public class TransactionCreateDTO
    {
        [Required]
        public decimal Amount { get; set; }
        public string? FromId { get; set; }
        public string? ToId { get; set; }
        public string? FromName { get; set; }
        public string? ToName { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public bool IsInFlow { get; set; }
        public string Content { get; set; }
        
        public List<AssetCreateDTO>? Assets { get; set; }

    }
}
