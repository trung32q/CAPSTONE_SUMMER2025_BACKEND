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
        Task<bool> CheckEmailExistsAsync(string email);
        Task AddAccountAsync(Account account);
        Task SaveOtpAsync(UserOtp otp);
        Task<Account> GetAccountByEmailAsync(string email);
        Task<Account> GetAccountByIdAsync(int accountId);
        Task<UserOtp> GetActiveUserOtpAsync(int accountId);
        Task SaveChangesAsync();


    }
}
