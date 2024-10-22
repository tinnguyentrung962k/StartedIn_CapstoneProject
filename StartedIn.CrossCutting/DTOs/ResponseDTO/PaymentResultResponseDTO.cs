using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class PaymentResultResponseDTO
    {
        public string Code { get; set; }
        public string Desc { get; set; }
        public PayOsCreatePaymentResult CreatedPaymentResult {get;set;}
        public string? Signature { get; set; }
    }
}
