using API.DTO.Admin;
using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace API.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly CAPSTONE_SUMMER2025Context _context;

        public AdminRepository(CAPSTONE_SUMMER2025Context context) {
                _context = context;
        }

        //lấy ra số lượng account active
        public async Task<int> GetCountAccountActiveAsync()
        {
            return await _context.Accounts.Where(a => a.Status == Utils.Constants.AccountStatusConst.VERIFIED || a.Status == Utils.Constants.AccountStatusConst.UNVERIFIED).CountAsync();
        }

        //lấy ra số lượng account
        public async Task<int> GetCountAccountAsync()
        {
            return await _context.Accounts.CountAsync();
        }

        //lấy sô lượng startup
        public async Task<int> GetStartupCountAsync()
        {
            return await _context.Startups.CountAsync();
        }

        //lấy ra số lượng account dc tạo mới trong 7 ngày tính cả ngày hiện tại
        public async Task<AccountStatsResultDTO> GetAccountsCreatedLast7DaysAsync()
        {
            DateTime today = DateTime.Now.Date;
            DateTime sevenDaysAgo = today.AddDays(-6); // Tính cả hôm nay và 6 ngày trước

            var accounts = await _context.Accounts
                .Where(a => a.CreatedAt.Date >= sevenDaysAgo)
                .GroupBy(a => a.CreatedAt.Date)
                .Select(g => new DailyAccountStatDTO
                {
                    Date = g.Key,
                    AccountCount = g.Count()
                })
                .ToListAsync();

            // Đảm bảo đủ 7 ngày liên tiếp
            var full7Days = Enumerable.Range(0, 7)
                .Select(i => sevenDaysAgo.AddDays(i))
                .Select(date => new DailyAccountStatDTO
                {
                    Date = date,
                    AccountCount = accounts.FirstOrDefault(x => x.Date == date)?.AccountCount ?? 0
                })
                .OrderBy(x => x.Date)
                .ToList();

            return new AccountStatsResultDTO
            {
                TotalNewAccountCount = full7Days.Sum(x => x.AccountCount),
                DailyAccountStats = full7Days
            };
        }

        //lấy ra số lượng startup dc tạo mới trong 7 ngày tính cả ngày hiện tại
        public async Task<StartupStatsResultDTO> GetStartupsCreatedLast7DaysAsync()
        {
            DateTime today = DateTime.Now.Date;
            DateTime sevenDaysAgo = today.AddDays(-6); // Tính cả hôm nay + 6 ngày trước

            var startups = await _context.Startups
                .Where(s => s.CreateAt.HasValue && s.CreateAt.Value.Date >= sevenDaysAgo)
                .GroupBy(s => s.CreateAt.Value.Date)
                .Select(g => new DailyStartupStatDTO
                {
                    Date = g.Key,
                    StartupCount = g.Count()
                })
                    .ToListAsync();


            // Bảo đảm đủ 7 ngày liên tiếp
            var full7Days = Enumerable.Range(0, 7)
                .Select(i => sevenDaysAgo.AddDays(i))
                .Select(date => new DailyStartupStatDTO
                {
                    Date = date,
                    StartupCount = startups.FirstOrDefault(x => x.Date == date)?.StartupCount ?? 0
                })
                .OrderBy(x => x.Date)
                .ToList();

            return new StartupStatsResultDTO
            {
                TotalNewStartupCount = full7Days.Sum(x => x.StartupCount),
                DailyStartupStats = full7Days
            };
        }
        public async Task<GrowthStatsDtocs> GetUserGrowthRateAsync()
        {
            var today = DateTime.Today;
            var startOfThisWeek = today.AddDays(-(int)today.DayOfWeek + 1); // Thứ 2 tuần này
            var startOfLastWeek = startOfThisWeek.AddDays(-7);              // Thứ 2 tuần trước
            var endOfLastWeek = startOfThisWeek.AddDays(-1);                // Chủ nhật tuần trước

            int lastWeekCount = await _context.Accounts
                .Where(a =>
                    a.CreatedAt.Date >= startOfLastWeek &&
                    a.CreatedAt.Date <= endOfLastWeek)
                .CountAsync();

            int thisWeekCount = await _context.Accounts
                .Where(a =>
                    a.CreatedAt.Date >= startOfThisWeek)
                .CountAsync();

            int growthPercent = 0;
            if (lastWeekCount > 0)
            {
                growthPercent = (int)Math.Round((thisWeekCount - lastWeekCount) * 100.0 / lastWeekCount);
            }
            else if (thisWeekCount > 0)
            {
                growthPercent = 100;
            }

            return new GrowthStatsDtocs
            {
                ThisWeek = thisWeekCount,
                LastWeek = lastWeekCount,
                GrowthPercent = growthPercent
            };
        }



    }
}
