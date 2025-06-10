using Infrastructure.Models;

namespace API.Repositories.Interfaces
{
    public interface IStartupRepository
    {
        Task<Startup> AddStartupAsync(Startup startup);
        Task<RoleInStartup> AddRoleAsync(RoleInStartup role);
        Task<StartupMember> AddMemberAsync(StartupMember member);
        Task AddStartupCategoryAsync(StartupCategory startupCategory);
        Task SaveChangesAsync();
        Task<List<Startup>> GetAllStartupsAsync();
        Task<bool> IsMemberOfAnyStartup(int accountId);
    }
}
