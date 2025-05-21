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
namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
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
        public async Task<ActionResult<ResAccountInfoDTO>> UpdateProfile(int accountId, [FromBody] ReqUpdateProfileDTO updateProfileDTO)
        {
            if (updateProfileDTO == null)
                return BadRequest("Profile data is required");

            var updatedAccount = await _accountService.UpdateProfileAsync(accountId, updateProfileDTO);
            if (updatedAccount == null)
                return NotFound($"Account with ID {accountId} not found");
            return Ok(updatedAccount);
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
    }
}
