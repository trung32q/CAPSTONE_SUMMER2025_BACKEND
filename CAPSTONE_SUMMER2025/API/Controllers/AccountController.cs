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
namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }
        [HttpGet("GetAllAccount")]
        public async Task<ActionResult<IEnumerable<ResAccountDTO>>> GetAccounts()
        {
            var accounts = await _accountRepository.GetAllAccountAsync();
            return Ok(accounts);
        }

        // GET: api/Account/email@example.com
        [HttpGet("{email}")]
        public async Task<ActionResult<ResAccountDTO>> GetAccountByEmail(string email)
        {
            var account = await _accountRepository.GetAccountByEmailAsync(email);

            if (account == null)
            {
                return NotFound($"Account with email {email} not found");
            }

            return Ok(account);
        }

        [HttpGet("get-account-info/{accountId}")]
        public async Task<ActionResult<ResAccountInfoDTO>> GetAccountInoByAccountID(int accountId)
        {
            var account = await _accountRepository.GetAccountByAccountIDAsync(accountId);
            if (account == null)
            {
                return NotFound($"Account with ID {accountId} not found");
            }
            return Ok(account);
        }

        [HttpGet("get-following/{accountId}")]
        public async Task<ActionResult<List<ResAccountInfoDTO>>> GetFollowing(int accountId)
        {
            var following = await _accountRepository.GetFollowingAsync(accountId);
            return Ok(following);
        }

        [HttpGet("get-follower/{accountId}")]
        public async Task<ActionResult<List<ResAccountInfoDTO>>> GetFollowers(int accountId)
        {
            var followers = await _accountRepository.GetFollowersAsync(accountId);
            return Ok(followers);
        }

        [HttpPut("update-profile/{accountId}")]
        public async Task<ActionResult<ResAccountInfoDTO>> UpdateProfile(int accountId, [FromBody] ReqUpdateProfileDTO updateProfileDTO)
        {
            if (updateProfileDTO == null)
                return BadRequest("Profile data is required");

            var updatedAccount = await _accountRepository.UpdateProfileAsync(accountId, updateProfileDTO);
            if (updatedAccount == null)
                return NotFound($"Account with ID {accountId} not found");
            return Ok(updatedAccount);
        }

        [HttpPut("update-bio/{accountId}")]
        public async Task<ActionResult<ResAccountInfoDTO>> UpdateBio(int accountId, [FromBody] ReqUpdateBioDTO updateBioDTO)
        {
            if (updateBioDTO == null)
                return BadRequest("Profile data is required");

            var updatedAccount = await _accountRepository.UpdateBioAsync(accountId, updateBioDTO);
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

            var success = await _accountRepository.ChangePasswordAsync(accountId, changePasswordDTO);
            if (!success)
                return BadRequest("Invalid old password, confirm password does not match, or account not found");

            return Ok("Password changed successfully");
        }
    }
}
