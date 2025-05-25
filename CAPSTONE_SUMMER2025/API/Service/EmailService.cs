using System.Management;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System.Net;
using API.Service.Interface;
using Infrastructure.Models;
using API.DTO.AuthDTO;
using API.Repositories.Interfaces;
using System.Security.Principal;

namespace API.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IAccountRepository _accountRepository;      
        private readonly IAuthRepository _authRepository;
        public EmailService(IConfiguration config, IAccountRepository accountRepository,IAuthRepository authRepository)
        {
            _config = config;
            _accountRepository = accountRepository;
            _authRepository = authRepository;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlContent)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            var smtpClient = new System.Net.Mail.SmtpClient(emailSettings["SmtpServer"])
            {
                Port = int.Parse(emailSettings["Port"]),
                Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"]),
                EnableSsl = true,
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailSettings["SenderEmail"], emailSettings["SenderName"]),
                Subject = subject,
                Body = htmlContent,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task<UserOtp> SendOTP(ReqSendOTPDTO dto)
        {
            var Account = await _accountRepository.GetAccountByEmailAsync(dto.Email);

            var plainOtp = GenerateOtp();
            var hashedOtp = BCrypt.Net.BCrypt.HashPassword(plainOtp);

            var otp = new UserOtp
            {
                AccountId = Account.AccountId,
                OtpCode = hashedOtp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };
            await _authRepository.SaveOtpAsync(otp);
            // Gửi OTP qua email
            var subject = _config["EmailTemplates:VerificationSubject"];
            var htmlTemplate = _config["EmailTemplates:VerificationHtmlBody"];

            if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(htmlTemplate))
            {
                var htmlContent = htmlTemplate
                    .Replace("{FirstName}", Account.AccountProfile.FirstName ?? "bạn")
                    .Replace("{OTP}", plainOtp);

                await SendEmailAsync(Account.Email, subject, htmlContent);
            }
            return otp;
        }
        private string GenerateOtp()
        {
            return new Random().Next(100000, 1000000).ToString();
        }
    }
}
