using API.DTO.AccountDTO;
using API.DTO.PostDTO;
using API.DTO.StartupDTO;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface IStartupService
    {
        Task<int> CreateStartupAsync(CreateStartupRequest request);
        Task<PagedResult<ResStartupDTO>> GetAllStartupsAsync(int pageNumber, int pageSize);
        Task<bool> IsMemberOfAnyStartup(int accountId);
        Task<ChatRoom> CreateChatRoomAsync(CreateChatRoomDTO dto);
        Task AddMembersToChatRoomAsync(AddMembersDTO dto);
        Task<List<StartupMemberDTO>> GetMembersByStartupIdAsync(int startupId);
        Task<List<ChatRoomMemberDTO>> GetMembersByChatRoomIdAsync(int chatRoomId);
        Task<PagedResult<ChatRoomDTO>> GetChatRoomsByAccountIdAsync(int accountId, int pageNumber, int pageSize);
        Task<ChatMessageDTO> SendMessageAsync(SendMessageRequest request);
        Task<PagedResult<ChatMessageDTO>> GetMessagesByRoomIdAsync(int chatRoomId, int pageNumber, int pageSize);
        Task<List<StartupStage>> GetAllStagesAsync();
        Task<ChatMessageDTO?> GetMessageByIdAsync(int messageId);
        Task<bool> UpdateMemberTitleAsync(UpdateMemberTitleRequest request);
        Task<List<Account>> SearchByEmailAsync(string keyword);
        Task<Invite> CreateInviteAsync(CreateInviteDTO dto);
        Task<int?> GetStartupIdByAccountIdAsync(int accountId);
        Task<RoleInStartup> CreateRoleAsync(CreateRoleDto dto);
        Task<RoleInStartup?> GetRoleAsync(int roleId);
        Task<RoleInStartup> UpdateRoleAsync(UpdateRoleDto dto);
        Task<bool> DeleteRoleAsync(int roleId);
        Task<List<RoleInStartup>> GetRolesByStartupAsync(int startupId);
        Task<PagedResult<ChatMessageDTO>> SearchMessageAsync(int chatRoomId, int pageNumber, int pageSize, string searchKey);
        Task<List<StartupMemberDTO>> SearchAndFilterMembersAsync(int startupId, int? roleId, string? search);
        Task<bool> KickMemberAsync(int startupId, int accountId);
        Task<bool> OutStartupAsync(int accountId);
        Task<bool> UpdateMemberRoleAsync(int startupId, int accountId, int newRoleId);
        Task<bool> KickMembersAsync(KickChatRoomMembersRequestDTO dto);


    }
}
