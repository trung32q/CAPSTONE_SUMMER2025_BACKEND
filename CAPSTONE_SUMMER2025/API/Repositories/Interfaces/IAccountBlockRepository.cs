using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Models;

namespace Infrastructure.Repositories
{
    public interface IAccountBlockRepository
    {
        Task<AccountBlock> BlockAccountAsync(AccountBlock block);
        Task<bool> UnblockAccountAsync(int blockerAccountId, int blockedAccountId);
        Task<bool> IsBlockedAsync(int blockerAccountId, int blockedAccountId);
        Task<IEnumerable<AccountBlock>> GetBlockedAccountsAsync(int accountId);
        Task<IEnumerable<AccountBlock>> GetBlockedByAccountsAsync(int accountId);
    }
} 