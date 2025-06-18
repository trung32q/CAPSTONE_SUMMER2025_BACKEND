using API.DTO.StartupDTO;
using API.Hubs;
using API.Service;
using API.Service.Interface;
using API.Utils.Constants;
using Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Ocsp;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StartupController : ControllerBase
    {
        private readonly IStartupService _service;
        private readonly IAccountService _accountservice;
        private readonly ILogger<StartupService> _logger;
        private readonly IHubContext<MessageHub> _hubContext;


        public StartupController(IStartupService service, ILogger<StartupService> logger, IAccountService accountservice, IHubContext<MessageHub> hubContext)
        {
            _service = service;
            _logger = logger;
            _accountservice = accountservice;
            _hubContext = hubContext;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateStartup([FromForm] CreateStartupRequest req)
        {
            var account = _accountservice.GetAccountByAccountIDAsync(req.CreatorAccountId);
            if (!account.Status.Equals(AccountStatusConst.VERIFIED))
            {
                return BadRequest("Account chua verified!");
            }
            bool isMember = await _service.IsMemberOfAnyStartup(req.CreatorAccountId);
            if (!isMember)
            {
                return BadRequest("Account da nam trong 1 startup!");
            }
            var startupId = await _service.CreateStartupAsync(req);
            return Ok(new { StartupId = startupId });
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllStartups()
        {
            var result = await _service.GetAllStartupsAsync();
            return Ok(result);
        }

        //tạo chatroom
        [HttpPost("create-chatromm")]
        public async Task<IActionResult> Create([FromBody] CreateChatRoomDTO dto)
        {
            var room = await _service.CreateChatRoomAsync(dto);
            return Ok(new { message = "Created", roomId = room.ChatRoomId });
        }
        [HttpGet("check-membership")]
        public async Task<IActionResult> CheckMembership(int accountID)
        {
            if (accountID <= 0)
                return BadRequest(new { message = "accountID không hợp lệ!" });
            bool isMember = await _service.IsMemberOfAnyStartup(accountID);
            return Ok(new { isMember });
        }
        //thêm member
        [HttpPost("add-chatroom-members")]
        public async Task<IActionResult> AddMembers([FromBody] AddMembersDTO dto)
        {
            await _service.AddMembersToChatRoomAsync(dto);
            return Ok(new { message = "Đã thêm thành viên thành công." });
        }

        // lấy ra tất cả member thuộc startup
        [HttpGet("{startupId}/startup-members")]
        public async Task<IActionResult> GetStartupMembers(int startupId)
        {
            var members = await _service.GetMembersByStartupIdAsync(startupId);
            return Ok(members);
        }

        //lấy ra tất cả các members thuộc 1 chatroom
        [HttpGet("{chatRoomId}/chatroom-members")]
        public async Task<IActionResult> GetChatRoomMembers(int chatRoomId)
        {
            try
            {
                var result = await _service.GetMembersByChatRoomIdAsync(chatRoomId);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Handled exception");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                return StatusCode(500, new { message = "Lỗi hệ thống." });
            }
        }

        // lấy ra các chatroom mà account thuộc về
        [HttpGet("chatrooms/{accountId}")]
        public async Task<IActionResult> GetChatRoomsForAccount(
             int accountId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetChatRoomsByAccountIdAsync(accountId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Handled error");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error");
                return StatusCode(500, new { message = "Lỗi hệ thống." });
            }
        }


        // gửi message
         [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
            // 1. Lưu message vào DB qua service
            var message = await _service.SendMessageAsync(request);

                // 2. Gửi realtime qua SignalR cho Receiver (và Sender nếu muốn đồng bộ nhiều tab)
                await _hubContext.Clients.Group(request.ChatRoomId.ToString())
         .SendAsync("ReceiveMessage", message);

            return Ok(message);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Gửi thất bại (xử lý được)");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
            _logger.LogError(ex, "Lỗi hệ thống");
            return StatusCode(500, new { message = "Lỗi hệ thống" });
            }
        }

        // lấy ra các message trong 1 chatroom
        [HttpGet("room-messages/{chatRoomId}")]
        public async Task<IActionResult> GetMessagesInRoom(
        int chatRoomId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetMessagesByRoomIdAsync(chatRoomId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Handled message error");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled server error");
                return StatusCode(500, new { message = "Lỗi hệ thống" });
            }
        }
        [HttpGet("stage")]
        public async Task<IActionResult> GetAllstage()
        {
            var stages = await _service.GetAllStagesAsync();
            return Ok(stages);
        }
        [HttpGet("search-account-by-email")]
        public async Task<IActionResult> SearchAccountByEmail([FromQuery] string keyword)
        {
            var accounts = await _service.SearchByEmailAsync(keyword);

            var result = accounts.Select(a => new {
                AccountId = a.AccountId,
                Email = a.Email,
                AvatarUrl = a.AccountProfile.AvatarUrl // Đảm bảo Include AccountProfile
            }).ToList();

            return Ok(result);
        }
        [HttpPost("create-invite")]
        public async Task<IActionResult> CreateInvite([FromBody] CreateInviteDTO dto)
        {
            bool isMember = await _service.IsMemberOfAnyStartup(dto.Account_ID);
            if (!isMember)
            {
                return BadRequest("Account da nam trong 1 startup!");
            }
            var invite = await _service.CreateInviteAsync(dto);
            return Ok(invite);
        }
        [HttpGet("startupid/{accountId}")]
        public async Task<IActionResult> GetStartupIdByAccountId(int accountId)
        {
            var startupId = await _service.GetStartupIdByAccountIdAsync(accountId);
            if (startupId == null)
                return NotFound("Account does not belong to any startup.");
            return Ok(startupId);
        }
        // GET: api/rolestartup/{roleId}
        [HttpGet("rolestartup/{roleId}")]
        public async Task<IActionResult> GetRole(int roleId)
        {
            var role = await _service.GetRoleAsync(roleId);
            if (role == null) return NotFound();
            return Ok(role);
        }
    
        [HttpGet("Getall-rolestartup/{startupId}")]
        public async Task<IActionResult> GetRolesByStartup(int startupId)
        {
            var roles = await _service.GetRolesByStartupAsync(startupId);
            return Ok(roles);
        }
    
        [HttpPost("Create-role")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            var role = await _service.CreateRoleAsync(dto);
            return Ok(role);
        }

        [HttpPut("Update-role")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDto dto)
        {
            var role = await _service.UpdateRoleAsync(dto);
            return Ok(role);
        }

        [HttpDelete("Delete-role/{roleId}")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            var success = await _service.DeleteRoleAsync(roleId);
            if (!success) return NotFound();
            return Ok();
        }
    }
}
