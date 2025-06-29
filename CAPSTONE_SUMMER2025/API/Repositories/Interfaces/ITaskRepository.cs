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
    }
}
