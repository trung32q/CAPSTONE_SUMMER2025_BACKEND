using API.DTO.AccountDTO;
using API.DTO.AuthDTO;
using API.DTO.BioDTO;
using API.DTO.ProfileDTO;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface IAccountService
    {
        Task<List<ResAccountDTO>> GetAllAccountAsync();
        Task<ResAccountInfoDTO> GetAccountByAccountIDAsync(int accountId);
        Task<List<ResAccountInfoDTO>> GetFollowingAsync(int accountId);
        Task<List<ResAccountInfoDTO>> GetFollowersAsync(int accountId);
        Task<ResAccountInfoDTO> UpdateProfileAsync(int accountId, ReqUpdateProfileDTO updateProfileDTO);
        Task<ResAccountInfoDTO> UpdateBioAsync(int accountId, ReqUpdateBioDTO updateBioDTO);
        Task<bool> ChangePasswordAsync(int accountId, ChangePasswordDTO changePasswordDTO);
        Task<bool> ForgetpassAsync( ReqForgetPassword reqForgetPassword);
        Task<bool> FollowAsync(int followerAccountId, int followingAccountId);
        Task<bool> UnfollowAsync(int followerAccountId, int followingAccountId);
        Task<PagedResult<AccountSearchResultDTO>> SearchAccountsAsync(string searchText, int pageNumber, int pageSize, int currentUser);
        Task<PagedResult<AccountRecommendDTO>> RecommendAccountsAsync(int currentAccountId, int pageNumber, int pageSize);
        Task<bool> IsFollowingAsync(int followerAccountId, int followingAccountId);
        Task<bool> BlockAccountAsync(int blockerId, int blockedId);
        Task<bool> UnblockAccountAsync(int blockerId, int blockedId);
        Task<List<BlockedAccountDto>> GetBlockedAccountsAsync(int blockerId);
        Task<Account> CreateAdminAccountAsync(CreateAdminDto dto);
    }
}
