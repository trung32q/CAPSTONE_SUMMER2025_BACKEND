using API.DTO.TaskDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Service.Interface;
using AutoMapper;
using Infrastructure.Models;
using Infrastructure.Repository;

namespace API.Service
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repo;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper; 
        private readonly INotificationService _notificationService;
        public TaskService(ITaskRepository repo, IMapper mapper, IAccountRepository accountRepository, INotificationService notificationService)
        {
            _repo = repo;
            _mapper = mapper;
            _accountRepository = accountRepository;
            _notificationService = notificationService;
        }
        public async Task<bool> AddMembersToMilestoneAsync(int milestoneId, List<int> memberIds)
        {
            bool addedAny = false;
            foreach (var memberId in memberIds.Distinct())
            {
                    var assignment = new MilestoneAssignment
                    {
                        MilestoneId = milestoneId,
                        MemberId = memberId
                    };
                    await _repo.AddMilestoneAssignmentAsync(assignment);
                    addedAny = true;
               
            }
            return addedAny; // Trả về true nếu có ít nhất 1 member được thêm mới
        }
        public async Task<List<int?>> AssignmentsExistAsync(int milestoneId, List<int> memberIds)
        {
            return await _repo.AssignmentsExistAsync(milestoneId, memberIds);
        }

        public async Task<bool> CreateMilestoneAsync(CreateMilestoneDto dto)
        {
            try
            {
                var milestone = new Milestone
                {
                    StartupId = dto.StartupId,
                    Name = dto.Name,
                    Description = dto.Description,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Status = dto.Status
                };
                await _repo.AddMilestoneAsync(milestone);

                // Gán member nếu có
                foreach (var memberId in dto.MemberIds.Distinct())
                {
                    var assignment = new MilestoneAssignment
                    {
                        MilestoneId = milestone.MilestoneId,
                        MemberId = memberId
                    };
                    await _repo.AddMilestoneAssignmentAsync(assignment);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
