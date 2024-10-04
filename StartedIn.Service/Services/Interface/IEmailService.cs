using StartedIn.CrossCutting.DTOs.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IEmailService
    {
        void SendMail(SendEmailModel model);
        void SendVerificationMail(string receiveEmail, string id);
        void SendInvitationToTeam(string receiveEmail, string teamId);
    }
}
