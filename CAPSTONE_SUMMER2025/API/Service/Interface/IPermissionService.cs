using API.DTO.StartupDTO;
using Infrastructure.Models;
using System.Linq.Expressions;

namespace API.Service.Interface
{
    public interface IPermissionService
    {
        Task<bool> UpdatePermissionAsync(UpdatePermissionDto dto);
        Task<PermissionDto?> GetPermissionByRoleIdAsync(int roleId);
        Task<bool> HasPermissionAsync(int accountId, Expression<Func<PermissionInStartup, bool>> permissionSelector);
    }
      
}
