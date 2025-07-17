using Infrastructure.Models;
using System.Linq.Expressions;

namespace API.Repositories.Interfaces
{
    public interface IPermissionRepository
    {
        Task<PermissionInStartup?> GetByIdAsync(int permissionId);
        Task UpdateAsync(PermissionInStartup permission);
        Task<PermissionInStartup?> GetByRoleIdAsync(int roleId);
        Task<int?> GetRoleIdByAccountIdAsync(int accountId);
        Task<bool> GetPermissionValueAsync(int roleId, Expression<Func<PermissionInStartup, bool>> permissionSelector);
    }
}
