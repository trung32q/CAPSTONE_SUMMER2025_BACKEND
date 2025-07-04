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
    }
}
