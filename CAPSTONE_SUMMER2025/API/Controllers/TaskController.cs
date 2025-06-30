using API.DTO.TaskDTO;
using API.Service;
using API.Service.Interface;
using Google.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _Service;

        public TaskController(ITaskService milestoneService)
        {
            _Service = milestoneService;
        }
        [HttpPost("CreateMilestone")]
        public async Task<IActionResult> CreateMilestone([FromBody] CreateMilestoneDto dto)
        {
            var result = await _Service.CreateMilestoneAsync(dto);
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
                var exists = await _Service.AssignmentExistsAsync(dto.MilestoneId, memberId);
                if (!exists)
                {
                    await _Service.AddMembersToMilestoneAsync(dto.MilestoneId, dto.MemberIds);
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
            var success = await _Service.CreateNewColumnAsync(dto);
            if (success)
                return Ok(new { success = true, message = "Tạo cột mới thành công!" });
            else
                return BadRequest(new { success = false, message = "Tạo cột mới thất bại!" });
        }
        [HttpGet("get-all-columns")]
        public async Task<IActionResult> GetColumnsByMilestone(int milestoneId)
        {
            var columns = await _Service.GetColumnsByMilestoneIdAsync(milestoneId);
            return Ok(columns);
        }
        [HttpPost("create-task")]
        public async Task<IActionResult> CreateTask([FromBody] CreateStartupTaskDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { error = "Title is required." });

            var task = await _Service.CreateTaskAsync(dto);

            return Ok(new
            {
                success = true,
                message = "Task created and assigned successfully."           
            });
        }
        [HttpGet("get-task-board")]
        public async Task<IActionResult> GetBoard(int milestoneId)
        {
            var result = await _Service.GetBoardAsync(milestoneId);
            return Ok(result);
        }
        [HttpGet("get-all-milestone")]
        public async Task<IActionResult> GetAll([FromQuery] int startupId)
        {
            var result = await _Service.GetAllMilestonesAsync(startupId);
            if (result == null || result.Count == 0)
                return NotFound(new { message = "Không có milestone nào!" });
            return Ok(result);
        }
        [HttpPut("Change-task-column")]
        public async Task<IActionResult> UpdateTaskColumn([FromBody] UpdateTaskColumnDto dto)
        {
            var success = await _Service.UpdateTaskColumnAsync(dto);
            if (!success)
                return NotFound(new { message = "Task không tồn tại!" });

            return Ok(new { success = true, message = "Cập nhật status thành công!" });
        }
    }
}
 