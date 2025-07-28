using API.DTO.AccountDTO;
using API.DTO.DartBoardDTO;
using API.DTO.TaskDTO;
using API.Repositories.Interfaces;
using AutoMapper;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace API.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly CAPSTONE_SUMMER2025Context _context;

        public TaskRepository(CAPSTONE_SUMMER2025Context context)
        {
            _context = context;
        }
        public async Task AddMilestoneAssignmentAsync(MilestoneAssignment assignment)
        {
            _context.MilestoneAssignments.Add(assignment);
            await _context.SaveChangesAsync();
        }
        public async Task<Milestone> AddMilestoneAsync(Milestone milestone)
        {
            _context.Milestones.Add(milestone);
            await _context.SaveChangesAsync();
            return milestone;
        }
        public async Task<bool> AssignmentExistsAsync(int milestoneId, int memberId)
        {
            return await _context.MilestoneAssignments
                .AnyAsync(x => x.MilestoneId == milestoneId && x.MemberId == memberId);
        }
        public async Task AddColumnStatusAsync(ColumnnStatus column)
        {
            _context.ColumnnStatuses.Add(column);
            await _context.SaveChangesAsync();
        }
        public async Task<int> GetMaxSortOrderAsync(int milestoneId)
        {
            var max = await _context.ColumnnStatuses
                .Where(c => c.MilestoneId == milestoneId)
                .MaxAsync(c => (int?)c.SortOrder);

            return max ?? 0; 
        }
        public async Task<List<ColumnnStatus>> GetColumnsByMilestoneIdAsync(int milestoneId)
        {
            return await _context.ColumnnStatuses
                .Where(c => c.MilestoneId == milestoneId)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
        }
        public async Task<StartupTask> AddStartupTaskAsync(StartupTask task)
        {
            _context.StartupTasks.Add(task);
            await _context.SaveChangesAsync(); 
            return task;
        }

        // Thêm nhiều assignment cho 1 task
        public async Task AddTaskAssignmentsAsync(List<TaskAssignment> assignments)
        {
            _context.TaskAssignments.AddRange(assignments);
            await _context.SaveChangesAsync();
        }
        public async Task<List<StartupTask>> GetTasksByMilestoneAsync(int milestoneId)
        {
            return await _context.StartupTasks
                .Include(x => x.TaskAssignments)
                    .ThenInclude(x => x.AssignToAccount)
                        .ThenInclude(acc => acc.AccountProfile)               
                .Where(t => t.MilestoneId == milestoneId)
                .ToListAsync();
        }
        public async Task<List<LabeltaskDto>> GetTaskslable(List<int> taskIds)
        {
            var taskLabels = await _context.StartupTaskLabels
      .Where(x => taskIds.Contains(x.TaskId))
      .Join(_context.Labels,
          stl => stl.LabelId,
          l => l.LabelId,
          (stl, l) => new LabeltaskDto{ 
              taskid =stl.TaskId,
              LabelID=  l.LabelId, 
              LabelName = l.LabelName,
              Color =l.Color})
      .ToListAsync();
            return taskLabels;
        }
        public async Task<List<Milestone>> GetAllMilestonesWithMembersAsync(int startupId )
        {
            var query = _context.Milestones
                .Include(m => m.MilestoneAssignments)
                    .ThenInclude(a => a.Member)
                        .ThenInclude(sm => sm.Account)
                            .ThenInclude(a => a.AccountProfile)
                .AsQueryable();          
                query = query.Where(m => m.StartupId == startupId);

            return await query.ToListAsync();
        }
        public async Task<bool> UpdateTaskColumnAsync(int taskId, int newColumnStatusId)
        {
            var task = await _context.StartupTasks.FindAsync(taskId);
            if (task == null) return false;

            task.ColumnnStatusId = newColumnStatusId;
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> AssignLabelToTaskAsync(int taskId, int labelId)
        {
            // Kiểm tra tồn tại
            bool exists = await _context.StartupTaskLabels
                .AnyAsync(x => x.TaskId == taskId && x.LabelId == labelId);
            if (exists)
                return false; // đã gán rồi

            var entity = new StartupTaskLabel
            {
                TaskId = taskId,
                LabelId = labelId
            };
            _context.StartupTaskLabels.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTaskAsync(UpdateTaskDto dto)
        {
            var task = await _context.StartupTasks.FindAsync(dto.TaskId);
            if (task == null) return false;

            if (!string.IsNullOrEmpty(dto.Title)) task.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Priority)) task.Priority = dto.Priority;
            if (!string.IsNullOrEmpty(dto.Description)) task.Description = dto.Description;
            if (dto.DueDate.HasValue) task.Duedate = dto.DueDate;
            if (dto.Progress.HasValue) task.Progress = dto.Progress;
            if (dto.ColumnnStatusId.HasValue) task.ColumnnStatusId = dto.ColumnnStatusId;
            if (!string.IsNullOrEmpty(dto.Note)) task.Note = dto.Note;
            if (dto.labelcolorID.HasValue)
            {
                // Xoá hết label cũ
                _context.StartupTaskLabels.RemoveRange(task.StartupTaskLabels);

                // Nếu truyền label mới thì add
                _context.StartupTaskLabels.Add(new StartupTaskLabel
                {
                    TaskId = task.TaskId,
                    LabelId = dto.labelcolorID.Value
                });
            }
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> AddCommentAsync(CreateCommentTaskDto dto)
        {
            var comment = new CommentTask
            {
                TaskId = dto.TaskId,
                AccountId = dto.AccountId,
                Comment = dto.Comment,
                CreateAt = DateTime.UtcNow
            };
            _context.CommentTasks.Add(comment);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<int>> GetAccountIdsByTaskIdAsync(int taskId)
        {
            return await _context.TaskAssignments
                .Where(a => a.TaskId == taskId)
                .Select(a => a.AssignToAccountId.Value)
                .ToListAsync();
        }
        public async Task<bool> AddTaskAssignmentAsync(TaskAssignment entity)
        {
            _context.TaskAssignments.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> TaskAssignmentExistsAsync(int taskId, int assignToAccountId)
        {
            return await _context.TaskAssignments
                .AnyAsync(x => x.TaskId == taskId && x.AssignToAccountId == assignToAccountId);
        }
        public async Task<PagedResult<TasklistDto>> GetTaskByMilestoneIdPagedAsync(int milestoneId, int pageNumber, int pageSize)
        {
            // Query gốc
            var query = _context.StartupTasks
                .Include(x => x.ColumnnStatus)
                .Include(x => x.TaskAssignments)
                    .ThenInclude(x => x.AssignedByAccount)
                        .ThenInclude(acc => acc.AccountProfile)
                .Include(x => x.TaskAssignments)
                    .ThenInclude(x => x.AssignToAccount)
                        .ThenInclude(acc => acc.AccountProfile)
                .Where(x => x.MilestoneId == milestoneId);

            var totalCount = await query.CountAsync();

            // Phân trang trước khi chuyển sang DTO
            var items = await query
                .OrderBy(x => x.TaskId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new TasklistDto
                {
                    TaskId = x.TaskId,
                    Title = x.Title,
                    Priority = x.Priority,
                    Description = x.Description,
                    DueDate = x.Duedate,
                    Progress = x.Progress,
                    ColumnStatus = x.ColumnnStatus.ColumnName,
                    Note = x.Note,
                    CreatedBy = x.TaskAssignments.FirstOrDefault() != null && x.TaskAssignments.FirstOrDefault().AssignedByAccount != null
                        ? x.TaskAssignments.FirstOrDefault().AssignedByAccount.AccountProfile.AvatarUrl
                        : null,
                    AsignTo = x.TaskAssignments
                            .Where(a => a.AssignToAccount != null && a.AssignToAccount.AccountProfile != null)
                            .Select(a => new AssignToDTO
                            {
                                Id = a.AssignToAccount.AccountId,
                                Fullname = a.AssignToAccount.AccountProfile.FirstName + " " + a.AssignToAccount.AccountProfile.LastName,
                                AvatarURL = a.AssignToAccount.AccountProfile.AvatarUrl
                            }).ToList()
                })
                .ToListAsync();

            return new PagedResult<TasklistDto>(items, totalCount, pageNumber, pageSize);
        }
        public async Task<PagedResult<TasklistDto>> GetTaskByMilestoneIdPagedAsync(
    int milestoneId, int pageNumber, int pageSize, string? search, int? columnStatusId)
        {
            var query = _context.StartupTasks
                .Include(x => x.ColumnnStatus)
                .Include(x => x.TaskAssignments)
                    .ThenInclude(x => x.AssignedByAccount)
                        .ThenInclude(acc => acc.AccountProfile)
                .Include(x => x.TaskAssignments)
                    .ThenInclude(x => x.AssignToAccount)
                        .ThenInclude(acc => acc.AccountProfile)
                .Where(x => x.MilestoneId == milestoneId);

            // Lọc theo cột (status)
            if (columnStatusId.HasValue)
                query = query.Where(x => x.ColumnnStatusId == columnStatusId);

            // Search (title/description)
            if (!string.IsNullOrEmpty(search))
            {
                var lowered = search.Trim().ToLower();
                query = query.Where(x =>
                    (!string.IsNullOrEmpty(x.Title) && x.Title.ToLower().Contains(lowered)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.ToLower().Contains(lowered))
                );
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.TaskId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new TasklistDto
                {
                    TaskId = x.TaskId,
                    Title = x.Title,
                    Priority = x.Priority,
                    Description = x.Description,
                    DueDate = x.Duedate,
                    Progress = x.Progress,
                    ColumnStatus = x.ColumnnStatus.ColumnName,
                    Note = x.Note,
                    CreatedBy = x.TaskAssignments.FirstOrDefault() != null && x.TaskAssignments.FirstOrDefault().AssignedByAccount != null
                        ? x.TaskAssignments.FirstOrDefault().AssignedByAccount.AccountProfile.AvatarUrl
                        : null,
                    AsignTo = x.TaskAssignments
                            .Where(a => a.AssignToAccount != null && a.AssignToAccount.AccountProfile != null)
                            .Select(a => new AssignToDTO
                            {
                                Id = a.AssignToAccount.AccountId,
                                Fullname = a.AssignToAccount.AccountProfile.FirstName + " " + a.AssignToAccount.AccountProfile.LastName,
                                AvatarURL = a.AssignToAccount.AccountProfile.AvatarUrl
                            }).ToList()
                })
                .ToListAsync();

            return new PagedResult<TasklistDto>(items, totalCount, pageNumber, pageSize);
        }
        // Thêm assignment
        public async Task<bool> AddTaskAssignAsync(TaskAssignment entity)
        {
            // Kiểm tra đã gán chưa (tránh trùng)
            var exists = await _context.TaskAssignments
                .AnyAsync(x => x.TaskId == entity.TaskId && x.AssignToAccountId == entity.AssignToAccountId);
            if (exists) return false;

            _context.TaskAssignments.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // Hủy assignment
        public async Task<bool> RemoveTaskAssignmentAsync(int taskId, int accountId)
        {
            var assignment = await _context.TaskAssignments
                .FirstOrDefaultAsync(x => x.TaskId == taskId && x.AssignToAccountId == accountId);
            if (assignment == null) return false;

            _context.TaskAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<CommentTaskDto>> GetCommentsByTaskIdAsync(int taskId)
        {
            return await _context.CommentTasks
                .Where(x => x.TaskId == taskId)
                .OrderByDescending(x => x.CreateAt)
                .Select(x => new CommentTaskDto
                {
                    CommentTaskId = x.CommentTaskId,
                    TaskId = (int)x.TaskId,
                    AccountId = (int)x.AccountId,
                    Comment = x.Comment,
                    CreateAt = x.CreateAt,
                    FullName = x.Account != null
                        ? x.Account.AccountProfile.FirstName + " " + x.Account.AccountProfile.LastName
                        : null,
                    AvatarUrl = x.Account != null && x.Account.AccountProfile != null? x.Account.AccountProfile.AvatarUrl: null
                })
                .ToListAsync();
        }
        public async Task<StartupTask?> GetTaskByIdAsync(int taskId)
        {
            return await _context.StartupTasks
                .Include(x => x.ColumnnStatus)
                .Include(x => x.TaskAssignments)
                    .ThenInclude(a => a.AssignToAccount)
                        .ThenInclude(acc => acc.AccountProfile)
                .Include(x => x.TaskAssignments)
                    .ThenInclude(a => a.AssignedByAccount)
                        .ThenInclude(acc => acc.AccountProfile)
                .FirstOrDefaultAsync(x => x.TaskId == taskId);
        }
        public async Task<int?> GetMilestoneIDByTaskIDAsync(int taskId)
        {
            return await _context.StartupTasks
                .Where(x => x.TaskId == taskId)
                .Select(x => (int?)x.MilestoneId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<MemberInMilestoneDto>> GetMembersInMilestoneAsync(int milestoneId)
        {
            return await _context.MilestoneAssignments
                .Where(ma => ma.MilestoneId == milestoneId)
                .Include(ma => ma.Member)
                    .ThenInclude(sm => sm.Account)
                        .ThenInclude(a => a.AccountProfile)
                .Select(ma => new MemberInMilestoneDto
                {
                    MemberId = (int)ma.MemberId,
                    AccountId = ma.Member.Account.AccountId,
                    FullName = ma.Member.Account.AccountProfile.FirstName + " " + ma.Member.Account.AccountProfile.LastName,
                    AvatarUrl = ma.Member.Account.AccountProfile.AvatarUrl
                })
                .ToListAsync();
        }
        public async Task<TaskAssignment?> GetAssignToByTaskIdAsync(int taskId)
        {
            return await _context.TaskAssignments.Include(a => a.AssignToAccount).ThenInclude(acc => acc.AccountProfile).FirstOrDefaultAsync(x => x.TaskId == taskId);
        }
        public async Task<int?> GetMemberIDByAccountIdAsync(int accountid)
        {
            return (await _context.StartupMembers.FirstOrDefaultAsync(x => x.AccountId == accountid))?.StartupMemberId;
        }
        public async Task<List<Label>> GetAllAsync()
        {
            return await _context.Labels.ToListAsync();
        }
        public async Task<List<ActivityLogDto>> GetAllActivityLogsAsync(int milestoneId)
        {
            var taskIds = await _context.StartupTasks
          .Where(t => t.MilestoneId == milestoneId)
          .Select(t => t.TaskId)
         .ToListAsync();
            var logs = await _context.TaskActivityLogs.Where(log => taskIds.Contains(log.TaskId))
            .Include(l => l.ByAccount)
            .ThenInclude(acc => acc.AccountProfile)
            .OrderByDescending(l => l.AtTime)
            .ToListAsync();
            var dtos = logs.Select(l => new ActivityLogDto
            {
                ActivityId = l.ActivityId,
                TaskId = l.TaskId,
                ActionType = l.ActionType,
                AtTime = l.AtTime,
                Content = l.Content,
                ByAccountId = l.ByAccountId,
                FullName = l.ByAccount != null && l.ByAccount.AccountProfile != null
        ? l.ByAccount.AccountProfile.FirstName + " " + l.ByAccount.AccountProfile.LastName
        : null,
                AvatarUrl = l.ByAccount != null && l.ByAccount.AccountProfile != null
        ? l.ByAccount.AccountProfile.AvatarUrl
        : null
            }).ToList();
            return dtos;
        }

        public async Task AddActivityLogAsync(TaskActivityLog log)
        {
            _context.TaskActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        public async Task<string> GetColumnNameAsync(int columnId)
        {
            return await _context.ColumnnStatuses
                .Where(x => x.ColumnnStatusId == columnId)
                .Select(x => x.ColumnName)
                .FirstOrDefaultAsync();
        }
        public async Task<TaskDashboardResponseDto> GetFullDashboardAsync(int milestoneId)
        {
            var now = DateTime.UtcNow;

            // 🔹 Truy vấn task theo trạng thái (ColumnStatus)
            var statusCounts = await _context.StartupTasks
                .Where(t => t.MilestoneId == milestoneId)
                .GroupBy(t => new { t.ColumnnStatusId, t.ColumnnStatus.ColumnName })
                .Select(g => new TaskStatusCountDto
                {
                    ColumnStatusId = (int)g.Key.ColumnnStatusId,
                    StatusName = g.Key.ColumnName,
                    Count = g.Count()
                })
                .ToListAsync();

            // 🔹 Truy vấn danh sách assignments + các navigation liên quan
            var assignments = await _context.TaskAssignments
                .Where(a => a.Task.MilestoneId == milestoneId)
                .Include(a => a.Task).ThenInclude(t => t.ColumnnStatus)
                .Include(a => a.AssignToAccount).ThenInclude(acc => acc.AccountProfile)
                .ToListAsync();

            // 🔹 Xử lý group và thống kê task theo từng thành viên
            var memberStats = assignments
                .GroupBy(a => a.AssignToAccountId)
                .Select(g =>
                {
                    var account = g.First().AssignToAccount;
                    var total = g.Count();
                    var completed = g.Count(x => x.Task.ColumnnStatus?.ColumnName == "DONE");
                    var overdue = g.Count(x =>
                        x.Task.Duedate.HasValue &&
                        x.Task.Duedate.Value < now &&
                        x.Task.ColumnnStatus?.ColumnName != "DONE");

                    return new TaskDashboardDto
                    {
                       
                        AccountName = account.AccountProfile.FirstName + " " + account.AccountProfile.LastName,
                        AccountAvatar = account.AccountProfile.AvatarUrl,
                        TotalTasks = total,
                        CompletedTasks = completed,
                        OverdueTasks = overdue
                        // CompletionRate tính tự động trong DTO
                    };
                })
                .ToList();

            // 🔹 Trả kết quả
            return new TaskDashboardResponseDto
            {
                StatusCounts = statusCounts,
                MemberTaskStats = memberStats
            };
        }


    }
}
