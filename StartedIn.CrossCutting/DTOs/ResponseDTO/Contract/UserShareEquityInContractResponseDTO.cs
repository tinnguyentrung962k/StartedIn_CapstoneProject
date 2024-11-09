using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Contract
{
    public class UserShareEquityInContractResponseDTO
    {
        public string UserId { get; set; }
        public int? ShareQuantity { get; set; }
        public decimal? Percentage { get; set; }
    }
}
