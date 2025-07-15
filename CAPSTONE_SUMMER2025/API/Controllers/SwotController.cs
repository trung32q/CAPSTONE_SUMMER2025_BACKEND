using API.DTO.StartupDTO;
using API.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SwotController : ControllerBase
    {
        private readonly ISwotService _swotService;
        public SwotController(ISwotService swotService)
        {
            _swotService = swotService;
        }

        [HttpPost("analyze-swot")]
        public async Task<IActionResult> AnalyzeSwot([FromBody] SwotBmcDto dto)
        {
            try
            {
                var result = await _swotService.AnalyzeSwotAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
