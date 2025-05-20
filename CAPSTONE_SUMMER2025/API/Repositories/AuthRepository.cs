using API.DTO.AccountDTO;
using API.DTO.AuthDTO;
using API.Repositories.Interfaces;
using AutoMapper;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using API.Utils.Constants;
using API.Service;
using Org.BouncyCastle.Ocsp;
namespace API.Repositories
{
    public class AuthRepository : IAuthRepository
    {

        private readonly IMapper _mapper;
        private readonly CAPSTONE_SUMMER2025Context _context;
        private IConfiguration _configuration;
        private readonly IEmailService _email;
        public AuthRepository(IMapper mapper, CAPSTONE_SUMMER2025Context context, IConfiguration configuration, IEmailService email)
        {
            _mapper = mapper;
            _context = context;
            _configuration = configuration;
            _email = email;
        }

        public async Task<Account> GetUserById(int userId)
        {
            var account = await _context.Accounts
                                        .Include(a => a.AccountProfile)
                                        .FirstOrDefaultAsync(x => x.AccountId == userId);
            if (account == null)
                return null;

            return account;
        }

        public async Task<Account> Login(LoginDTO loginDto)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == loginDto.Email);
            if (account == null)
            {
                return null; 
            }
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(loginDto.Password, account.Password);
            if (!isPasswordCorrect)
            {            
                return null; 
            }
            return account;
        }


        public async Task<Account> Register(ReqAccountDTO req)
        {
         
            if (await _context.Accounts.AnyAsync(a => a.Email == req.Email))
                throw new Exception("Email đã được sử dụng.");

           
            if (req.Password != req.ConfirmPassword)
                throw new Exception("Mật khẩu xác nhận không khớp.");

            
            var account = _mapper.Map<Account>(req);
            account.Password = BCrypt.Net.BCrypt.HashPassword(req.Password);

            //  Tạo profile liên kết với account
            var profile = new AccountProfile
            {
                FirstName = req.FirstName,
                LastName = req.LastName,
                Dob = req.DOB,
                Address = req.Address,
                Account = account
            };
            //  Tạo BIO liên kết với account
            var bio = new Bio
            {
                Account = account,
                IsPublicProfile = true 
                                      
            };
            account.Bio = bio;  
            account.AccountProfile = profile;

            try
            {
              
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();
                // 6. Tạo OTP
                var plainOtp = GenerateOtp();
                var hashedOtp = BCrypt.Net.BCrypt.HashPassword(plainOtp);
                var otpExpiryTime = DateTime.UtcNow.AddMinutes(10);

                var userOtp = new UserOtp
                {
                    AccountId = account.AccountId,
                    OtpCode = hashedOtp,
                    ExpiresAt = otpExpiryTime,
                    IsUsed = false
                };
                _context.UserOtps.Add(userOtp);
                await _context.SaveChangesAsync();

                //  Gửi email OTP
                var subject = _configuration["EmailTemplates:VerificationSubject"];
                var htmlTemplate = _configuration["EmailTemplates:VerificationHtmlBody"];

                if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(htmlTemplate))
                {
                    Console.WriteLine("Thiếu cấu hình EmailTemplates. Không thể gửi email OTP.");
                }
                else
                {
                    var htmlContent = htmlTemplate
                        .Replace("{FirstName}", profile.FirstName ?? "bạn")
                        .Replace("{OTP}", plainOtp); 

                    try
                    {
                        await _email.SendEmailAsync(account.Email, subject, htmlContent);
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($"Lỗi khi gửi email OTP đến {account.Email}: {emailEx.Message}");
                    }
                }

                return account;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tạo tài khoản hoặc gửi OTP: {ex.Message}");
                throw; 
            }
        }


        public Task<UserStringHandle> RegisterWithGoogle(string email, string fullName, string avatar)
        {
            throw new NotImplementedException();
        }
        public async Task UpdateStatusAccountAsync(ReqOtpDTO reqOtp)
        {
           

 
            var account = await _context.Accounts
                                       .FirstOrDefaultAsync(a => a.Email == reqOtp.Email);

            if (account == null)
            {
                throw new Exception("Tài khoản không tồn tại."); 
            }
            try
            {            
                account.Status= AccountStatusConst.UNVERIFIED;
                await _context.SaveChangesAsync();              
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu thay đổi xác nhận email cho User ID ");
                throw new Exception($"Lỗi xảy ra khi cập nhật trạng thái tài khoản .", ex);
            }
        }
        public async Task UpdateAccountRefreshTokenAsync(int  userId, string newRefreshToken, DateTime refreshTokenExpiryTime)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == userId); // Giả sử bạn có DbContext là _context và bảng Accounts
            if (account != null)
            {
                account.RefreshToken = newRefreshToken; 
                account.RefreshTokenExpiry = refreshTokenExpiryTime; // Cột để lưu thời gian hết hạn
                await _context.SaveChangesAsync();
            }
          
        }
        public async Task<UserOtp> GetActiveUserOtpAsync(int accountId)
        {
            return await _context.UserOtps
                                 .Where(o => o.AccountId == accountId &&
                                             o.ExpiresAt > DateTime.UtcNow)
                                 .OrderByDescending(o => o.UserOtpId) 
                                 .FirstOrDefaultAsync();
        }
        private string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 1000000).ToString();
        }
    }
}
