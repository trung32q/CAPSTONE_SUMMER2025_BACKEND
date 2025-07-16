using API.DTO.AccountDTO;
using API.DTO.PostDTO;
using Infrastructure.Models;

namespace API.Repositories.Interfaces
{
    public interface ICVRepository
    {
        Task<int> AddCandidateCvAsync(CandidateCv cv);
        Task SaveChangesAsync();
        Task<PagedResult<CandidateCVResponseDTO>> GetCandidateCVsByStartupIdAsync(
     int startupId, int positionId, int pageNumber, int pageSize);
        Task<CandidateCv?> GetCandidateCvWithRelationsAsync(int candidateCvId);
        Task<CandidateCv?> GetCandidateCVByIdAsync(int id);
        Task<bool> HasSubmittedCVAsync(int accountId, int internshipId);
    }
}
