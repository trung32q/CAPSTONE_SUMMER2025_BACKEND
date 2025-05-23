using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using AutoMapper;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountBlockController : ControllerBase
    {
        private readonly IAccountBlockRepository _accountBlockRepository;
        private readonly IMapper _mapper;

        public AccountBlockController(IAccountBlockRepository accountBlockRepository, IMapper mapper)
        {
            _accountBlockRepository = accountBlockRepository;
            _mapper = mapper;
        }

        [HttpPost("block")]
        public async Task<ActionResult<AccountBlockResponseDTO>> BlockAccount([FromBody] BlockAccountDTO blockDto)
        {
            try
            {
                var isAlreadyBlocked = await _accountBlockRepository.IsBlockedAsync(blockDto.BlockerAccountId, blockDto.BlockedAccountId);
                if (isAlreadyBlocked)
                {
                    return BadRequest("Account is already blocked");
                }

                var block = _mapper.Map<Infrastructure.Models.AccountBlock>(blockDto);
                var result = await _accountBlockRepository.BlockAccountAsync(block);
                return Ok(_mapper.Map<AccountBlockResponseDTO>(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("unblock")]
        public async Task<IActionResult> UnblockAccount([FromBody] UnblockAccountDTO unblockDto)
        {
            try
            {
                var result = await _accountBlockRepository.UnblockAccountAsync(unblockDto.BlockerAccountId, unblockDto.BlockedAccountId);
                if (!result)
                {
                    return NotFound("Block relationship not found");
                }
                return Ok("Account unblocked successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("list-blocked-accounts/{accountId}")]
        public async Task<ActionResult<IEnumerable<AccountBlockResponseDTO>>> GetBlockedAccounts(int accountId)
        {
            try
            {
                var blockedAccounts = await _accountBlockRepository.GetBlockedAccountsAsync(accountId);
                return Ok(_mapper.Map<IEnumerable<AccountBlockResponseDTO>>(blockedAccounts));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //[HttpGet("list-blocked-by/{accountId}")]
        //public async Task<ActionResult<IEnumerable<AccountBlockResponseDTO>>> GetBlockedByAccounts(int accountId)
        //{
        //    try
        //    {
        //        var blockedByAccounts = await _accountBlockRepository.GetBlockedByAccountsAsync(accountId);
        //        return Ok(_mapper.Map<IEnumerable<AccountBlockResponseDTO>>(blockedByAccounts));
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}
    }
} 