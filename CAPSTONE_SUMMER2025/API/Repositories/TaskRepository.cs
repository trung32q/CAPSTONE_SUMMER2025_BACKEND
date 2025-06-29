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
        public async Task<List<int?>> AssignmentsExistAsync(int milestoneId, List<int> memberIds)
        {
            return await _context.MilestoneAssignments
                .Where(x => x.MilestoneId == milestoneId && memberIds.Contains((int)x.MemberId))
                .Select(x => x.MemberId)
                .ToListAsync();
        }


    }
}
