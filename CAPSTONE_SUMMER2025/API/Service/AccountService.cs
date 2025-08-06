using System.Globalization;
using System.Text;
using API.DTO.AccountDTO;
using API.DTO.AuthDTO;
using API.DTO.BioDTO;
using API.DTO.NotificationDTO;
using API.DTO.ProfileDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Service.Interface;
using API.Utils.Constants;
using AutoMapper;
using CloudinaryDotNet;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Service
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly INotificationService _notificationService;
        private readonly IFilebaseHandler _filebaseHandler;
        private readonly IMapper _mapper;

        public AccountService(IMapper mapper, IAccountRepository accountRepository, IFilebaseHandler filebaseHandler, INotificationService notificationService)
        {
            _mapper = mapper;
            _accountRepository = accountRepository;
            _filebaseHandler = filebaseHandler;
            _notificationService = notificationService;
        }
        public async Task<List<ResAccountDTO>> GetAllAccountAsync()
        {
            var accounts = await _accountRepository.GetAllAccountAsync();
            return _mapper.Map<List<ResAccountDTO>>(accounts);
        }
        public async Task<ResAccountInfoDTO> GetAccountByAccountIDAsync(int accountId)
        {
            var account = await _accountRepository.GetAccountByAccountIDAsync(accountId);
            if (account == null) return null;

            var accountDTO = _mapper.Map<ResAccountInfoDTO>(account);

            // Xử lý URL của avatar nếu có
            if (!string.IsNullOrEmpty(accountDTO.AvatarUrl))
            {
                // Nếu Avatar không phải là URL đầy đủ, gọi GeneratePreSignedUrl để tạo URL
                if (!accountDTO.AvatarUrl.StartsWith("https://"))
                {
                    Console.WriteLine($"Converting Avatar to full URL: {accountDTO.AvatarUrl}");
                    accountDTO.AvatarUrl = _filebaseHandler.GeneratePreSignedUrl(accountDTO.AvatarUrl);
                }
            }

            return accountDTO;
        }

        public async Task<List<ResAccountInfoDTO>> GetFollowingAsync(int accountId)
        {
            var following = await _accountRepository.GetFollowingAsync(accountId);
            if (following == null) return null;
            return _mapper.Map<List<ResAccountInfoDTO>>(following);
        }
        public async Task<List<ResAccountInfoDTO>> GetFollowersAsync(int accountId)
        {
            var followers = await _accountRepository.GetFollowersAsync(accountId);
            if (followers == null) return null;
            return _mapper.Map<List<ResAccountInfoDTO>>(followers);
        }

        public async Task<ResAccountInfoDTO> UpdateProfileAsync(int accountId, ReqUpdateProfileDTO updateProfileDTO)
        {
            var account = await _accountRepository.GetAccountWithProfileByIdAsync(accountId);
            if (account?.AccountProfile == null)
                return null;

            var profile = account.AccountProfile;

            profile.FirstName = string.IsNullOrWhiteSpace(updateProfileDTO.FirstName) ? profile.FirstName : updateProfileDTO.FirstName;
            profile.LastName = string.IsNullOrWhiteSpace(updateProfileDTO.LastName) ? profile.LastName : updateProfileDTO.LastName;
            profile.Gender = string.IsNullOrWhiteSpace(updateProfileDTO.Gender) ? profile.Gender : updateProfileDTO.Gender;
            profile.Dob = updateProfileDTO.Dob ?? profile.Dob;
            profile.Address = string.IsNullOrWhiteSpace(updateProfileDTO.Address) ? profile.Address : updateProfileDTO.Address;
            profile.PhoneNumber = string.IsNullOrWhiteSpace(updateProfileDTO.PhoneNumber) ? profile.PhoneNumber : updateProfileDTO.PhoneNumber;

            if (updateProfileDTO.AvatarUrl != null)
            {
                var fileUrl = await _filebaseHandler.UploadMediaFile(updateProfileDTO.AvatarUrl);
                if (!string.IsNullOrWhiteSpace(fileUrl))
                    profile.AvatarUrl = fileUrl;
            }
            if (updateProfileDTO.BackgroundURL != null)
            {
                var fileUrl = await _filebaseHandler.UploadMediaFile(updateProfileDTO.BackgroundURL);
                if (!string.IsNullOrWhiteSpace(fileUrl))
                    profile.BackgroundUrl = fileUrl;
            }

            await _accountRepository.SaveChangesAsync();
            return _mapper.Map<ResAccountInfoDTO>(account);
        }


        public async Task<ResAccountInfoDTO> UpdateBioAsync(int accountId, ReqUpdateBioDTO updateBioDTO)
        {
            var account = await _accountRepository.GetAccountWithBioByIdAsync(accountId);
            if (account == null || account.Bio == null)
                return null;

            var bio = account.Bio;

            if (!string.IsNullOrWhiteSpace(updateBioDTO.IntroTitle)) bio.IntroTitle = updateBioDTO.IntroTitle;
            if (!string.IsNullOrWhiteSpace(updateBioDTO.Position)) bio.Position = updateBioDTO.Position;
            if (!string.IsNullOrWhiteSpace(updateBioDTO.Workplace)) bio.Workplace = updateBioDTO.Workplace;
            if (!string.IsNullOrWhiteSpace(updateBioDTO.FacebookUrl)) bio.FacebookUrl = updateBioDTO.FacebookUrl;
            if (!string.IsNullOrWhiteSpace(updateBioDTO.LinkedinUrl)) bio.LinkedinUrl = updateBioDTO.LinkedinUrl;
            if (!string.IsNullOrWhiteSpace(updateBioDTO.GithubUrl)) bio.GithubUrl = updateBioDTO.GithubUrl;
            if (!string.IsNullOrWhiteSpace(updateBioDTO.PortfolioUrl)) bio.PortfolioUrl = updateBioDTO.PortfolioUrl;
            if (!string.IsNullOrWhiteSpace(updateBioDTO.Country)) bio.Country = updateBioDTO.Country;

            await _accountRepository.SaveChangesAsync();

            return _mapper.Map<ResAccountInfoDTO>(account);
        }
        public async Task<bool> ChangePasswordAsync(int accountId, ChangePasswordDTO changePasswordDTO)
        {
            var account = await _accountRepository.GetAccountByIdAsync(accountId);
            if (account == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDTO.OldPassword, account.Password))
                return false;

            if (changePasswordDTO.NewPassword != changePasswordDTO.ConfirmPassword)
                return false;

            account.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDTO.NewPassword);
            await _accountRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ForgetpassAsync(ReqForgetPassword reqForgetPassword)
        {
            var account = await _accountRepository.GetAccountByEmailAsync(reqForgetPassword.Email);
            if (account == null)
                return false;
            account.Password = BCrypt.Net.BCrypt.HashPassword(reqForgetPassword.NewPassword);
            await _accountRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FollowAsync(int followerAccountId, int followingAccountId)
        {
            if (followerAccountId == followingAccountId)
            {
                throw new ArgumentException("Cannot follow yourself.");
            }
            var targetUrl = $"/profile/{followerAccountId}";
            var success = await _accountRepository.FollowAsync(followerAccountId, followingAccountId);
            if (success)
            {
                // Gửi thông báo tới người được follow
                var follower = await _accountRepository.GetAccountByIdAsync(followerAccountId);
                if (follower != null)
                {
                    var message = $" has followed you.";
                    await _notificationService.CreateAndSendAsync(new reqNotificationDTO
                    {
                        UserId = followingAccountId,
                        Message = message,
                        CreatedAt = DateTime.Now,
                        IsRead = false,
                        senderid = followerAccountId,
                        NotificationType = NotiConst.Follow,
                        TargetURL = targetUrl
                    });
                }
            }
            return success;
        }

        public async Task<bool> UnfollowAsync(int followerAccountId, int followingAccountId)
        {
            if (followerAccountId == followingAccountId)
            {
                throw new ArgumentException("Cannot unfollow yourself.");
            }

            return await _accountRepository.UnfollowAsync(followerAccountId, followingAccountId);
        }

        // tìm kiếm account
        public async Task<PagedResult<AccountSearchResultDTO>> SearchAccountsAsync(string searchText, int currentUserId, int pageNumber, int pageSize)
        {
            // ✅ Kiểm tra tài khoản tồn tại và không bị vô hiệu hóa
            var account = await _accountRepository.GetAccountByIdAsync(currentUserId);
            if (account == null || account.Status == "banned" || account.Status == "deactive")
                throw new UnauthorizedAccessException("Tài khoản không hợp lệ hoặc đã bị vô hiệu hóa.");

            // ✅ Gọi repository lấy danh sách kết quả
            var query = _accountRepository.GetSearchAccounts(searchText, currentUserId);

            var totalCount = query.Count();
            var pagedItems = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<AccountSearchResultDTO>(pagedItems, totalCount, pageNumber, pageSize);
        }


        public async Task<PagedResult<AccountRecommendDTO>> RecommendAccountsAsync(int currentAccountId, int pageNumber, int pageSize)
        {
            // ✅ Kiểm tra tài khoản tồn tại và không bị vô hiệu hóa
            var account = await _accountRepository.GetAccountByIdAsync(currentAccountId);
            if (account == null )
                throw new UnauthorizedAccessException("Tài khoản không hợp lệ hoặc đã bị vô hiệu hóa.");

            return await _accountRepository.RecommendAccountsAsync(currentAccountId, pageNumber, pageSize);
        }


        // hàm bỏ dấu
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var normalized = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }
            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
        public async Task<bool> IsFollowingAsync(int followerAccountId, int followingAccountId)
        {
            return await _accountRepository.IsFollowingAsync(followerAccountId, followingAccountId);
        }
        public async Task<bool> BlockAccountAsync(int blockerId, int blockedId)
        {
            return await _accountRepository.BlockAccountAsync(blockerId, blockedId);
        }

        public async Task<bool> UnblockAccountAsync(int blockerId, int blockedId)
        {
            return await _accountRepository.UnblockAccountAsync(blockerId, blockedId);
        }
        public async Task<List<BlockedAccountDto>> GetBlockedAccountsAsync(int blockerId)
        {
            var blocks = await _accountRepository.GetBlockedAccountsAsync(blockerId);
            var dtos = blocks.Select(x => new BlockedAccountDto
            {
                BlockedAccountId = x.BlockedAccountId ?? 0,
                BlockedFullName = x.BlockedAccount?.AccountProfile.FirstName+" " + x.BlockedAccount?.AccountProfile.LastName,
                BlockedAvatar = x.BlockedAccount?.AccountProfile.AvatarUrl
            }).ToList();
            return dtos;
        }
        public async Task<Infrastructure.Models.Account> CreateAdminAccountAsync(CreateAdminDto dto)
        {
            var existing = await _accountRepository.GetAccountByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("Email already exists");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var account = new Infrastructure.Models.Account
            {
                Email = dto.Email,
                Password = hashedPassword,
                Role = "admin",
                Status = "verified",
                CreatedAt = DateTime.Now
            };

            return await _accountRepository.CreateAdminAccountAsync(account);
        }
    }
}

