using Infrastructure.Models;

namespace API.Repositories.Interfaces
{
    public interface ITaskRepository
    {
        Task<Milestone> AddMilestoneAsync(Milestone milestone);
        Task AddMilestoneAssignmentAsync(MilestoneAssignment assignment);
        Task<List<int?>> AssignmentsExistAsync(int milestoneId, List<int> memberIds);
    }
}
