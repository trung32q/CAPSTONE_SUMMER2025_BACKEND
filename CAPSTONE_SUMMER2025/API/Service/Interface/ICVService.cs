using API.DTO.AccountDTO;
using API.DTO.PostDTO;

namespace API.Service.Interface
{
    public interface ICVService
    {
        Task<bool> ResponseCandidateCVAsync(int candidateCVId, string newStatus);
        Task<bool> ApplyCVAsync(ApplyCVRequestDTO dto);
        Task<PagedResult<CandidateCVResponseDTO>> GetCVsOfStartupAsync(int startupId, int positionId, int page, int pageSize);
        Task<bool> CheckSubmittedCVAsync(int accountId, int internshipId);
    }
}
