using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace API.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly CAPSTONE_SUMMER2025Context _context;

        public PermissionRepository(CAPSTONE_SUMMER2025Context context)
        {
            _context = context;
        }

        public async Task<PermissionInStartup?> GetByIdAsync(int permissionId)
        {
            return await _context.PermissionInStartups.FindAsync(permissionId);
        }

        public async Task UpdateAsync(PermissionInStartup permission)
        {
            _context.PermissionInStartups.Update(permission);
            await _context.SaveChangesAsync();
        }
        public async Task<int?> GetRoleIdByAccountIdAsync(int accountId)
        {
            return await _context.StartupMembers
                .Where(m => m.AccountId == accountId)
                .Select(m => (int?)m.RoleId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> GetPermissionValueAsync(int roleId, Expression<Func<PermissionInStartup, bool>> permissionSelector)
        {
            return await _context.PermissionInStartups
                .Where(p => p.RoleId == roleId)
                .Select(permissionSelector)
                .FirstOrDefaultAsync();
        }
        public async Task<PermissionInStartup?> GetByRoleIdAsync(int roleId)
        {
            return await _context.PermissionInStartups
                                 .FirstOrDefaultAsync(p => p.RoleId == roleId);
        }

    }
}
