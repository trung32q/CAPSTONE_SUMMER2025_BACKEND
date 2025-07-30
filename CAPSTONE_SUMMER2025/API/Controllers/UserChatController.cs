using API.DTO.Mesage;
using API.DTO.StartupDTO;
using API.DTO.VideoCall;
using API.Hubs;
using API.Service.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserChatController : ControllerBase
    {
        private readonly IUserChatService _service;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly ILogger<UserChatController> _logger;
        private readonly IHubContext<CallHub> _hubcallContext;
        private readonly CAPSTONE_SUMMER2025Context _context;
        public UserChatController(IUserChatService service, IHubContext<MessageHub> hubContext, ILogger<UserChatController> logger, IHubContext<CallHub> hubcallContext,CAPSTONE_SUMMER2025Context context)
        {
            _service = service;
            _hubContext = hubContext;
            _logger = logger;
            _hubcallContext = hubcallContext;
            _context = context; 
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
        [HttpGet("list-chatroom-startup/{startupId}")]
        public async Task<IActionResult> GetChatRoomsByStartup(int startupId)
        {
            var result = await _service.GetChatRoomsByStartupAsync(startupId);
            return Ok(result);
        }
        [HttpPost("start-call")]
        public async Task<IActionResult> StartCall([FromBody] StartCallDto dto)
        {
            var result = await _service.StartCallAsync(dto);
            var members = await _context.UserChatRoomMembers
         .Where(x => x.ChatRoomId == dto.ChatRoomId)
         .Select(x => x.AccountId)
         .ToListAsync();

            var calleeId = members.FirstOrDefault(x => x != dto.AccountId);
            // Gửi thông báo tới người nhận nếu họ đang online
            if (CallHub.UserConnectionMap.TryGetValue((int)calleeId, out var calleeConnectionId))
            {
                await _hubcallContext.Clients.Client(calleeConnectionId).SendAsync("IncomingCall", new
                {
                    roomToken = result.RoomToken,
                    callSessionId = result.CallSessionId,
                    from = result.Caller // Thường là CallerId
                });
            }
            else
            {
                Console.WriteLine($"Callee with ID {calleeId} is offline or not connected.");
                // Có thể lưu thông báo đợi gửi sau nếu muốn
            }

            // Trả kết quả cho caller
            return Ok(new
            {
                calleeConnectionId,
                result.RoomToken,
                result.CallSessionId,
                result.Caller
            });
        }


        [HttpPost("end-call")]
        public async Task<IActionResult> EndCall([FromBody] EndCallDto dto)
        {
            await _service.EndCallAsync(dto.CallSessionId);
            await _hubcallContext.Clients.Group(dto.RoomToken).SendAsync("CallEnded", dto.CallSessionId);
            return Ok();
        }

        [HttpGet("history-call")]
        public async Task<IActionResult> GetHistory([FromQuery] int chatRoomId)
        {
            var result = await _service.GetCallHistoryAsync(chatRoomId);
            return Ok(result);
        }

        [HttpPost("accept-call")]
        public async Task<IActionResult> AcceptCall([FromBody] UpdateCallStatusDto dto)
        {
            await _service.AcceptCallAsync(dto.CallSessionId);

            await _hubContext.Clients.Group(dto.RoomToken).SendAsync("CallAccepted", dto.CallSessionId);

            return Ok();
        }

        [HttpPost("reject-call")]
        public async Task<IActionResult> RejectCall([FromBody] UpdateCallStatusDto dto)
        {
            await _service.RejectCallAsync(dto.CallSessionId);

            // Optionally notify caller that callee rejected
            await _hubContext.Clients.Group(dto.RoomToken).SendAsync("CallRejected", dto.CallSessionId);
            return Ok();
        }

    }
}
