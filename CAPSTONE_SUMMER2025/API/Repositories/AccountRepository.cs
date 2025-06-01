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
using System.Globalization;
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
        public async Task<bool> IsBlockedAsync(int blockerAccountId, int blockedAccountId)
        {
            return await _context.AccountBlocks
                .AnyAsync(b => b.BlockerAccountId == blockerAccountId &&
                             b.BlockedAccountId == blockedAccountId);
        }
        public async Task<bool> FollowAsync(int followerAccountId, int followingAccountId)
        {
            // Kiểm tra tài khoản tồn tại
            var follower = await _context.Accounts.FindAsync(followerAccountId);
            var following = await _context.Accounts.FindAsync(followingAccountId);
            if (follower == null || following == null)
            {
                return false;
            }

            // Kiểm tra đã follow chưa
            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerAccountId == followerAccountId && f.FollowingAccountId == followingAccountId);
            if (existingFollow != null)
            {
                return true; // Đã follow, không cần thêm
            }

            // Thêm bản ghi follow
            var follow = new Follow
            {
                FollowerAccountId = followerAccountId,
                FollowingAccountId = followingAccountId,
                FollowDate = DateTime.UtcNow
            };

            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfollowAsync(int followerAccountId, int followingAccountId)
        {
            // Tìm bản ghi follow
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerAccountId == followerAccountId && f.FollowingAccountId == followingAccountId);
            if (follow == null)
            {
                return false; // Chưa follow, không thể unfollow
            }

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFollowingAsync(int followerAccountId, int followingAccountId)
        {
            return await _context.Follows
                .AnyAsync(f => f.FollowerAccountId == followerAccountId && f.FollowingAccountId == followingAccountId);
        }

        // hàm tìm kiếm account để ở querry vì ko convert được
        public IQueryable<AccountSearchResultDTO> GetSearchAccounts(string keyword)
        {
            keyword = RemoveDiacritics(keyword).ToLower();

            // Chuẩn bị truy vấn base
            var query = _context.Accounts
                .Include(a => a.AccountProfile)
                .Include(a => a.Bio)
                .Select(a => new AccountSearchResultDTO
                {
                    AccountId = a.AccountId,
                    FullName = a.AccountProfile.FirstName + " " + a.AccountProfile.LastName,
                    AvatarUrl = a.AccountProfile.AvatarUrl,
                    Position = a.Bio != null ? a.Bio.Position : null,
                    Workplace = a.Bio != null ? a.Bio.Workplace : null,
                    FollowerCount = _context.Follows.Count(f => f.FollowingAccountId == a.AccountId)
                })
                .AsEnumerable() // xử lý RemoveDiacritics ở client
                .Where(dto =>
                    RemoveDiacritics(dto.FullName).ToLower().Contains(keyword) ||
                    RemoveDiacritics(dto.Position ?? "").ToLower().Contains(keyword) ||
                    RemoveDiacritics(dto.Workplace ?? "").ToLower().Contains(keyword)
                )
                .OrderByDescending(dto => dto.FollowerCount) // ưu tiên người có follower nhiều
                .AsQueryable();

            return query;
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var normalized = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    builder.Append(c);
            }
            return builder.ToString().Normalize(NormalizationForm.FormC);
        }



    }
}
