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

    }
}
