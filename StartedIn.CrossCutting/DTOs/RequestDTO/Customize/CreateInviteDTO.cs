using SignNow.Net.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Customize
{
    public class CreateInviteDTO
    {
        public string DocumentId { get; set; }
        public SignInvite Invite { get; set; }
        public List<string> Emails { get; set; }
    }
}
