using API.DTO.StartupDTO;
using API.Service;
using API.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StartupController : ControllerBase
    {
        private readonly IStartupService _service;
        private readonly ILogger<StartupService> _logger;

        public StartupController(IStartupService service, ILogger<StartupService> logger)
        {
            _service = service;
            _logger = logger;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateStartup([FromForm] CreateStartupRequest req)
        {
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

        // gửi tin nhắn
        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                await _service.SendMessageAsync(request);
                return Ok(new { message = "Gửi thành công" });
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
    }
}
