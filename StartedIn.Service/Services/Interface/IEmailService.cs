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
        Task SendInvitationToTeamAsync(string receiveEmail, string teamId);
        Task SendMailAsync(SendEmailModel model);
        Task SendVerificationMailAsync(string receiveEmail, string id);
        Task SendAccountInfoMailAsync(string receiveEmail, string password);
        Task SendResetPasswordEmail(string receiveEmail, string resetLink);
    }
}
