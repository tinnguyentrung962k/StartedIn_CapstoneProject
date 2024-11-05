using StartedIn.CrossCutting.DTOs.Email;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IEmailService
    {
        Task SendInvitationToProjectAsync(string receiveEmail, string projectId, string senderName, string projectName, RoleInTeam roleInTeam);
        Task SendMailAsync(SendEmailModel model);
        Task SendVerificationMailAsync(string receiveEmail, string id);
        Task SendAccountInfoMailAsync(string receiveEmail, string password);
        Task SendResetPasswordEmail(string receiveEmail, string resetLink);
        Task SendingSigningContractRequest(User user, string contractLink);
    }
}
