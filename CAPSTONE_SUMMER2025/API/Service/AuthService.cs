using API.DTO.AccountDTO;
using API.DTO.AuthDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Service.Interface;
using API.Utils.Constants;
using AutoMapper;
using Infrastructure.Models;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace API.Service
{
    public class AuthService : IAuthSevice
    {
        private readonly IAuthRepository _authRepo;
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailService _email;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public AuthService(IAuthRepository authRepo, IEmailService email, IMapper mapper, IConfiguration config, IAccountRepository accountRepository)
        {
            _authRepo = authRepo;
            _email = email;
            _mapper = mapper;
            _config = config;
            _accountRepository = accountRepository;
        }

        public async Task<Account> RegisterAsync(ReqAccountDTO req)
        {
            if (req.Password != req.ConfirmPassword)
                throw new Exception("Mật khẩu xác nhận không khớp.");

            // Kiểm tra email
            var existed = await _authRepo.CheckEmailExistsAsync(req.Email);
            if (existed)
                throw new Exception("Email đã được sử dụng.");

            // Tạo tài khoản
            var account = _mapper.Map<Account>(req);
            account.Password = BCrypt.Net.BCrypt.HashPassword(req.Password);

            var profile = new AccountProfile
            {
                FirstName = req.FirstName,
                LastName = req.LastName,
                Dob = req.DOB,
                Address = req.Address,
                Account = account
            };

            var bio = new Bio
            {
                Account = account,
                IsPublicProfile = true
            };

            account.AccountProfile = profile;
            account.Bio = bio;

            await _authRepo.AddAccountAsync(account);

            // Tạo và lưu OTP
            var plainOtp = GenerateOtp();
            var hashedOtp = BCrypt.Net.BCrypt.HashPassword(plainOtp);
            var otp = new UserOtp
            {
                AccountId = account.AccountId,
                OtpCode = hashedOtp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _authRepo.SaveOtpAsync(otp);

            // Gửi OTP qua email
            var subject = _config["EmailTemplates:VerificationSubject"];
            var htmlTemplate = _config["EmailTemplates:VerificationHtmlBody"];

            if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(htmlTemplate))
            {
                var htmlContent = htmlTemplate
                    .Replace("{FirstName}", profile.FirstName ?? "bạn")
                    .Replace("{OTP}", plainOtp);

                await _email.SendEmailAsync(account.Email, subject, htmlContent);
            }

            return account;
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
            await _authRepo.SaveOtpAsync(otp);
            // Gửi OTP qua email
            var subject = _config["EmailTemplates:VerificationSubject"];
            var htmlTemplate = _config["EmailTemplates:VerificationHtmlBody"];

            if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(htmlTemplate))
            {
                var htmlContent = htmlTemplate
                    .Replace("{FirstName}", Account.AccountProfile.FirstName ?? "bạn")
                    .Replace("{OTP}", plainOtp);

                await _email.SendEmailAsync(Account.Email, subject, htmlContent);
            }
            return otp;
        }
       

        public async Task<Account> LoginAsync(LoginDTO loginDto)
        {
            var account = await _authRepo.GetAccountByEmailAsync(loginDto.Email);
            if (account == null) return null;

            var isMatch = BCrypt.Net.BCrypt.Verify(loginDto.Password, account.Password);
            return isMatch ? account : null;
        }

        public async Task UpdateStatusAccountAsync(ReqOtpDTO reqOtp,string status)
        {
            var account = await _authRepo.GetAccountByEmailAsync(reqOtp.Email);
            if (account == null) throw new Exception("Tài khoản không tồn tại.");

            account.Status = status;
            await _authRepo.SaveChangesAsync();
        }
        public async Task UpdateStatusVerifyAccountAsync(int id, string status)
        {
            var account = await _authRepo.GetAccountByIdAsync(id);
            if (account == null) throw new Exception("Tài khoản không tồn tại.");

            account.Status = status;
            await _authRepo.SaveChangesAsync();
        }
        private string GenerateOtp()
        {
            return new Random().Next(100000, 1000000).ToString();
        }

        public async Task UpdateAccountRefreshTokenAsync(int userId, string newRefreshToken, DateTime refreshTokenExpiryTime)
        {
            var account = await _authRepo.GetAccountByIdAsync(userId);
            if (account != null)
            {
                account.RefreshToken = newRefreshToken;
                account.RefreshTokenExpiry = refreshTokenExpiryTime; // Cột để lưu thời gian hết hạn
                await _authRepo.SaveChangesAsync(); 
            }
        }

        public Task<UserOtp> GetActiveUserOtpAsync(int accountId)
        {
           return _authRepo.GetActiveUserOtpAsync(accountId);
        }
    }
}
