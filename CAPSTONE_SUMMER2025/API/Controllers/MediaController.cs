using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        [HttpGet("view-pdf")]
        public async Task<IActionResult> ViewPdf(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("Thiếu URL.");

            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Thêm User-Agent giả Browser
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return NotFound("Không tìm thấy file PDF. " + errorContent);
            }

            var stream = await response.Content.ReadAsStreamAsync();
            return File(stream, "application/pdf");
        }


        [HttpGet("proxy-pdf")]
        public async Task<IActionResult> ProxyPdf([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("Missing URL");

            try
            {
                using var client = new HttpClient();
                var data = await client.GetByteArrayAsync(url);
                return File(data, "application/pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error downloading PDF", error = ex.Message });
            }
        }



    }
}
