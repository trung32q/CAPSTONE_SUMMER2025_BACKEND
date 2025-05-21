using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Repositories;
using API.Repositories.Interfaces;
using API.DTO.AccountDTO;
using AutoMapper;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using API.DTO.ProfileDTO;
using API.DTO.BioDTO;
namespace Infrastructure.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IMapper _mapper;
        private readonly CAPSTONE_SUMMER2025Context _context;

        public AccountRepository(IMapper mapper, CAPSTONE_SUMMER2025Context context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Account> GetAccountByEmailAsync(string email)
        {
            var account = await _context.Accounts
                                         .Include(a => a.AccountProfile) 
                                         .FirstOrDefaultAsync(x => x.Email == email);
            if (account == null)
                return null;

            return account;   
        }


        public async Task<List<Account>> GetAllAccountAsync()
        {
            var accounts = await _context.Accounts.Include(a => a.AccountProfile).ToListAsync();
            return accounts;
        }

        public async Task<Account> GetAccountByAccountIDAsync(int accountId)
        {
            return await _context.Accounts
                                       .Include(a => a.AccountProfile)
                                       .Include(a => a.Bio)
                                       .Include(a => a.Posts)
                                       .Include(a => a.FollowFollowerAccounts)
                                       .Include(a => a.FollowFollowingAccounts)
                                       .FirstOrDefaultAsync(x => x.AccountId == accountId);           
        }


        public async Task<List<Account>> GetFollowingAsync(int accountId)
        {
           return await _context.Follows
                                         .Where(f => f.FollowerAccountId == accountId)
                                         .Join(_context.Accounts,
                                               f => f.FollowingAccountId,
                                               a => a.AccountId,
                                               (f, a) => a)
                                         .Include(a => a.AccountProfile)
                                         .Include(a => a.Bio)
                                         .Include(a => a.Posts)
                                         .Include(a => a.FollowFollowerAccounts)
                                         .Include(a => a.FollowFollowingAccounts)
                                         .ToListAsync();
          
        }

        public async Task<List<Account>> GetFollowersAsync(int accountId)
        {
            return await _context.Follows
                                         .Where(f => f.FollowingAccountId == accountId)
                                         .Join(_context.Accounts,
                                               f => f.FollowerAccountId,
                                               a => a.AccountId,
                                               (f, a) => a)
                                         .Include(a => a.AccountProfile)
                                         .Include(a => a.Bio)
                                         .Include(a => a.Posts)
                                         .Include(a => a.FollowFollowerAccounts)
                                         .Include(a => a.FollowFollowingAccounts)
                                         .ToListAsync();
        }
        public async Task<Account> GetAccountWithProfileByIdAsync(int accountId)
        {
            return await _context.Accounts
                                 .Include(a => a.AccountProfile)
                                 .Include(a => a.Bio)
                                 .Include(a => a.Posts)
                                 .Include(a => a.FollowFollowerAccounts)
                                 .Include(a => a.FollowFollowingAccounts)
                                 .FirstOrDefaultAsync(x => x.AccountId == accountId);
        }
        public async Task<Account> GetAccountWithBioByIdAsync(int accountId)
        {
            return await _context.Accounts
                                 .Include(a => a.AccountProfile)
                                 .Include(a => a.Bio)
                                 .Include(a => a.Posts)
                                 .Include(a => a.FollowFollowerAccounts)
                                 .Include(a => a.FollowFollowingAccounts)
                                 .FirstOrDefaultAsync(x => x.AccountId == accountId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }    

        public async Task<ResAccountInfoDTO> UpdateBioAsync(int accountId, ReqUpdateBioDTO updateBioDTO)
        {
            var account = await _context.Accounts
                                       .Include(a => a.AccountProfile)
                                       .Include(a => a.Bio)
                                       .Include(a => a.Posts)
                                       .Include(a => a.FollowFollowerAccounts)
                                       .Include(a => a.FollowFollowingAccounts)
                                       .FirstOrDefaultAsync(x => x.AccountId == accountId);
            if (account == null || account.AccountProfile == null)
                return null;
            if (updateBioDTO.IntroTitle != null) account.Bio.IntroTitle = updateBioDTO.IntroTitle;
            if (updateBioDTO.Position != null) account.Bio.Position = updateBioDTO.Position;
            if (updateBioDTO.Workplace != null) account.Bio.Workplace = updateBioDTO.Workplace;
            if (updateBioDTO.FacebookUrl != null) account.Bio.FacebookUrl = updateBioDTO.FacebookUrl;
            if (updateBioDTO.LinkedinUrl != null) account.Bio.LinkedinUrl = updateBioDTO.LinkedinUrl;
            if (updateBioDTO.GithubUrl != null) account.Bio.GithubUrl = updateBioDTO.GithubUrl;
            if (updateBioDTO.PortfolioUrl != null) account.Bio.PortfolioUrl = updateBioDTO.PortfolioUrl;
            if (updateBioDTO.Country != null) account.Bio.Country = updateBioDTO.Country;
            await _context.SaveChangesAsync();
            return _mapper.Map<ResAccountInfoDTO>(account);
        }

        public async Task<bool> ChangePasswordAsync(int accountId, ChangePasswordDTO changePasswordDTO)
        {
            var account = await _context.Accounts
                                        .FirstOrDefaultAsync(x => x.AccountId == accountId);

            if (account == null)
            {            
                return false;
            }
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDTO.OldPassword, account.Password))
            {               
                return false;
            }
            if (changePasswordDTO.NewPassword != changePasswordDTO.ConfirmPassword)
            {
                return false;
            }
            account.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDTO.NewPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Account> GetAccountByIdAsync(int accountId)
        {
            return await _context.Accounts
                       .FirstOrDefaultAsync(x => x.AccountId == accountId);
        }
    }
}
