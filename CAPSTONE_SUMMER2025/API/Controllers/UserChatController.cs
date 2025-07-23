using API.DTO.Mesage;
using API.DTO.StartupDTO;
using API.Hubs;
using API.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserChatController : ControllerBase
    {
        private readonly IUserChatService _service;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly ILogger<UserChatController> _logger;

        public UserChatController(IUserChatService service, IHubContext<MessageHub> hubContext, ILogger<UserChatController> logger)
        {
            _service = service;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpPost("ensure-room")]
        public async Task<IActionResult> EnsureRoom([FromQuery] EnsureRoomDto dto)
        {
            var roomId = await _service.EnsureChatRoomAsync(dto.AccountId, dto.TargetAccountId, dto.TargetStartupId);
            return Ok(roomId);
        }

        [HttpGet("messages/{chatRoomId}")]
        public async Task<IActionResult> GetMessages(int chatRoomId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pagedMessages = await _service.GetMessagesAsync(chatRoomId, pageNumber, pageSize);
            return Ok(pagedMessages);
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromForm] UserMessageDto dto)
        {
            try
            {
                // 1. Lưu tin nhắn
                var message = await _service.SendMessageAsync(dto);

                // 2. Gửi realtime đến tất cả clients trong phòng
                await _hubContext.Clients.Group(dto.ChatRoomId.ToString()).SendAsync("NewMessage", message);

                return Ok(message);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Lỗi gửi tin nhắn (xử lý được)");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi gửi tin nhắn");
                return StatusCode(500, new { message = "Lỗi hệ thống" });
            }
        }       
        [HttpGet("list-chatroom-by/{accountId}")]
        public async Task<IActionResult> GetChatRoomsByAccount(int accountId)
        {
            var result = await _service.GetChatRoomsByAccountAsync(accountId);
            return Ok(result);
        }
    }
}
