using API.DTO.AccountDTO;
using API.DTO.PostDTO;
using API.DTO.StartupDTO;
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
        Task<PagedResult<Startup>> GetAllStartupsAsync(int pageNumber, int pageSize);
        Task<ChatRoom> CreateChatRoomAsync(ChatRoom room);
        Task AddMemberAsync(ChatRoomMember member);
        Task AddMembersAsync(List<ChatRoomMember> members);
        Task<ChatRoom> GetChatRoomByIdAsync(int chatRoomId);
        Task<bool> IsStartupMemberAsync(int startupId, int accountId);
        Task<bool> IsChatRoomAdminAsync(int chatRoomId, int accountId);
        Task<List<int>> FilterValidStartupMembersAsync(int startupId, List<int> accountIds);
        Task<List<int>> GetExistingChatRoomMemberIdsAsync(int chatRoomId);
        Task<List<StartupMember>> GetByStartupIdAsync(int startupId);
        Task<List<ChatRoomMember>> GetMembersByChatRoomIdAsync(int chatRoomId);
        IQueryable<ChatRoom> GetChatRoomsByAccountId(int accountId);
        Task<int> AddMessageAsync(ChatMessage message);
        IQueryable<ChatMessage> GetMessagesByRoomId(int chatRoomId);
        Task<bool> IsMemberOfAnyStartup(int accountId);
        Task<List<StartupStage>> GetAllAsync();
        Task<ChatMessage?> GetMessageWithDetailsByIdAsync(int messageId);
        Task<List<Account>> SearchByEmailAsync(string keyword);
        Task<ChatRoomMember?> GetChatRoomMemberAsync(int chatRoomId, int accountId);
        Task UpdateMemberTitleAsync(ChatRoomMember member);
        Task<Invite> AddInviteAsync(Invite invite);
        Task<int?> GetStartupIdByAccountIdAsync(int accountId);
        Task<RoleInStartup> CreateRoleAsync(RoleInStartup role);
        Task<RoleInStartup?> GetRoleAsync(int roleId);
        Task<RoleInStartup> UpdateRoleAsync(RoleInStartup role);
        Task<bool> DeleteRoleAsync(int roleId);
        Task<List<RoleInStartup>> GetRolesByStartupAsync(int startupId);
        IQueryable<StartupMember> SearchAndFilterMembers(int startupId, int? roleId, string? search);
        Task<StartupMember?> GetMemberAsync(int startupId, int accountId);
        Task<bool> RemoveMemberAsync(StartupMember member);
        Task<StartupMember?> GetMemberByAccountAsync(int accountId);
        Task<bool> UpdateMemberRoleAsync(int startupId, int accountId, int newRoleId);
        Task<bool> IsAdminChatRoomAsync(int chatRoomId, int accountId);
        Task<List<ChatRoomMember>> GetChatRoomMembersAsync(int chatRoomId, List<int> accountIds);
        void DeleteChatRoomMembersRange(List<ChatRoomMember> members);
        Task AddPermissionAsync(PermissionInStartup permission);
        Task<(List<Invite>, int)> GetInvitesByStartupIdPagedAsync(int startupId, int pageNumber, int pageSize);
        Task<bool> ExistsPendingInviteAsync(int accountId, int startupId);
        Task<Invite?> GetInviteByIdAsync(int inviteId);
        }
}
