using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Transaction
{
    public class TransactionInAssetCreateDTO
    {
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public decimal Budget { get; set; }
        public string ToName { get; set; }
        public string Content { get; set; }
        
        [Required]
        public IFormFile EvidenceFile { get; set; }



    }
}
