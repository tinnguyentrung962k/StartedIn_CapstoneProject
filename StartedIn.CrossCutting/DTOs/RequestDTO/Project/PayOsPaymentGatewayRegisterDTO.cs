using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Project
{
    public class PayOsPaymentGatewayRegisterDTO
    {
        public string ClientKey { get; set; }
        public string ApiKey { get; set; }
        public string ChecksumKey { get; set; }
    }
}
