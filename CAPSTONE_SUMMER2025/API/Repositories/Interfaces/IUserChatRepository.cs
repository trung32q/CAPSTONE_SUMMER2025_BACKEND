using Infrastructure.Models;

namespace API.Repositories.Interfaces
{
    public interface IUserChatRepository
    {
        Task<UserChatRoom?> GetChatRoomAsync(int accountId, int? targetAccountId, int? targetStartupId);
        Task<UserChatRoom> CreateChatRoomAsync(int accountId, int? targetAccountId, int? targetStartupId);
        Task<List<UserMessage>> GetMessagesAsync(int chatRoomId);
        Task SendMessageAsync(UserMessage message);
    }
}
