using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.DTO;
using API.DTO.AccountDTO;
using API.DTO.ProfileDTO;
using API.DTO.BioDTO;
namespace API.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<List<Account>> GetAllAccountAsync();
        Task<Account> GetAccountByEmailAsync(string email);
        Task<Account> GetAccountByAccountIDAsync(int accountId);
        Task<List<Account>> GetFollowingAsync(int accountId);
        Task<List<Account>> GetFollowersAsync(int accountId);
        Task<Account> GetAccountWithProfileByIdAsync(int accountId);
        Task<Account> GetAccountWithBioByIdAsync(int accountId);
        Task<Account> GetAccountByIdAsync(int accountId);
        Task<bool> IsBlockedAsync(int blockerAccountId, int blockedAccountId);
        Task SaveChangesAsync();
        Task<bool> FollowAsync(int followerAccountId, int followingAccountId);
        Task<bool> UnfollowAsync(int followerAccountId, int followingAccountId);
        Task<bool> IsFollowingAsync(int followerAccountId, int followingAccountId);
        IQueryable<AccountSearchResultDTO> GetSearchAccounts(string keyword);

    }
}
