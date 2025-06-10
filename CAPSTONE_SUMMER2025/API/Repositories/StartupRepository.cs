using API.Repositories.Interfaces;
using AutoMapper;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class StartupRepository : IStartupRepository
    {
        private readonly IMapper _mapper;
        private readonly CAPSTONE_SUMMER2025Context _context;

        public StartupRepository(IMapper mapper, CAPSTONE_SUMMER2025Context context)
        {
            _mapper = mapper;
            _context = context;
        }
        public async Task<StartupMember> AddMemberAsync(StartupMember member)
        {
            _context.StartupMembers.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<RoleInStartup> AddRoleAsync(RoleInStartup role)
        {
            _context.RoleInStartups.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Startup> AddStartupAsync(Startup startup)
        {
            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();
            return startup;
        }

        public async Task AddStartupCategoryAsync(StartupCategory startupCategory)
        {
            _context.StartupCategories.Add(startupCategory);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
        public async Task<List<Startup>> GetAllStartupsAsync()
        {
            return await _context.Startups
                .Include(s => s.StartupCategories)
                    .ThenInclude(sc => sc.Category)
                .ToListAsync();
        }
        public async Task<bool> IsMemberOfAnyStartup(int accountId)
        {
            return await _context.StartupMembers.AnyAsync(sm => sm.AccountId == accountId);
        }
    }
}
