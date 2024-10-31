using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.SignNowWebhookRequestDTO
{
    public class RegisterContractWebhookDTO
    {
        public string ContractId { get; set; }
        public string CallBack { get; set; }
    }
}
