using API.DTO.TaskDTO;

namespace API.Service.Interface
{
    public interface ITaskService
    {
        Task<bool> CreateMilestoneAsync(CreateMilestoneDto dto);
        Task<bool> AddMembersToMilestoneAsync(int milestoneId, List<int> memberIds);
        Task<bool> AssignmentExistsAsync(int milestoneId, int memberId);
        Task<bool> CreateNewColumnAsync(CreateColumnDto dto);
        Task<List<ColumnStatusDto>> GetColumnsByMilestoneIdAsync(int milestoneId);
    }
}
