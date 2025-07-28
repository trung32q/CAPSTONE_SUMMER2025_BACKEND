using API.DTO.AccountDTO;
using API.DTO.DartBoardDTO;
using API.DTO.TaskDTO;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface ITaskService
    {
        Task<bool> CreateMilestoneAsync(CreateMilestoneDto dto);
        Task<bool> AddMembersToMilestoneAsync(int milestoneId, List<int> memberIds);
        Task<bool> AssignmentExistsAsync(int milestoneId, int memberId);
        Task<bool> CreateNewColumnAsync(CreateColumnDto dto);
        Task<List<ColumnStatusDto>> GetColumnsByMilestoneIdAsync(int milestoneId);
        Task<StartupTask> CreateTaskAsync(CreateStartupTaskDTO dto);
        Task<List<ColumnWithTasksDto>> GetBoardAsync(int milestoneId);
        Task<List<ResMilestoneDto>> GetAllMilestonesAsync(int startupId );
        Task<bool> UpdateTaskColumnAsync(UpdateTaskColumnDto dto);
        Task<bool> AssignLabelToTaskAsync(int taskId, int labelId);
        Task<bool> UpdateTaskAsync(UpdateTaskDto dto);
        Task<bool> AddCommentAsync(CreateCommentTaskDto dto);
        Task<bool> AssignTaskAsync(TaskAssignmentDto dto);
        Task<PagedResult<TasklistDto>> GetTaskByMilestoneIdPagedAsync(int milestoneId, int pageNumber, int pageSize);
        Task<PagedResult<TasklistDto>> GetTaskByMilestoneIdPagedAsync( int milestoneId, int pageNumber, int pageSize, string? search, int? columnStatusId);
        Task<bool> AssignAccountToTaskAsync(TaskAssignmentDto dto);
        Task<bool> UnassignAccountFromTaskAsync(int taskId, int accountId);
        Task<List<CommentTaskDto>> GetCommentsByTaskIdAsync(int taskId);
        Task<TasklistDto?> GetTaskDetailByIdAsync(int taskId);
        Task<List<MemberInMilestoneDto>> GetMembersInMilestoneAsync(int milestoneId);
        Task<List<AssignToDTO>> GetMembersInTaskAsync(int taskId);
        Task<List<LabelDto>> GetAllLabelsAsync();
        Task<List<ActivityLogDto>> GetAllActivityLogsAsync(int milestoneId);
        Task AddActivityLogAsync(ActivityLogDto dto);
        Task<TaskDashboardResponseDto> GetFullDashboardAsync(int milestoneId);

    }
}
