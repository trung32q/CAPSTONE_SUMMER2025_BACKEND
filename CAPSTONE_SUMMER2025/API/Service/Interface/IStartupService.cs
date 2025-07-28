using API.DTO.AccountDTO;
using API.DTO.PostDTO;
using API.DTO.StartupDTO;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface IStartupService
    {
        Task<int> CreateStartupAsync(CreateStartupRequest request);
        Task<PagedResult<ResStartupDTO>> GetAllStartupsAsync(int pageNumber, int pageSize, int? categoryId = null);
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
        Task<ResInviteDto> CreateInviteAsync(CreateInviteDTO dto);
        Task<int?> GetStartupIdByAccountIdAsync(int accountId);
        Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
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
        Task<PagedResult<ResInviteDto>> GetInvitesByStartupIdPagedAsync(int startupId, int pageNumber, int pageSize);
        Task<bool> CanInviteAgainAsync(int accountId, int startupId);
        Task<ResInviteDto?> GetInviteByIdAsync(int inviteId);
        Task<bool> UpdateInviteAsync(int inviteId, string response);

        Task<PositionRequirementResponseDto> GetPositionRequirementByIdAsync(int id);
        Task<PagedResult<PositionRequirementResponseDto>> GetPositionRequirementPagedAsync(int startupId, int pageNumber, int pageSize);
        Task<PositionRequirementResponseDto> AddPositionRequirementAsync(PositionRequirementCreateDto dto); 
        Task<bool> UpdatePositionRequirementAsync(int id, PositionRequirementUpdateDto dto); 
        Task<bool> DeletePositionRequirementAsync(int id);
        Task<bool> SubscribeAsync(int accountId, int startupId);
        Task<bool> UnsubscribeAsync(int accountId, int startupId);
        Task<StartupDetailDTO?> GetStartupByIdAsync(int startupId);
        Task<bool> UpdateStartupAsync(int id, UpdateStartupDto dto);
        Task<PagedResult<CandidateCVResponseDTO>> GetCVsOfStartupAsync(int startupId,int positionId, int page, int pageSize);
        Task<PositionRequirementDto?> GetRequirementInfoAsync(int positionId);
        Task AddEvaluationAsync(CVRequirementEvaluationResultDto evaluation);
        Task<StartupPitching> AddStartupPitchingAsync(StartupPitchingCreateDTO dto);
        Task<List<GetStartupPitchingDTO>> GetPitchingsByTypeAndStartupAsync(int startupId, string type);
        Task<bool> DeleteStartupPitchingAsync(int pitchingId);
        Task<bool> UpdateStartupPitchingAsync(int startupPitchingId, IFormFile file);
        Task<bool> IsAccountSubcibeStartup(int accountId, int startupId);
    }
}
