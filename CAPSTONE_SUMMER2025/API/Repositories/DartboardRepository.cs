using API.DTO.DartBoardDTO;
using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class DartboardRepository : IDartboardRepository
    {
        private readonly CAPSTONE_SUMMER2025Context _context;

        public DartboardRepository(CAPSTONE_SUMMER2025Context context)
        {
            _context = context;
        }

        // lấy ra số lượng tương tác với bài viết của starup trong 7 ngày
        public async Task<InteractionStatsResultDTO> GetStartupInteractionsByDayLast7DaysAsync(int startupId)
        {
            DateTime today = DateTime.Now.Date;
            DateTime sevenDaysAgo = today.AddDays(-6); // Bao gồm hôm nay và 6 ngày trước

            var interactions = await _context.Posts
                .Where(p => p.StartupId == startupId)
                .SelectMany(p => p.PostComments
                        .Where(c => c.CommentAt.HasValue && c.CommentAt.Value.Date >= sevenDaysAgo)
                        .Select(c => new { Date = c.CommentAt.Value.Date })
                    .Concat(
                        p.PostLikes
                        .Where(l => l.LikedAt.HasValue && l.LikedAt.Value.Date >= sevenDaysAgo)
                        .Select(l => new { Date = l.LikedAt.Value.Date })
                    )
                )
                .GroupBy(x => x.Date)
                .Select(g => new DailyInteractionStatDTO
                {
                    Date = g.Key,
                    InteractionCount = g.Count()
                })
                .ToListAsync();


            // Đảm bảo có đủ 7 ngày
            var full7Days = Enumerable.Range(0, 7)
                .Select(i => sevenDaysAgo.AddDays(i))
                .Select(date => new DailyInteractionStatDTO
                {
                    Date = date,
                    InteractionCount = interactions.FirstOrDefault(x => x.Date == date)?.InteractionCount ?? 0
                })
                .OrderBy(x => x.Date)
                .ToList();

            var interactionCount = 0;
            foreach (var d in full7Days)
            {
                interactionCount += d.InteractionCount;
            }

            return new InteractionStatsResultDTO
            {
                TotalInteractionCount = interactionCount,
                DailyInteractionStats = full7Days
            };
        }

        public async Task<SubscribeStatsResultDTO> GetStartupSubcribesByDayLast7DaysAsync(int startupId)
        {
            DateTime today = DateTime.Now.Date;
            DateTime sevenDaysAgo = today.AddDays(-6); // Bao gồm hôm nay + 6 ngày trước

            var subscribes = await _context.Subcribes
                .Where(s => s.FollowingStartUpId == startupId &&
                            s.FollowDate >= sevenDaysAgo)
                .GroupBy(s => s.FollowDate.Value.Date) // CHỈ LẤY NGÀY
                .Select(g => new DailySubscribeStatDTO
                {
                    Date = g.Key,
                    SubscribeCount = g.Count()
                })
                .ToListAsync();


            // Đảm bảo đủ 7 ngày
            var full7Days = Enumerable.Range(0, 7)
                .Select(i => sevenDaysAgo.AddDays(i))
                .Select(date => new DailySubscribeStatDTO
                {
                    Date = date,
                    SubscribeCount = subscribes.FirstOrDefault(x => x.Date == date)?.SubscribeCount ?? 0
                })
                .OrderBy(x => x.Date)
                .ToList();

            var subcribeCount = 0;
            foreach (var subcribe in subscribes)
            {
                subcribeCount += subcribe.SubscribeCount;
            }

            return new SubscribeStatsResultDTO
            {
                TotalSubcribeCount = subcribeCount,
                DailySubcribeStats = full7Days
            };
        }

        public async Task<ClickStatsResultDTO> GetStartupClicksByDayLast7DaysAsync(int startupId)
        {
            DateTime today = DateTime.Now.Date;
            DateTime sevenDaysAgo = today.AddDays(-6); // Bao gồm hôm nay + 6 ngày trước

            var clicks = await _context.StartupClicks
                .Where(c => c.StartupId == startupId &&
                            c.DateClick.HasValue &&
                            c.DateClick.Value.Date >= sevenDaysAgo)
                .GroupBy(c => c.DateClick.Value.Date)
                .Select(g => new DailyClickStatDTO
                {
                    Date = g.Key,
                    ClickCount = g.Count()
                })
                .ToListAsync();

            // Đảm bảo đủ 7 ngày
            var full7Days = Enumerable.Range(0, 7)
                .Select(i => sevenDaysAgo.AddDays(i))
                .Select(date => new DailyClickStatDTO
                {
                    Date = date,
                    ClickCount = clicks.FirstOrDefault(x => x.Date == date)?.ClickCount ?? 0
                })
                .OrderBy(x => x.Date)
                .ToList();

            return new ClickStatsResultDTO
            {
                TotalClickCount = full7Days.Sum(x => x.ClickCount),
                DailyClickStats = full7Days
            };

        }


        public async Task<PostStatsResultDTO> GetStartupPostsByDayLast7DaysAsync(int startupId)
        {
            DateTime today = DateTime.Now.Date;
            DateTime sevenDaysAgo = today.AddDays(-6); // Hôm nay + 6 ngày trước

            var posts = await _context.Posts
                .Where(p => p.StartupId == startupId &&
                            p.CreateAt.HasValue &&
                            p.CreateAt.Value.Date >= sevenDaysAgo)
                .GroupBy(p => p.CreateAt.Value.Date)
                .Select(g => new DailyPostStatDTO
                {
                    Date = g.Key,
                    PostCount = g.Count()
                })
                .ToListAsync();

            // Đảm bảo đủ 7 ngày
            var full7Days = Enumerable.Range(0, 7)
                .Select(i => sevenDaysAgo.AddDays(i))
                .Select(date => new DailyPostStatDTO
                {
                    Date = date,
                    PostCount = posts.FirstOrDefault(x => x.Date == date)?.PostCount ?? 0
                })
                .OrderBy(x => x.Date)
                .ToList();

            return new PostStatsResultDTO
            {
                TotalPostCount = full7Days.Sum(x => x.PostCount),
                DailyPostStats = full7Days
            };
        }

        public async Task<InternshipPostStatsResultDTO> GetStartupInternshipPostsByDayLast7DaysAsync(int startupId)
        {
            DateTime today = DateTime.Now.Date;
            DateTime sevenDaysAgo = today.AddDays(-6); // Bao gồm hôm nay + 6 ngày trước

            var posts = await _context.InternshipPosts
                .Where(p => p.StartupId == startupId &&
                            p.CreateAt.HasValue &&
                            p.CreateAt.Value.Date >= sevenDaysAgo)
                .GroupBy(p => p.CreateAt.Value.Date)
                .Select(g => new DailyInternshipPostStatDTO
                {
                    Date = g.Key,
                    PostCount = g.Count()
                })
                .ToListAsync();

            // Đảm bảo đủ 7 ngày
            var full7Days = Enumerable.Range(0, 7)
                .Select(i => sevenDaysAgo.AddDays(i))
                .Select(date => new DailyInternshipPostStatDTO
                {
                    Date = date,
                    PostCount = posts.FirstOrDefault(x => x.Date == date)?.PostCount ?? 0
                })
                .OrderBy(x => x.Date)
                .ToList();

            return new InternshipPostStatsResultDTO
            {
                TotalInternshipPostCount = full7Days.Sum(x => x.PostCount),
                DailyInternshipPostStats = full7Days
            };
        }

    }
}
