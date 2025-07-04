using API.DTO.NotificationDTO;
using API.DTO.TaskDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Service.Interface;
using API.Utils.Constants;
using AutoMapper;
using Infrastructure.Models;
using Infrastructure.Repository;
using MimeKit;

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
        public async Task<StartupTask> CreateTaskAsync(CreateStartupTaskDTO dto)
        {
            var task = new StartupTask
            {
                MilestoneId = dto.Milestoneid,
                Title = dto.Title,
                Priority = dto.Priority,
                Description = dto.Description,
                Duedate = dto.DueDate,
                ColumnnStatusId = dto.ColumnnStatusId,
                Note = dto.Note,
                Progress = 0
            };

            // Thêm task, lấy TaskId sau khi save
            var createdTask = await _repo.AddStartupTaskAsync(task);

            if (dto.AssignToAccountIds != null && dto.AssignToAccountIds.Any())
            {
                var assignments = dto.AssignToAccountIds.Select(assignToId => new TaskAssignment
                {
                    TaskId = createdTask.TaskId,
                    AssignToAccountId = assignToId,
                    AssignedByAccountId = dto.AssignedByAccountId,
                    AssignAt = DateTime.Now
                }).ToList();

                await _repo.AddTaskAssignmentsAsync(assignments);
            }
            return createdTask;
        }
        public async Task<List<ColumnWithTasksDto>> GetBoardAsync(int milestoneId)
        {
            var columns = await _repo.GetColumnsByMilestoneIdAsync(milestoneId);
            var tasks = await _repo.GetTasksByMilestoneAsync(milestoneId);

            return columns.Select(c => new ColumnWithTasksDto
            {
                ColumnStatusId = c.ColumnnStatusId,
                ColumnName = c.ColumnName,
                Tasks = tasks
                    .Where(t => t.ColumnnStatusId == c.ColumnnStatusId)
                    .Select(t => new TaskDto
                    {
                        TaskId = t.TaskId,
                        Title = t.Title,
                        Description=t.Description,
                        Priority =t.Priority,
                        DueDate=t.Duedate,
                        AvatarURL= t.TaskAssignments
                    .Where(a => a.AssignToAccount != null && a.AssignToAccount.AccountProfile != null)
                    .Select(a => a.AssignToAccount.AccountProfile.AvatarUrl)
                    .Where(url => !string.IsNullOrEmpty(url))
                    .ToList()
                    }).ToList()
            }).ToList();
        }

        public async Task<List<ResMilestoneDto>> GetAllMilestonesAsync(int startupId)
        {
            var milestones = await _repo.GetAllMilestonesWithMembersAsync(startupId);

            return milestones.Select(m => new ResMilestoneDto
            {
                MilestoneId = m.MilestoneId,
                StartupId = (int)m.StartupId,
                Name = m.Name,
                Description = m.Description,
                StartDate = m.StartDate ?? DateTime.MinValue,
                EndDate = m.EndDate ?? DateTime.MinValue,
                Status = m.Status,
                Members = m.MilestoneAssignments?.Select(a => new MemberInMilestoneDto
                {
                    MemberId = a.MemberId ?? 0,
                    AccountId = a.Member?.AccountId ?? 0,
                    FullName = a.Member?.Account?.AccountProfile != null
                        ? a.Member.Account.AccountProfile.FirstName + " " + a.Member.Account.AccountProfile.LastName
                        : null,
                    AvatarUrl = a.Member?.Account?.AccountProfile?.AvatarUrl
                }).ToList() ?? new List<MemberInMilestoneDto>()
            }).ToList();
        }
        public async Task<bool> UpdateTaskColumnAsync(UpdateTaskColumnDto dto)
        {
            return await _repo.UpdateTaskColumnAsync(dto.TaskId, dto.NewColumnStatusId);
        }

        public async Task<bool> AssignLabelToTaskAsync(int taskId, int labelId)
        {
            return await _repo.AssignLabelToTaskAsync(taskId, labelId);
        }
        public async Task<bool> UpdateTaskAsync(UpdateTaskDto dto)
        {
            return await _repo.UpdateTaskAsync(dto);
        }
        public async Task<bool> AddCommentAsync(CreateCommentTaskDto dto)
        {
            var comment = await _repo.AddCommentAsync(dto);
            var accountids= await _repo.GetAccountIdsByTaskIdAsync(dto.TaskId);
            var accountsender = await _accountRepository.GetAccountByAccountIDAsync(dto.AccountId);
            var targetUrl = $"/task/{dto.TaskId}";
            foreach (var accountId in accountids)
            {
                // Không gửi noti cho người tạo sự kiện
                if (accountId == dto.AccountId) return comment; 

                await _notificationService.CreateAndSendAsync(new reqNotificationDTO
                {
                    UserId = accountId,
                    Message = accountsender.AccountProfile.FirstName+accountsender.AccountProfile.LastName+"has comment on your task.",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    senderid = dto.AccountId,
                    NotificationType = NotiConst.Task,
                    TargetURL = targetUrl
                });
            }
            return comment;
        }
        public async Task<bool> AssignTaskAsync(TaskAssignmentDto dto)
        {
            bool exists = await _repo.TaskAssignmentExistsAsync(dto.TaskId, dto.AssignToAccountId);
            if (exists)
                return false; 
            var entity = new TaskAssignment
            {
                TaskId = dto.TaskId,
                AssignedByAccountId = dto.AssignedByAccountId,
                AssignToAccountId = dto.AssignToAccountId,
                AssignAt = DateTime.Now
            };

           var assign = await _repo.AddTaskAssignmentAsync(entity);
            var targetUrl = $"/task/{dto.TaskId}";
            var accountsender = await _accountRepository.GetAccountByAccountIDAsync(dto.AssignedByAccountId);
           
            return assign;
        }

    }
}
