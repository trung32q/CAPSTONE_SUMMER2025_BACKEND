using API.DTO.Mesage;
using Infrastructure.Models;

namespace API.Repositories.Interfaces
{
    public interface IUserChatRepository
    {
        Task<UserChatRoom?> GetChatRoomAsync(int accountId, int? targetAccountId, int? targetStartupId);
        Task<UserChatRoom> CreateChatRoomAsync(int accountId, int? targetAccountId, int? targetStartupId);
        Task<List<UserMessage>> GetMessagesAsync(int chatRoomId, int pageNumber, int pageSize);
        Task SendMessageAsync(UserMessage message);
        Task<int> GetTotalMessagesAsync(int chatRoomId);
        Task<List<ChatRoomWithLatestMessageDto>> GetChatRoomsByAccountIdAsync(int accountId);
        Task<List<ChatRoomWithLatestMessageDto>> GetChatRoomsByStartupIdAsync(int startupId);
    }
}
