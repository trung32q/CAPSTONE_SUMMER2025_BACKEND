using API.DTO.Admin;

namespace API.Repositories.Interfaces
{
    public interface IAdminRepository
    {

        //lấy sô lượng account active(verifile + unverified)
        Task<int> GetCountAccountActiveAsync();

        //lấy ra số lượng account dc tạo mới trong 7 ngày tính cả ngày hiện tại
        Task<AccountStatsResultDTO> GetAccountsCreatedLast7DaysAsync();

        //lấy sô lượng startup
        Task<int> GetStartupCountAsync();

        //lấy sô lượng account
        Task<int> GetCountAccountAsync();

        //lấy ra số lượng startup dc tạo mới trong 7 ngày tính cả ngày hiện tại
        Task<StartupStatsResultDTO> GetStartupsCreatedLast7DaysAsync();
        Task<GrowthStatsDtocs> GetUserGrowthRateAsync();
    }
}
