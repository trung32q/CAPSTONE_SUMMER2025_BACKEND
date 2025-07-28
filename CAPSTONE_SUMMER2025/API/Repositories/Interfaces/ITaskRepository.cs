using API.DTO.AccountDTO;
using API.DTO.DartBoardDTO;
using API.DTO.TaskDTO;
using Infrastructure.Models;

namespace API.Repositories.Interfaces
{
    public interface ITaskRepository
    {
        Task<Milestone> AddMilestoneAsync(Milestone milestone);
        Task AddMilestoneAssignmentAsync(MilestoneAssignment assignment);
        Task<bool> AssignmentExistsAsync(int milestoneId, int memberId);
        Task AddColumnStatusAsync(ColumnnStatus column);
        Task<int> GetMaxSortOrderAsync(int milestoneId);
        Task<List<ColumnnStatus>> GetColumnsByMilestoneIdAsync(int milestoneId);
        Task<StartupTask> AddStartupTaskAsync(StartupTask task);
        Task AddTaskAssignmentsAsync(List<TaskAssignment> assignments);
        Task<List<StartupTask>> GetTasksByMilestoneAsync(int milestoneId);
        Task<List<Milestone>> GetAllMilestonesWithMembersAsync(int startupId);
        Task<bool> UpdateTaskColumnAsync(int taskId, int newColumnStatusId);
        Task<bool> AssignLabelToTaskAsync(int taskId, int labelId);
        Task<bool> UpdateTaskAsync(UpdateTaskDto dto);
        Task<bool> AddCommentAsync(CreateCommentTaskDto dto);
        Task<List<int>> GetAccountIdsByTaskIdAsync(int taskId);
        Task<bool> AddTaskAssignmentAsync(TaskAssignment entity);
        Task<bool> TaskAssignmentExistsAsync(int taskId, int assignToAccountId);
        Task<PagedResult<TasklistDto>> GetTaskByMilestoneIdPagedAsync(int milestoneId, int pageNumber, int pageSize);
        Task<PagedResult<TasklistDto>> GetTaskByMilestoneIdPagedAsync(int milestoneId, int pageNumber, int pageSize, string? search, int? columnStatusId);
        Task<bool> AddTaskAssignAsync(TaskAssignment entity);
        Task<bool> RemoveTaskAssignmentAsync(int taskId, int accountId);
        Task<List<CommentTaskDto>> GetCommentsByTaskIdAsync(int taskId);
        Task<StartupTask?> GetTaskByIdAsync(int taskId);
        Task<List<MemberInMilestoneDto>> GetMembersInMilestoneAsync(int milestoneId);
        Task<TaskAssignment?> GetAssignToByTaskIdAsync(int taskId);
        Task<int?> GetMemberIDByAccountIdAsync(int accountid);
        Task<List<Label>> GetAllAsync();
        Task<List<ActivityLogDto>> GetAllActivityLogsAsync(int milestoneId);
        Task AddActivityLogAsync(TaskActivityLog log);
        Task<string> GetColumnNameAsync(int columnId);
        Task<List<LabeltaskDto>> GetTaskslable(List<int> taskIds);
        Task<int?> GetMilestoneIDByTaskIDAsync(int taskId);
        Task<TaskDashboardResponseDto> GetFullDashboardAsync(int milestoneId);
    }
}
