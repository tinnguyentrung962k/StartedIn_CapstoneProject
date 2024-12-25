using StartedIn.CrossCutting.DTOs.Email;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.Service.Services.Interface
{
    public interface IEmailService
    {
        Task SendInvitationToProjectAsync(string receiveEmail, string projectId, string senderName, string projectName, RoleInTeam roleInTeam);
        Task SendMailAsync(SendEmailModel model);
        Task SendVerificationMailAsync(string receiveEmail, string id);
        Task SendAccountInfoMailAsync(string receiveEmail, string password);
        Task SendResetPasswordEmail(string receiveEmail, string resetLink);
        Task SendDisbursementReminder(string receiveEmail, DateOnly disbursementEndDate, string projectName, string disbursementTitle, decimal disbursementAmount);
        Task SendDisbursementStartReminder(string receiveEmail, DateOnly disbursementStartDate, string projectName, string disbursementTitle, decimal disbursementAmount);
        Task SendClosingProject(string receiveEmail, string senderName, string receiverName, string projectName);
        Task SendAppointmentInvite(string receiveEmail, string projectName, string receiverName, string meetingLink, DateTimeOffset appointmentTime);
    }
}
