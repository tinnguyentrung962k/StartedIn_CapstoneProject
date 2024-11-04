using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.SignNowWebhookRequestDTO
{
    public class SignNowWebhookCreateDTO
    {
        public string EntityId { get; set; }
        public string Event { get; set; }
        public string Action { get; set; }
        public string CallBackUrl { get; set; }

    }
}
