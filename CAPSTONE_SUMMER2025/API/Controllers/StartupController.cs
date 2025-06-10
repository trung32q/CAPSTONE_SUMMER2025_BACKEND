using API.DTO.StartupDTO;
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
        public StartupController(IStartupService service)
        {
            _service = service;
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
        [HttpGet("check-membership")]
        public async Task<IActionResult> CheckMembership(int accountID)
        {
            if (accountID <= 0)
                return BadRequest(new { message = "accountID không hợp lệ!" });
            bool isMember = await _service.IsMemberOfAnyStartup(accountID);
            return Ok(new { isMember });
        }
    }
}
