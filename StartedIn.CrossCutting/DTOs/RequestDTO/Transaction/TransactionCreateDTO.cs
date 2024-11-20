using Microsoft.AspNetCore.Http;
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
        
        [Required]
        public decimal Budget { get; set; }

        [Required]
        public string ToInvestorID { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public bool IsInFlow { get; set; }
        public string Content { get; set; }
        
        [Required]
        public IFormFile EvidenceFile { get; set; }

    }
}
