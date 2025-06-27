using API.DTO.DartBoardDTO;
using API.Repositories.Interfaces;
using API.Service.Interface;

namespace API.Service
{
    public class DartboardService : IDartboardService
    {
        public readonly IDartboardRepository _repository;

        public DartboardService(IDartboardRepository repository)
        {
            _repository = repository;
        }

        // lấy ra số lượng tương tác với bài viết của starup trong 7 ngày
        public async Task<InteractionStatsResultDTO> GetStartupInteractionsDailyStatsAsync(int startupId)
        {
            return await _repository.GetStartupInteractionsByDayLast7DaysAsync(startupId);
        }

        public async Task<SubscribeStatsResultDTO> GetSubcribesByDayLast7DaysAsync(int startupId)
        {
            return await _repository.GetStartupSubcribesByDayLast7DaysAsync(startupId);
        }

        public async Task<ClickStatsResultDTO> GetStartupClicksByDayLast7DaysAsync(int startupId)
        {
            return await _repository.GetStartupClicksByDayLast7DaysAsync(startupId);
        }
        public async Task<PostStatsResultDTO> GetStartupPostsByDayLast7DaysAsync(int startupId)
        {
            return await _repository.GetStartupPostsByDayLast7DaysAsync(startupId);
        }
        public async Task<InternshipPostStatsResultDTO> GetStartupInternshipPostsByDayLast7DaysAsync(int startupId)
        {
            return await _repository.GetStartupInternshipPostsByDayLast7DaysAsync(startupId);
        }

        public async Task<DartBoardResultDTO> GetStartupDartBoard(int startupId)
        {
            return new DartBoardResultDTO
            {
                clickStatsResultDTO = await _repository.GetStartupClicksByDayLast7DaysAsync(startupId),
                subscribeStatsResultDTO = await _repository.GetStartupSubcribesByDayLast7DaysAsync(startupId),
                interactionStatsResultDTO = await _repository.GetStartupInteractionsByDayLast7DaysAsync(startupId),
                internshipPostStatsResultDTO = await _repository.GetStartupInternshipPostsByDayLast7DaysAsync(startupId),
                postStatsResultDTO = await _repository.GetStartupPostsByDayLast7DaysAsync(startupId)
            };
        }

    }
}
