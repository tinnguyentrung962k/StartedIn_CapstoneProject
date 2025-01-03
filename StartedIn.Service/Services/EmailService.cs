﻿using Microsoft.Extensions.Configuration;
using StartedIn.CrossCutting.DTOs.Email;
using StartedIn.Service.Services.Interface;
using System.Net.Mail;
using System.Net;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.Service.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendInvitationToProjectAsync(string receiveEmail, string projectId, string senderName, string projectName, RoleInTeam roleInTeam)
        {
            var webDomain = _configuration.GetValue<string>("WEB_DOMAIN") ?? _configuration["Local_domain"];
            var subject = "Lời mời tham gia nhóm";
            var body = $"{senderName} đã gửi lời mời tham gia dự án {projectName} cho bạn \n\n Bạn vui lòng bấm vào đường link sau để tham gia vào dự án:\n{webDomain}/invite/{projectId}/{roleInTeam} \n\n Xin chân thành cảm ơn vì đã đồng hành cùng StartedIn!";
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

        public async Task SendDisbursementReminder(string receiveEmail, DateOnly disbursementEndDate, string projectName, string disbursementTitle, decimal disbursementAmount)
        {
            var subject = "Nhắc nhở về việc giải ngân";
            var body = $"Bạn có đợt giải ngân kết vào ngày {disbursementEndDate} với dự án {projectName}: \n\t Tên đợt giải ngân: {disbursementTitle} \n\t Số tiền giải ngân: {disbursementAmount}";
            await SendEmailAsync(receiveEmail, subject, body);
        }

        public async Task SendDisbursementStartReminder(string receiveEmail, DateOnly disbursementStartDate, string projectName,
            string disbursementTitle, decimal disbursementAmount)
        {
            var subject = "Nhắc nhở về việc giải ngân";
            var body = $"Đợt giải ngân với dự án {projectName} đã bắt đầu vào ngày {disbursementStartDate}: \n\t Tên đợt giải ngân: {disbursementTitle} \n\t Số tiền giải ngân: {disbursementAmount}";
            await SendEmailAsync(receiveEmail, subject, body);
        }

        public async Task SendClosingProject(string receiveEmail, string senderName, string receiverName, string projectName)
        {
            var subject = "Thông báo kết thúc dự án";
            var body = $"Kính gửi {receiverName} \n{senderName} gửi cho bạn thông báo rằng dự án {projectName} đã kết thúc. \nXin chân thành cảm ơn bạn đã đồng hành cùng dự án {projectName}. \nTrân trọng.";
            await SendEmailAsync(receiveEmail, subject, body);
        }

        public async Task SendAppointmentInvite(string receiveEmail, string projectName, string receiverName, string meetingLink, DateTimeOffset appointmentTime)
        {
            // Format the appointment time
            var formattedAppointmentTime = appointmentTime.DateTime.ToString("dd/MM/yyyy HH:mm");

            // Subject of the email
            var subject = "Thông báo về cuộc họp ngày: " + appointmentTime.Date.ToString("dd/MM/yyyy");

            // Body of the email
            var body = $"Kính gửi {receiverName},\n{projectName} gửi cho bạn thông báo về cuộc họp vào ngày: {formattedAppointmentTime}\nBạn hãy truy cập vào đường link sau để tham gia cuộc họp: {meetingLink}";

            // Send the email
            await SendEmailAsync(receiveEmail, subject, body);
        }
    }
}
