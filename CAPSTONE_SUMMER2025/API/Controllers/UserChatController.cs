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
        public async Task<IActionResult> EnsureRoom([FromBody] EnsureRoomDto dto)
        {
            var roomId = await _service.EnsureChatRoomAsync(dto.AccountId, dto.TargetAccountId, dto.TargetStartupId);
            return Ok(roomId);
        }

        [HttpGet("messages/{chatRoomId}")]
        public async Task<IActionResult> GetMessages(int chatRoomId)
        {
            var messages = await _service.GetMessagesAsync(chatRoomId);
            return Ok(messages);
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] UserMessageDto dto)
        {
            await _service.SendMessageAsync(dto);
            return Ok();
        }
    }
}
