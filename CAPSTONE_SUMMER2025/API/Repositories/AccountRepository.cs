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
using Application.DTO.AccountDTO;
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


        public async Task<List<ResAccountDTO>> GetAllAccountAsync()
        {
            var accounts = await _context.Accounts.Include(a => a.AccountProfile).ToListAsync();
            return _mapper.Map<List<ResAccountDTO>>(accounts);
        }

        public async Task<ResAccountInfoDTO> GetAccountByAccountIDAsync(int accountId)
        {
            var account = await _context.Accounts
                                       .Include(a => a.AccountProfile)
                                       .Include(a => a.Bio)
                                       .Include(a => a.Posts)
                                       .Include(a => a.FollowFollowerAccounts)
                                       .Include(a => a.FollowFollowingAccounts)
                                       .FirstOrDefaultAsync(x => x.AccountId == accountId);
            if (account == null) return null;
            return _mapper.Map<ResAccountInfoDTO>(account);
        }


        public async Task<List<ResAccountInfoDTO>> GetFollowingAsync(int accountId)
        {
            var following = await _context.Follows
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
            return _mapper.Map<List<ResAccountInfoDTO>>(following);
        }

        public async Task<List<ResAccountInfoDTO>> GetFollowersAsync(int accountId)
        {
            var followers = await _context.Follows
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
            return _mapper.Map<List<ResAccountInfoDTO>>(followers);
        }

        public async Task<ResAccountInfoDTO> UpdateProfileAsync(int accountId, ReqUpdateProfileDTO updateProfileDTO)
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
            if (updateProfileDTO.FirstName != null) account.AccountProfile.FirstName = updateProfileDTO.FirstName;
            if (updateProfileDTO.LastName != null) account.AccountProfile.LastName = updateProfileDTO.LastName;
            if (updateProfileDTO.Gender != null) account.AccountProfile.Gender = updateProfileDTO.Gender;
            if (updateProfileDTO.Dob.HasValue) account.AccountProfile.Dob = updateProfileDTO.Dob;
            if (updateProfileDTO.Address != null) account.AccountProfile.Address = updateProfileDTO.Address;
            if (updateProfileDTO.PhoneNumber != null) account.AccountProfile.PhoneNumber = updateProfileDTO.PhoneNumber;
            if (updateProfileDTO.AvatarUrl != null) account.AccountProfile.AvatarUrl = updateProfileDTO.AvatarUrl;
            await _context.SaveChangesAsync();
            return _mapper.Map<ResAccountInfoDTO>(account);
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
                return false;
            if (account.Password != changePasswordDTO.OldPassword)
                return false;
            if (changePasswordDTO.NewPassword != changePasswordDTO.ConfirmPassword)
                return false;
            account.Password = changePasswordDTO.NewPassword;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<ResSuggestionsAccountDTO>> GetSuggestionsAccountAsync(string position)
        {
            var accounts = await _context.Accounts
                .Include(a => a.AccountProfile)
                .Include(a => a.Bio)
                .Where(a => a.Bio != null && a.Bio.Position == position)
                .ToListAsync();
            return _mapper.Map<List<ResSuggestionsAccountDTO>>(accounts);
        }
    }
}
