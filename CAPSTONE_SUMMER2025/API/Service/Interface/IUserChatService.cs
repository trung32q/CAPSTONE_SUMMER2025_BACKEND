using API.DTO.AccountDTO;
using API.DTO.Mesage;
using API.DTO.VideoCall;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface IUserChatService
    {
        Task<int> EnsureChatRoomAsync(int accountId, int? targetAccountId, int? targetStartupId);
        Task<PagedResult<GetUserMessageDTO>> GetMessagesAsync(int chatRoomId, int pageNumber, int pageSize);
        Task<UserMessage> SendMessageAsync(UserMessageDto dto);
        Task<List<ChatRoomWithLatestMessageDto>> GetChatRoomsByAccountAsync(int accountId);
        Task<List<ChatRoomWithLatestMessageDto>> GetChatRoomsByStartupAsync(int startupId);
        Task<ResStartCallDto> StartCallAsync(StartCallDto dto);
        Task EndCallAsync(Guid callSessionId);
        Task<List<UserCallSession>> GetCallHistoryAsync(int chatRoomId);
        Task AcceptCallAsync(Guid callSessionId);
        Task RejectCallAsync(Guid callSessionId);

    }
}
