using API.DTO.NotificationDTO;
using API.Repositories.Interfaces;
using API.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        [HttpPost("CreateNotification")]
        public async Task<IActionResult> CreateNotification([FromBody] reqNotificationDTO dto)
        {
            var result = await _service.CreateAndSendAsync(dto);

            if (result == null)
            {
                return NotFound(new { error = "Account not found" });
            }

            return Ok(result);
        }
    }
}
