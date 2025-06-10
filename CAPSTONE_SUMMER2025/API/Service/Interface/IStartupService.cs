using API.DTO.StartupDTO;

namespace API.Service.Interface
{
    public interface IStartupService
    {
        Task<int> CreateStartupAsync(CreateStartupRequest request);
        Task<List<ResStartupDTO>> GetAllStartupsAsync();
        Task<bool> IsMemberOfAnyStartup(int accountId);
    }
}
