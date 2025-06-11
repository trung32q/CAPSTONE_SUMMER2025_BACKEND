using API.DTO.AccountDTO;
using API.DTO.StartupDTO;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface IStartupService
    {
        Task<int> CreateStartupAsync(CreateStartupRequest request);
        Task<List<ResStartupDTO>> GetAllStartupsAsync();

        Task<ChatRoom> CreateChatRoomAsync(CreateChatRoomDTO dto);
        Task AddMembersToChatRoomAsync(AddMembersDTO dto);
        Task<List<StartupMemberDTO>> GetMembersByStartupIdAsync(int startupId);
        Task<List<ChatRoomMemberDTO>> GetMembersByChatRoomIdAsync(int chatRoomId);
        Task<PagedResult<ChatRoomDTO>> GetChatRoomsByAccountIdAsync(int accountId, int pageNumber, int pageSize);
        Task SendMessageAsync(SendMessageRequest request);

        Task<PagedResult<ChatMessageDTO>> GetMessagesByRoomIdAsync(int chatRoomId, int pageNumber, int pageSize);

    }
}
