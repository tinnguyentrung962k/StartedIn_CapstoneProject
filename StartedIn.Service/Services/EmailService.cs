using Microsoft.Extensions.Configuration;
using StartedIn.CrossCutting.DTOs.Email;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendInvitationToTeamAsync(string receiveEmail, string projectId)
        {
            var webDomain = _configuration.GetValue<string>("WEB_DOMAIN") ?? _configuration["Local_domain"];
            var subject = "Lời mời tham gia nhóm";
            var body = $"Bạn vui lòng bấm vào đường link sau để tham gia vào nhóm:\n{webDomain}/invite/{projectId} \n\n Xin chân thành cảm ơn vì đã đồng hành cùng StartedIn!";

            await SendEmailAsync(receiveEmail, subject, body);
        }

        public async Task SendMailAsync(SendEmailModel model)
        {
            var subject = string.IsNullOrEmpty(model.Subject) ? "No Subject" : model.Subject;
            await SendEmailAsync(model.ReceiveAddress, subject, model.Content);
        }

        public async Task SendVerificationMailAsync(string receiveEmail, string id)
        {
            var appDomain = _configuration.GetValue<string>("API_DOMAIN") ?? _configuration["Api_domain"];
            var subject = "Kích hoạt tài khoản";
            var body = $"Bạn vui lòng bấm vào đường link sau để kích hoạt tài khoản StartedIn:\n{appDomain}/api/activate-user/{id} \n\n Xin chân thành cảm ơn vì đã đồng hành cùng StartedIn!";

            await SendEmailAsync(receiveEmail, subject, body);
        }

        public async Task SendAccountInfoMailAsync(string receiveEmail, string password)
        {
            var subject = "Thông tin tài khoản StartedIn";
            var body = $"Bạn đã được cấp tài khoản StartedIn với thông tin sau:\n Tên đăng nhập (Email): {receiveEmail} \n Mật khẩu: {password} \n\n Xin chân thành cảm ơn vì đã đồng hành cùng StartedIn!";

            await SendEmailAsync(receiveEmail, subject, body);
        }

        private async Task SendEmailAsync(string recipient, string subject, string body)
        {
            try
            {
                var mailMessage = CreateMailMessage(recipient, subject, body);

                using (var smtpClient = CreateSmtpClient())
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it accordingly
                throw new Exception("Email sending failed.", ex);
            }
        }

        private MailMessage CreateMailMessage(string recipient, string subject, string body)
        {
            var mailMessage = new MailMessage
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false,
                From = new MailAddress(EmailSettingModel.Instance.FromEmailAddress, EmailSettingModel.Instance.FromDisplayName)
            };

            mailMessage.To.Add(recipient);
            return mailMessage;
        }

        private SmtpClient CreateSmtpClient()
        {
            var smtpClient = new SmtpClient
            {
                EnableSsl = EmailSettingModel.Instance.Smtp.EnableSsl,
                Host = EmailSettingModel.Instance.Smtp.Host,
                Port = EmailSettingModel.Instance.Smtp.Port,
                Credentials = new NetworkCredential(EmailSettingModel.Instance.Smtp.EmailAddress, EmailSettingModel.Instance.Smtp.Password)
            };

            return smtpClient;
        }

        public async Task SendResetPasswordEmail(string receiveEmail, string resetLink)
        {
            var subject = "Đặt lại mật khẩu";
            var body = $"Bạn vui lòng bấm vào đường link sau để đặt lại mật khẩu của bạn:\n{resetLink}\n\n Xin chân thành cảm ơn vì đã đồng hành cùng StartedIn!";

            await SendEmailAsync(receiveEmail, subject, body);
        }
    }
}
