using API.DTO.Mesage;
using API.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserChatController : ControllerBase
    {
        private readonly IUserChatService _service;

        public UserChatController(IUserChatService service)
        {
            _service = service;
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


        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromQuery] UserMessageDto dto)
        {
            await _service.SendMessageAsync(dto);
            return Ok();
        }
    }
}
