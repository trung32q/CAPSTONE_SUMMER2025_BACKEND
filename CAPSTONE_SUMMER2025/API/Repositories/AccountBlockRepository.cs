using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AccountBlockRepository : IAccountBlockRepository
    {
        private readonly CAPSTONE_SUMMER2025Context _context;

        public AccountBlockRepository(CAPSTONE_SUMMER2025Context context)
        {
            _context = context;
        }

        public async Task<AccountBlock> BlockAccountAsync(AccountBlock block)
        {
            block.BlockedAt = DateTime.UtcNow;
            await _context.AccountBlocks.AddAsync(block);
            await _context.SaveChangesAsync();
            return block;
        }

        public async Task<bool> UnblockAccountAsync(int blockerAccountId, int blockedAccountId)
        {
            var block = await _context.AccountBlocks
                .FirstOrDefaultAsync(b => b.BlockerAccountId == blockerAccountId && 
                                        b.BlockedAccountId == blockedAccountId);

            if (block == null)
                return false;

            _context.AccountBlocks.Remove(block);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsBlockedAsync(int blockerAccountId, int blockedAccountId)
        {
            return await _context.AccountBlocks
                .AnyAsync(b => b.BlockerAccountId == blockerAccountId && 
                             b.BlockedAccountId == blockedAccountId);
        }

        public async Task<IEnumerable<AccountBlock>> GetBlockedAccountsAsync(int accountId)
        {
            return await _context.AccountBlocks
                .Include(b => b.BlockedAccount)
                .Where(b => b.BlockerAccountId == accountId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AccountBlock>> GetBlockedByAccountsAsync(int accountId)
        {
            return await _context.AccountBlocks
                .Include(b => b.BlockerAccount)
                .Where(b => b.BlockedAccountId == accountId)
                .ToListAsync();
        }
    }
} 