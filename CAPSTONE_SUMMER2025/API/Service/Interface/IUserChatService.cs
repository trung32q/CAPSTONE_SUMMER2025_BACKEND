using API.DTO.Mesage;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface IUserChatService
    {
        Task<int> EnsureChatRoomAsync(int accountId, int? targetAccountId, int? targetStartupId);
        Task<List<UserMessage>> GetMessagesAsync(int chatRoomId);
        Task SendMessageAsync(UserMessageDto dto);
    }
}
