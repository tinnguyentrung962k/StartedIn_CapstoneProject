using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Project
{
    public class PayOsPaymentGatewayResponseDTO
    {
        public string ProjectId { get; set; }
        public string? ClientKey { get; set; }
        public string? ApiKey { get; set; }
        public string? ChecksumKey { get; set; }
    }
}
