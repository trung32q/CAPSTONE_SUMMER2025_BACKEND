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
using Amazon.Util;
using System.Reflection.Metadata;
using API.Utils;
using API.Utils.Constants;
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
                return false; // Đã follow, không cần thêm
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
        public IQueryable<AccountSearchResultDTO> GetSearchAccounts(string keyword, int currentUserId)
        {
                keyword = RemoveDiacritics(keyword).ToLower();

                // Lấy danh sách account bị chặn bởi current user
                var blockedIds = _context.AccountBlocks
                .Where(b => b.BlockerAccountId == currentUserId)
                .Select(b => b.BlockedAccountId)
                .ToList();

                // Bước 1: Lọc sơ bộ (có thông tin và không bị chặn)
                var candidates = _context.Accounts
                .Include(a => a.AccountProfile)
                .Include(a => a.Bio)
                .Where(a =>
                    a.AccountProfile != null &&
                    !blockedIds.Contains(a.AccountId) &&
                    (!string.IsNullOrEmpty(a.AccountProfile.FirstName) || !string.IsNullOrEmpty(a.AccountProfile.LastName)))
                    .ToList(); // load về để xử lý RemoveDiacritics

                // Bước 2: Lọc không dấu
                var filtered = candidates
                .Where(a =>
                {
                    var fullName = $"{a.AccountProfile.FirstName} {a.AccountProfile.LastName}";
                    var position = a.Bio?.Position ?? "";
                    var workplace = a.Bio?.Workplace ?? "";

                    return RemoveDiacritics(fullName).ToLower().Contains(keyword)
                        || RemoveDiacritics(position).ToLower().Contains(keyword)
                        || RemoveDiacritics(workplace).ToLower().Contains(keyword);
                });

                // Bước 3: Map sang DTO
                var result = filtered
                .Select(a => new AccountSearchResultDTO
                {
                    AccountId = a.AccountId,
                    FullName = a.AccountProfile.FirstName + " " + a.AccountProfile.LastName,
                    AvatarUrl = a.AccountProfile.AvatarUrl,
                    Position = a.Bio?.Position,
                    Workplace = a.Bio?.Workplace,
                    FollowerCount = _context.Follows.Count(f => f.FollowingAccountId == a.AccountId)
                })
                .OrderByDescending(a => a.FollowerCount)
                .AsQueryable();

            return result;
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

        // hàm đề xuất tài khoản có thể người dùng muốn follow
        public async Task<PagedResult<AccountRecommendDTO>> RecommendAccountsAsync(int currentAccountId, int pageNumber, int pageSize)
        {
            // 1. Kiểm tra tài khoản hợp lệ
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == currentAccountId);
            if (account == null)
                throw new UnauthorizedAccessException("Tài khoản không hợp lệ hoặc đã bị vô hiệu hóa.");

            // 2. Danh sách đã follow hoặc đã block
            var followedIds = await _context.Follows
                .Where(f => f.FollowerAccountId == currentAccountId)
                .Select(f => f.FollowingAccountId)
                .ToListAsync();

            var blockedIds = await _context.AccountBlocks
                .Where(b => b.BlockerAccountId == currentAccountId)
                .Select(b => b.BlockedAccountId)
                .ToListAsync();

            // 3. Các post đã tương tác
            var interactedPostIds = await _context.PostLikes
                .Where(pl => pl.AccountId == currentAccountId)
                .Select(pl => pl.PostId)
                .Union(
                    _context.PostComments
                        .Where(pc => pc.AccountId == currentAccountId)
                        .Select(pc => pc.PostId)
                ).Distinct().ToListAsync();

            // 4. Các account cũng tương tác với post đó (trừ chính mình)
            var relatedAccountIds = await _context.PostLikes
                .Where(pl => interactedPostIds.Contains(pl.PostId) && pl.AccountId != currentAccountId)
                .Select(pl => pl.AccountId)
                .Union(
                    _context.PostComments
                        .Where(pc => interactedPostIds.Contains(pc.PostId) && pc.AccountId != currentAccountId)
                        .Select(pc => pc.AccountId)
                ).Distinct().ToListAsync();

            // 5. Nguồn gợi ý chính: từ tương tác chung
            var fromInteractions = _context.Accounts
                .Where(acc =>
                    relatedAccountIds.Contains(acc.AccountId) &&
                    !followedIds.Contains(acc.AccountId) &&
                    !blockedIds.Contains(acc.AccountId) &&
                    acc.Status == AccountStatusConst.VERIFIED &&
                    acc.Role != RoleConst.ADMIN)
                .Select(acc => new AccountRecommendDTO
                {
                    AccountId = acc.AccountId,
                    FullName = acc.AccountProfile.FirstName + " " + acc.AccountProfile.LastName,
                    AvatarUrl = acc.AccountProfile.AvatarUrl,
                    Position = acc.Bio.Position,
                    TotalFollowers = _context.Follows.Count(f => f.FollowingAccountId == acc.AccountId)
                });

            // 6. Gợi ý bổ sung: các account phổ biến (fallback)
            var popularAccounts = _context.Accounts
                .Where(acc =>
                    acc.AccountId != currentAccountId &&
                    !followedIds.Contains(acc.AccountId) &&
                    !blockedIds.Contains(acc.AccountId) &&
                    acc.Status != AccountStatusConst.BANNED &&
                    acc.Role != RoleConst.ADMIN)
                .Select(acc => new AccountRecommendDTO
                {
                    AccountId = acc.AccountId,
                    FullName = acc.AccountProfile.FirstName + " " + acc.AccountProfile.LastName,
                    AvatarUrl = acc.AccountProfile.AvatarUrl,
                    Position = acc.Bio.Position,
                    TotalFollowers = _context.Follows.Count(f => f.FollowingAccountId == acc.AccountId)
                });

            // 7. Gộp và phân trang
            var finalQuery = fromInteractions
                .Union(popularAccounts)
                .Distinct()
                .OrderByDescending(x => x.TotalFollowers);

            var totalCount = await finalQuery.CountAsync();
            var items = await finalQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AccountRecommendDTO>(items, totalCount, pageNumber, pageSize);
        }
        public async Task<AccountBlock?> GetBlockAsync(int blockerId, int blockedId)
        {
            return await _context.AccountBlocks
                .FirstOrDefaultAsync(x => x.BlockerAccountId == blockerId && x.BlockedAccountId == blockedId);
        }
        public async Task<bool> BlockAccountAsync(int blockerId, int blockedId)
        {
            if (await GetBlockAsync(blockerId, blockedId) != null) return false; // Đã block
            var block = new AccountBlock
            {
                BlockerAccountId = blockerId,
                BlockedAccountId = blockedId,
                BlockedAt = DateTime.UtcNow
            };
            _context.AccountBlocks.Add(block);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UnblockAccountAsync(int blockerId, int blockedId)
        {
            var block = await GetBlockAsync(blockerId, blockedId);
            if (block == null) return false;
            _context.AccountBlocks.Remove(block);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<AccountBlock>> GetBlockedAccountsAsync(int blockerId)
        {
            return await _context.AccountBlocks
     .Include(x => x.BlockedAccount)
     .ThenInclude(acc => acc.AccountProfile) 
     .Where(x => x.BlockerAccountId == blockerId)
     .ToListAsync();
        }
        public async Task<Account> CreateAdminAccountAsync(Account account)
        {
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }
    }
}
