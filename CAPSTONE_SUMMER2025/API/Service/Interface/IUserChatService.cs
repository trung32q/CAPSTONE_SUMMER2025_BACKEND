using API.DTO.AccountDTO;
using API.DTO.Mesage;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface IUserChatService
    {
        Task<int> EnsureChatRoomAsync(int accountId, int? targetAccountId, int? targetStartupId);
        Task<PagedResult<GetUserMessageDTO>> GetMessagesAsync(int chatRoomId, int pageNumber, int pageSize);
        Task<ResUserMessageDTO> SendMessageAsync(UserMessageDto dto);
        Task<List<ChatRoomWithLatestMessageDto>> GetChatRoomsByAccountAsync(int accountId);
    }
}
