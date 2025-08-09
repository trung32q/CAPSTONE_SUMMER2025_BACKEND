using API.DTO.StartupDTO;
using API.Service;
using API.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _service;

        public PermissionController(IPermissionService service)
        {
            _service = service;
        }

        [HttpPut("update-permission")]
        public async Task<IActionResult> UpdatePermission([FromBody] UpdatePermissionDto dto)
        {
            var success = await _service.UpdatePermissionAsync(dto);
            if (!success) return NotFound("Permission not found");

            return Ok("Permission updated successfully");
        }
        [HttpGet("by-role/{roleId}")]
        public async Task<IActionResult> GetPermissionByRoleId(int roleId)
        {
            var permission = await _service.GetPermissionByRoleIdAsync(roleId);
            if (permission == null)
                return NotFound("Permission not found");

            return Ok(permission);
        }
        [HttpGet("can-post")]
        public async Task<IActionResult> CheckCanPost([FromQuery] int accountId)
        {
            var canPost = await _service.HasPermissionAsync(accountId, p => p.CanManagePost);
            return Ok(canPost);
        }
        [HttpGet("can-manage-member")]
        public async Task<IActionResult> CheckCanManageMember([FromQuery] int accountId)
        {
            var canmember = await _service.HasPermissionAsync(accountId, p => p.CanManageMember);
            return Ok(canmember);
        }
        [HttpGet("can-manage-Startup-chat")]
        public async Task<IActionResult> CheckCanManageStartupchat([FromQuery] int accountId)
        {
            var canmember = await _service.HasPermissionAsync(accountId, p => p.CanManageStartupChat);
            return Ok(canmember);
        }
    }
}
