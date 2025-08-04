using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using API.Repositories.Interfaces;
using API.Attributes;
using API.Utils.Constants;
using API.DTO.AccountDTO;
using API.DTO.ProfileDTO;
using API.DTO.BioDTO;
using API.Service.Interface;
using API.Service;
using MailKit.Search;
using Google.Api;
namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ICCCDService _cccdService;
        public AccountController(IAccountService accountService, ICCCDService cccdService)
        {
            _accountService = accountService;
            _cccdService = cccdService;
        }
        [HttpGet("GetAllAccount")]
        public async Task<ActionResult<IEnumerable<ResAccountDTO>>> GetAccounts()
        {
            var accounts = await _accountService.GetAllAccountAsync();
            return Ok(accounts);
        }

       

        [HttpGet("get-account-info/{accountId}")]
        public async Task<ActionResult<ResAccountInfoDTO>> GetAccountInoByAccountID(int accountId)
        {
            var account = await _accountService.GetAccountByAccountIDAsync(accountId);
            if (account == null)
            {
                return NotFound($"Account with ID {accountId} not found");
            }
            return Ok(account);
        }

        [HttpGet("get-following/{accountId}")]
        public async Task<ActionResult<List<ResAccountInfoDTO>>> GetFollowing(int accountId)
        {
            var following = await _accountService.GetFollowingAsync(accountId);
            return Ok(following);
        }

        [HttpGet("get-follower/{accountId}")]
        public async Task<ActionResult<List<ResAccountInfoDTO>>> GetFollowers(int accountId)
        {
            var followers = await _accountService.GetFollowersAsync(accountId);
            return Ok(followers);
        }

        [HttpPut("update-profile/{accountId}")]
        public async Task<IActionResult> UpdateProfile(int accountId, [FromForm] ReqUpdateProfileDTO updateProfileDTO)
        {
            if (updateProfileDTO == null)
                return BadRequest("Profile data is required.");

            var result = await _accountService.UpdateProfileAsync(accountId, updateProfileDTO);

            return result == null
                ? NotFound($"Account with ID {accountId} not found.")
                : Ok(result);
        }

        [HttpPut("update-bio/{accountId}")]
        public async Task<ActionResult<ResAccountInfoDTO>> UpdateBio(int accountId, [FromBody] ReqUpdateBioDTO updateBioDTO)
        {
            if (updateBioDTO == null)
                return BadRequest("Profile data is required");

            var updatedAccount = await _accountService.UpdateBioAsync(accountId, updateBioDTO);
            if (updatedAccount == null)
                return NotFound($"Account with ID {accountId} not found");
            return Ok(updatedAccount);
        }

        [HttpPut("change-password/{accountId}")]
        public async Task<ActionResult> ChangePassword(int accountId, [FromBody] ChangePasswordDTO changePasswordDTO)
        {
            if (changePasswordDTO == null ||
                string.IsNullOrEmpty(changePasswordDTO.OldPassword) ||
                string.IsNullOrEmpty(changePasswordDTO.NewPassword) ||
                string.IsNullOrEmpty(changePasswordDTO.ConfirmPassword))
                return BadRequest("Old password, new password, and confirm password are required");

            var success = await _accountService.ChangePasswordAsync(accountId, changePasswordDTO);
            if (!success)
                return BadRequest("Invalid old password, confirm password does not match, or account not found");

            return Ok("Password changed successfully");
        }

        [HttpPost("verify-cccd")]
        public async Task<IActionResult> Verify([FromForm] CccdVerificationRequestDTO request)
        {
            if (request.CccdFront == null || request.Selfie == null)
                return BadRequest("Thiếu ảnh CCCD hoặc ảnh selfie");

            var result = await _cccdService.VerifyCccdAsync(request.CccdFront, request.Selfie);

            return Ok(result);
        }      


        [HttpPost("follow")]
        public async Task<IActionResult> Follow([FromBody] FollowRequestDTO request)
        {
            if (request == null || request.FollowerAccountId <= 0 || request.FollowingAccountId <= 0)
            {
                return BadRequest("Invalid follower or following account ID.");
            }

            try
            {
                var success = await _accountService.FollowAsync(request.FollowerAccountId, request.FollowingAccountId);
                if (!success)
                {
                    return BadRequest("Follow action failed. Accounts may not exist or already followed.");
                }
                return Ok(new { message = "Followed successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("unfollow")]
        public async Task<IActionResult> Unfollow([FromBody] FollowRequestDTO request)
        {
            if (request == null || request.FollowerAccountId <= 0 || request.FollowingAccountId <= 0)
            {
                return BadRequest("Invalid follower or following account ID.");
            }

            try
            {
                var success = await _accountService.UnfollowAsync(request.FollowerAccountId, request.FollowingAccountId);
                if (!success)
                {
                    return BadRequest("Unfollow action failed. You may not be following this account.");
                }
                return Ok(new { message = "Unfollowed successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // tìm kiếm account
        [HttpGet("search-account")]
        public async Task<IActionResult> SearchAccounts(string searchText, int currentAccountId, int pageNumber = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return BadRequest(new { message = "searchText is required." });

            try
            {
                var result = await _accountService.SearchAccountsAsync(searchText, currentAccountId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }


        [HttpGet("recommend")]
        public async Task<IActionResult> RecommendAccounts(int currentAccountId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var result = await _accountService.RecommendAccountsAsync(currentAccountId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
        [HttpGet("is-following")]
        public async Task<IActionResult> IsFollowing(int followerAccountId, int followingAccountId)
        {
            var isFollowing = await _accountService.IsFollowingAsync(followerAccountId, followingAccountId);
            return Ok(new { isFollowing });
        }
        // POST: api/accountblock/block
        [HttpPost("block")]
        public async Task<IActionResult> BlockAccount(int accountId, int blockedId)
        {
            var success = await _accountService.BlockAccountAsync(accountId, blockedId);
            if (!success) return BadRequest("Already blocked or invalid.");
            return Ok("Blocked successfully.");
        }

        // POST: api/accountblock/unblock
        [HttpPost("unblock")]
        public async Task<IActionResult> UnblockAccount(int accountId, int blockedId)
        {
            var success = await _accountService.UnblockAccountAsync(accountId, blockedId);
            if (!success) return BadRequest("Not blocked or invalid.");
            return Ok("Unblocked successfully.");
        }
        [HttpGet("blocked-list/{accountId}")]
        public async Task<IActionResult> GetBlockedAccounts(int accountId)
        {
            var list = await _accountService.GetBlockedAccountsAsync(accountId);
            return Ok(list);
        }
        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
        {
            try
            {
                var account = await _accountService.CreateAdminAccountAsync(dto);
                return Ok(new { message = "Admin created", account.Email });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
