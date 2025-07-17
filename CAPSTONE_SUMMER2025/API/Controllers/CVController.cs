using API.DTO.PostDTO;
using API.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CVController : ControllerBase
    {

        public readonly ICVService _cvService;
        public readonly IPostService _postServiceService;

        public CVController(ICVService cvService, IPostService postServiceService)
        {
            _cvService = cvService;
            _postServiceService = postServiceService;
        }

        //apply cv
        [HttpPost("apply-cv")]
        public async Task<IActionResult> Apply([FromForm] ApplyCVRequestDTO dto)
        {
            var success = await _cvService.ApplyCVAsync(dto);
            if (!success)
                return BadRequest("Already applied or error occurred");

            return Ok("CV submitted successfully");
        }

        // lấy ra cv theo statupid cùng với lọc theo positionId
        [HttpGet("candidateCv/{startupId}")]
        public async Task<IActionResult> GetCVsByStartup(int startupId, int postionId = 0, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var cvs = await _cvService.GetCVsOfStartupAsync(startupId, postionId, page, pageSize);

            if (cvs == null || cvs.Items.Count == 0)
                return NotFound("No CVs found for this startup");

            return Ok(cvs);
        }

        // accpept hoặc reject cv
        [HttpPut("response-candidateCV/{candidateCVId}")]
        public async Task<IActionResult> UpdateStatus(int candidateCVId, string status)
        {
            var success = await _cvService.ResponseCandidateCVAsync(candidateCVId, status);
            if (!success) return NotFound("Candidate CV not found.");

            return Ok("resonse successfully.");
        }

        //xem account đã nộp cv vào bài internshippost chưa
        [HttpGet("submitted")]
        public async Task<IActionResult> HasSubmittedCV([FromQuery] int accountId, [FromQuery] int internshipId)
        {
            var hasSubmitted = await _cvService.CheckSubmittedCVAsync(accountId, internshipId);
            return Ok(hasSubmitted);
        }

        //lấy ra top internship post được nộp cv nhiều nhất
        [HttpGet("top-internship-post-cv-submitted")]
        public async Task<IActionResult> GetTopCVSubmittedInternshipPosts([FromQuery] int top = 5)
        {
            var result = await _postServiceService.GetTopInternshipPostsAsync(top);
            return Ok(result);
        }

    }
}
