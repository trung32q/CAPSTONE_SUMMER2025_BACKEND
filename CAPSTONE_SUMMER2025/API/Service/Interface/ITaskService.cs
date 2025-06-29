using API.DTO.TaskDTO;

namespace API.Service.Interface
{
    public interface ITaskService
    {
        Task<bool> CreateMilestoneAsync(CreateMilestoneDto dto);
        Task<bool> AddMembersToMilestoneAsync(int milestoneId, List<int> memberIds);
        Task<List<int?>> AssignmentsExistAsync(int milestoneId, List<int> memberIds);
    }
}
