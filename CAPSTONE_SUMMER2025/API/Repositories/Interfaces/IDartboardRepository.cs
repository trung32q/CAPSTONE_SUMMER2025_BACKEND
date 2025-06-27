using API.DTO.DartBoardDTO;

namespace API.Repositories.Interfaces
{
    public interface IDartboardRepository
    {
        Task<InteractionStatsResultDTO> GetStartupInteractionsByDayLast7DaysAsync(int startupId);
        Task<SubscribeStatsResultDTO> GetStartupSubcribesByDayLast7DaysAsync(int startupId);
        Task<ClickStatsResultDTO> GetStartupClicksByDayLast7DaysAsync(int startupId);
        Task<PostStatsResultDTO> GetStartupPostsByDayLast7DaysAsync(int startupId);
        Task<InternshipPostStatsResultDTO> GetStartupInternshipPostsByDayLast7DaysAsync(int startupId);
    }
}
