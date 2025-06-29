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
                var exists = await _milestoneService.AssignmentsExistAsync(dto.MilestoneId, dto.MemberIds);
                if (exists==null)
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


    }
}
