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
            return await _context.StartupTasks.Include(x=>x.TaskAssignments)
                .ThenInclude(x=>x.AssignToAccount)
                .ThenInclude(x=>x.AccountProfile)
                .Where(t => t.MilestoneId == milestoneId)
                .ToListAsync();
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

    }
}
