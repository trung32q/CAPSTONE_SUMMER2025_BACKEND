using API.DTO.DartBoardDTO;

namespace API.Service.Interface
{
    public interface IDartboardService
    {
        Task<InteractionStatsResultDTO> GetStartupInteractionsDailyStatsAsync(int startupId);
        Task<SubscribeStatsResultDTO> GetSubcribesByDayLast7DaysAsync(int startupId);
        Task<ClickStatsResultDTO> GetStartupClicksByDayLast7DaysAsync(int startupId);
        Task<PostStatsResultDTO> GetStartupPostsByDayLast7DaysAsync(int startupId);
        Task<InternshipPostStatsResultDTO> GetStartupInternshipPostsByDayLast7DaysAsync(int startupId);
        Task<DartBoardResultDTO> GetStartupDartBoard(int startupId);

    }
}
