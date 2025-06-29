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

    }
}
