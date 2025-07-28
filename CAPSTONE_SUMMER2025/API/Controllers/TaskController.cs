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
        private readonly IPermissionService _permissionService;
        public TaskController(ITaskService milestoneService, IPermissionService service)
        {
            _Service = milestoneService;
            _permissionService = service; 
        }
        [HttpPost("CreateMilestone")]
        public async Task<IActionResult> CreateMilestone([FromBody] CreateMilestoneDto dto)
        {
            var allowed = await _permissionService.HasPermissionAsync(dto.AccountID, p => p.CanManageMilestone);
            if (!allowed)
                return Forbid("Không đủ quyền để thao tác milestone");
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

        [HttpPost("startup-task/assign-label")]
        public async Task<IActionResult> AssignLabelToTask([FromBody] AssignLabelToTaskDTO dto)
        {
            var success = await _Service.AssignLabelToTaskAsync(dto.StartupTaskId, dto.LabelId);
            if (!success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Label đã được gán hoặc không hợp lệ"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Gán label thành công"
            });
        }
        [HttpPut("update-task")]
        public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskDto dto)
        {
            var result = await _Service.UpdateTaskAsync(dto);
            if (!result)
                return NotFound(new { success = false, message = "Task không tồn tại!" });
            return Ok(new { success = true, message = "Cập nhật task thành công!" });
        }
        [HttpPost("comment-task")]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentTaskDto dto)
        {
            var result = await _Service.AddCommentAsync(dto);
            if (result)
                return Ok(new { success = true, message = "Thêm comment thành công!" });
            else
                return BadRequest(new { success = false, message = "Thêm comment thất bại!" });
        }
        [HttpPost("assign-task")]
        public async Task<IActionResult> AssignTask([FromBody] TaskAssignmentDto dto)
        {
            var result = await _Service.AssignTaskAsync(dto);
            if (result)
                return Ok(new { success = true, message = "Assigned successfully!" });
            return BadRequest(new { success = false, message = "User is already assigned to this task!" });
        }   
        [HttpGet("tasks-list-by-milestone")]
        public async Task<IActionResult> GetTasksByMilestone(
    int milestoneId,
    int pageNumber = 1,
    int pageSize = 10,
    string? search = null,          
    int? columnStatusId = null      
    )
        {
            if (milestoneId <= 0)
                return BadRequest(new { message = "MilestoneId không hợp lệ!" });

            if (pageNumber < 1 || pageSize < 1)
                return BadRequest(new { message = "PageNumber và PageSize phải lớn hơn 0!" });

            var pagedTasks = await _Service.GetTaskByMilestoneIdPagedAsync(
                milestoneId, pageNumber, pageSize, search, columnStatusId
            );

            if (pagedTasks == null || pagedTasks.Items.Count == 0)
                return NotFound(new { message = "Không có task nào phù hợp!" });

            return Ok(pagedTasks);
        }   

        [HttpDelete("unassign-task")]
        public async Task<IActionResult> UnassignAccountFromTask(int taskId, int accountId)
        {
            var result = await _Service.UnassignAccountFromTaskAsync(taskId, accountId);
            if (!result)
                return NotFound(new { message = "Không tìm thấy assignment!" });
            return Ok(new { message = "Đã hủy gán account khỏi task!" });
        }
        [HttpGet("get-all-task-comment")]
        public async Task<IActionResult> GetCommentsByTaskId(int taskId)
        {
            if (taskId <= 0)
                return BadRequest(new { message = "TaskId không hợp lệ!" });

            var comments = await _Service.GetCommentsByTaskIdAsync(taskId);
            return Ok(comments);
        }
        [HttpGet("task-detail/{taskId}")]
        public async Task<IActionResult> GetTaskDetail(int taskId)
        {
            if (taskId <= 0)
                return BadRequest(new { message = "TaskId không hợp lệ!" });

            var detail = await _Service.GetTaskDetailByIdAsync(taskId);
            if (detail == null)
                return NotFound(new { message = "Không tìm thấy task!" });

            return Ok(detail);
        }
        [HttpGet("members-in-milestone")]
        public async Task<IActionResult> GetMembersInMilestone(int milestoneId)
        {
            if (milestoneId <= 0)
                return BadRequest(new { message = "MilestoneId không hợp lệ!" });

            var members = await _Service.GetMembersInMilestoneAsync(milestoneId);
            if (members == null || members.Count == 0)
                return NotFound(new { message = "Không tìm thấy thành viên nào trong milestone này!" });

            return Ok(members);
        }
        [HttpGet("members-in-task")]
        public async Task<IActionResult> GetMembersInTask(int taskId)
        {
            if (taskId <= 0)
                return BadRequest(new { message = "taskId không hợp lệ!" });

            var members = await _Service.GetMembersInTaskAsync(taskId);
            if (members == null || members.Count == 0)
                return NotFound(new { message = "Không tìm thấy thành viên nào trong task này!" });

            return Ok(members);
        }
        [HttpGet("labels")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _Service.GetAllLabelsAsync();
            return Ok(result);
        }
        [HttpGet("all-activity-log")]
        public async Task<IActionResult> GetAllActivityLog(int milestoneId)
        {
            var logs = await _Service.GetAllActivityLogsAsync(milestoneId);
            if (logs == null || logs.Count == 0)
                return NotFound(new { message = "Không có activity log nào cho milestone này!" });
            return Ok(logs);
        }
        [HttpGet("dashboard-task")]
        public async Task<IActionResult> GetDashboard([FromQuery] int milestoneId)
        {
            var result = await _Service.GetFullDashboardAsync(milestoneId);
            return Ok(result);
        }

    }
}
 