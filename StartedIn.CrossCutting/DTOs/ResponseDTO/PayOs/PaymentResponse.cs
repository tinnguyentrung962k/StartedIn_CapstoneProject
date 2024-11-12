using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.PayOs
{
    public class PaymentResponse
    {
        public string? Status { get; set; }
        public string? Code { get; set; }
        public string? Id { get; set; }
        public bool Cancel { get; set; }
        public long OrderCode { get; set; }
    }
}
