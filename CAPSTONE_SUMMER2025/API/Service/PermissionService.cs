using API.DTO.StartupDTO;
using API.Repositories.Interfaces;
using API.Service.Interface;
using Infrastructure.Models;
using System.Linq.Expressions;

namespace API.Service
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _repository;

        public PermissionService(IPermissionRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> UpdatePermissionAsync(UpdatePermissionDto dto)
        {
            var permission = await _repository.GetByIdAsync(dto.PermissionId);
            if (permission == null) return false;

            if (dto.CanManagePost.HasValue)
                permission.CanManagePost = dto.CanManagePost.Value;

            if (dto.CanManageCandidate.HasValue)
                permission.CanManageCandidate = dto.CanManageCandidate.Value;

            if (dto.CanManageChatRoom.HasValue)
                permission.CanManageChatRoom = dto.CanManageChatRoom.Value;

            if (dto.CanManageMember.HasValue)
                permission.CanManageMember = dto.CanManageMember.Value;

            if (dto.CanManageMilestone.HasValue)
                permission.CanManageMilestone = dto.CanManageMilestone.Value;

            if (dto.CanManageStartupchat.HasValue)
                permission.CanManageStartupChat = dto.CanManageStartupchat.Value;

            await _repository.UpdateAsync(permission);
            return true;
        }
          public async Task<PermissionDto?> GetPermissionByRoleIdAsync(int roleId)
        {
            var permission = await _repository.GetByRoleIdAsync(roleId);
            if (permission == null) return null;

            return new PermissionDto
            {
                PermissionId = permission.PermissionId,
                RoleId = permission.RoleId,
                CanManagePost = permission.CanManagePost,
                CanManageCandidate = permission.CanManageCandidate,
                CanManageChatRoom = permission.CanManageChatRoom,
                CanManageMember = permission.CanManageMember,
                CanManageMilestone = permission.CanManageMilestone,
                CanManageStartupChat = permission.CanManageStartupChat,
            };
        }
        public async Task<bool> HasPermissionAsync(int accountId, Expression<Func<PermissionInStartup, bool>> permissionSelector)
        {
            var roleId = await _repository.GetRoleIdByAccountIdAsync(accountId);
            if (!roleId.HasValue) return false;

            return await _repository.GetPermissionValueAsync(roleId.Value, permissionSelector);
        }
    }
}
