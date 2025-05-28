using API.DTO.AccountDTO;
using API.DTO.NotificationDTO;
using API.Repositories.Interfaces;
using API.Service;
using API.Service.Interface;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;
        private readonly IAccountRepository _accountRepository;

        public NotificationController(INotificationService service, IAccountRepository accountRepository )
        {
            _service = service;
            _accountRepository = accountRepository;
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
        [HttpGet("Notification/{accountId}")]
        public async Task<ActionResult<PagedResult<resNotificationDTO>>> GetNotifications(int accountId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var account = await _accountRepository.GetAccountByIdAsync(accountId);
            if (account == null)
            {
                return NotFound(new { message = $"Account with ID {accountId} not found." });
            }
            var result = await _service.GetPagedNotificationsAsync(accountId, pageNumber, pageSize);
            return Ok(result);
        }
        [HttpGet("unread-count/{accountId}")]
        public async Task<IActionResult> GetUnreadNotificationCount(int accountId)
        {
            var account = await _accountRepository.GetAccountByIdAsync(accountId);
            if (account == null)
            {
                return NotFound(new { message = $"Account with ID {accountId} not found." });
            }
            var count = await _service.GetUnreadNotificationCountAsync(accountId);
            return Ok(new { unreadCount = count });
        }
        [HttpPut("mark-as-read{notificationId}")]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId, [FromQuery] int accountId)
        {
            var success = await _service.MarkNotificationAsReadAsync(notificationId, accountId);
            if (!success)
            {
                return NotFound("Notification not found or does not belong to the user.");
            }
            return Ok(new { message = "Notification marked as read." });
        }

    }
}
