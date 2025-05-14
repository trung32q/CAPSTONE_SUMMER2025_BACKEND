using System.Threading.Tasks;
using Infrastructure.Models;
using API.DTO;
using API.DTO.AccountDTO;
using System.Reflection.Metadata;
using API.DTO.AuthDTO;

namespace API.Repositories.Interfaces
{
    public interface IAuthRepository
    {

        Task<Account> Register(ReqAccountDTO reqAccountDto);
        Task<Account> Login(LoginDTO loginDto);
        Task<Account> GetUserById(int userId);
        Task<UserStringHandle> RegisterWithGoogle(string email, string fullName, string avatar);
        Task UpdateStatusAccountAsync(ReqOtpDTO reqOtp);
        Task UpdateAccountRefreshTokenAsync(int userId, string refreshToken, DateTime refreshTokenExpiryTime);
        Task<UserOtp> GetActiveUserOtpAsync(int accountId);
    }
}
