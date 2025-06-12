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
        Task<List<Startup>> GetAllStartupsAsync();
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

    }
}
