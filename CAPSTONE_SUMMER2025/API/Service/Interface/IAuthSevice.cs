using API.DTO.AccountDTO;
using API.DTO.AuthDTO;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface IAuthSevice
    {
        Task<Account> RegisterAsync(ReqAccountDTO req);
        Task<Account> LoginAsync(LoginDTO loginDto);
        Task UpdateStatusAccountAsync(ReqOtpDTO reqOtp);
        Task UpdateAccountRefreshTokenAsync(int userId, string newRefreshToken, DateTime refreshTokenExpiryTime);
        Task<UserOtp> GetActiveUserOtpAsync(int accountId);
        Task<UserOtp> SendOTP(ReqSendOTPDTO dto);
    }
}
