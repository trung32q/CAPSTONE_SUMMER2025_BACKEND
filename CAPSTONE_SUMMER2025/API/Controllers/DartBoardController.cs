using API.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DartBoardController : ControllerBase
    {
        public readonly IDartboardService _service;

        public DartBoardController(IDartboardService service)
        {
            _service = service;
        }

        // lấy ra số lượng tương tác với bài viết của starup trong 7 ngày
        [HttpGet("startup/{startupId}/dartboard")]
        public async Task<IActionResult> GetDailyInteractionsByStartup(int startupId)
        {
            var result = await _service.GetStartupDartBoard(startupId);
            return Ok(result);
        }
    }
}
