using API.DTO.TaskDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Service.Interface;
using API.Utils.Constants;
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
        public async Task<bool> AssignmentExistsAsync(int milestoneId, int memberId)
        {
            return await _repo.AssignmentExistsAsync(milestoneId, memberId);
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
                    Status = MilestoneStatus.ACTIVE
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
                var col1 = new ColumnnStatus { MilestoneId = milestone.MilestoneId, ColumnName = "TO DO", SortOrder = 1, Description = "To do" };
                await _repo.AddColumnStatusAsync(col1);

                var col2 = new ColumnnStatus { MilestoneId = milestone.MilestoneId, ColumnName = "IN PROGRESS", SortOrder = 2, Description = "Doing" };
                await _repo.AddColumnStatusAsync(col2);

                var col3 = new ColumnnStatus { MilestoneId = milestone.MilestoneId, ColumnName = "DONE", SortOrder = 3, Description = "Done" };
                await _repo.AddColumnStatusAsync(col3);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> CreateNewColumnAsync(CreateColumnDto dto)
        {
            int maxSort = await _repo.GetMaxSortOrderAsync(dto.MilestoneId);
            var column = new ColumnnStatus
            {
                MilestoneId = dto.MilestoneId,
                ColumnName = dto.ColumnName,
                SortOrder = maxSort + 1,
                Description = dto.Description
            };
            await _repo.AddColumnStatusAsync(column);
            return true;
        }
        public async Task<List<ColumnStatusDto>> GetColumnsByMilestoneIdAsync(int milestoneId)
        {
            var columns = await _repo.GetColumnsByMilestoneIdAsync(milestoneId);

            return columns.Select(c => new ColumnStatusDto
            {
                ColumnStatusId = c.ColumnnStatusId,
                ColumnName = c.ColumnName,
                SortOrder = (int)c.SortOrder,
                Description = c.Description
            }).ToList();
        }
    }
}
