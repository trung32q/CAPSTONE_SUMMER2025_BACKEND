using API.Repositories.Interfaces;
using API.Service;
using API.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminSerivce)
        {
            _adminService = adminSerivce;
        }

        [HttpGet("admin-dashboard")]
        public async Task<IActionResult> GetAccountStatsLast7Days()
        {
            var result = await _adminService.GetAdminDashboardResult();
            return Ok(result);
        }

    }
}
