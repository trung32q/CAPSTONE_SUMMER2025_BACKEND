using API.DTO.TaskDTO;
using API.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _milestoneService;

        public TaskController(ITaskService milestoneService)
        {
            _milestoneService = milestoneService;
        }
        [HttpPost("CreateMilestone")]
        public async Task<IActionResult> CreateMilestone([FromBody] CreateMilestoneDto dto)
        {
            var result = await _milestoneService.CreateMilestoneAsync(dto);
            if (result)
                return Ok(new { success = true, message = "Tạo milestone thành công!" });
            else
                return Ok(new { success = false, message = "Tạo milestone thất bại!" });
        }
        [HttpPost("add-milestone-members")]
        public async Task<IActionResult> AddMembersToMilestone([FromBody] AddMembersToMilestoneDto dto)
        {
            bool addedAny = false;
            foreach (var memberId in dto.MemberIds.Distinct())
            {
                // Kiểm tra tồn tại
                var exists = await _milestoneService.AssignmentExistsAsync(dto.MilestoneId, memberId);
                if (!exists)
                {
                    await _milestoneService.AddMembersToMilestoneAsync(dto.MilestoneId, dto.MemberIds);
                    addedAny = true;
                }
            }
            if (addedAny)
                return Ok(new { success = true, message = "Add thành công!" });
            else
                return Ok(new { success = false, message = "Không có thành viên nào được thêm (có thể đã tồn tại)!" });
        }
        [HttpPost("create-column")]
        public async Task<IActionResult> CreateColumn([FromBody] CreateColumnDto dto)
        {
            var success = await _milestoneService.CreateNewColumnAsync(dto);
            if (success)
                return Ok(new { success = true, message = "Tạo cột mới thành công!" });
            else
                return BadRequest(new { success = false, message = "Tạo cột mới thất bại!" });
        }
        [HttpGet("get-all-columns")]
        public async Task<IActionResult> GetColumnsByMilestone(int milestoneId)
        {
            var columns = await _milestoneService.GetColumnsByMilestoneIdAsync(milestoneId);
            return Ok(columns);
        }
    }
}
