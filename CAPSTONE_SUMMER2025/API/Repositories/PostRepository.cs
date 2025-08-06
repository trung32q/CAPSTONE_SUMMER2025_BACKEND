using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using API.DTO.PostDTO;
using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using API.DTO.AccountDTO;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Utilities;
using System.Globalization;
using System.Text;
using API.Utils.Constants;

namespace API.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly IFilebaseHandler _filebaseHandler;

        public PostRepository(IFilebaseHandler filebaseHandler, CAPSTONE_SUMMER2025Context context)
        {
            _context = context;
            _filebaseHandler = filebaseHandler;
        }

        // --- HÀM TÁI SỬ DỤNG ---
        private async Task<bool> IsPostExistsAsync(int postId) => await _context.Posts.AnyAsync(p => p.PostId == postId);
        private async Task<bool> IsAccountExistsAsync(int accountId) => await _context.Accounts.AnyAsync(a => a.AccountId == accountId);

        //hàm tạo comment
        public async Task<bool> CreatePostComment(reqPostCommentDTO reqPostCommentDTO)
        {
            if (!await IsAccountExistsAsync(reqPostCommentDTO.AccountId) || !await IsPostExistsAsync(reqPostCommentDTO.PostId))
                return false;

            await _context.PostComments.AddAsync(new PostComment
            {
                AccountId = reqPostCommentDTO.AccountId,
                PostId = reqPostCommentDTO.PostId,
                Content = reqPostCommentDTO.Content,
                ParentCommentId = reqPostCommentDTO.ParentCommentId,
            });

            await _context.SaveChangesAsync();
            return true;
        }

        //hàm cập nhật post
        public async Task<bool> UpdatePostAsync(int postId, string title, string content)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return false;

            post.Title = title;
            post.Content = content;

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
            return true;
        }

        //hàm tìm bài post theo id
        public async Task<Post> GetPostByPostIdAsync(int id)
        {
            return await _context.Posts.Include(p => p.Startup).Include(p => p.PostMedia).Include(p => p.Account).ThenInclude(a => a.AccountProfile).FirstOrDefaultAsync(x => x.PostId == id);
        }



        //hàm xóa comment và comment con
        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var comment = await _context.PostComments.FindAsync(commentId);
            if (comment == null) return false;

            var childComments = await _context.PostComments.Where(c => c.ParentCommentId == commentId).ToListAsync();

            _context.PostComments.RemoveRange(childComments);
            _context.PostComments.Remove(comment);

            await _context.SaveChangesAsync();
            return true;
        }

        //hàm sửa comment
        public async Task<bool> UpdateCommentAsync(int commentId, string newContent)
        {
            var comment = await _context.PostComments.FindAsync(commentId);
            if (comment == null) return false;

            comment.Content = newContent;
            _context.PostComments.Update(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        //hàm like bài viết
        public async Task<bool> LikePostAsync(int postId, int accountId)
        {
            if (await IsPostLikedAsync(postId, accountId))
                return false;

            var like = new PostLike { PostId = postId, AccountId = accountId };
            _context.PostLikes.Add(like);
            return await _context.SaveChangesAsync() > 0;
        }

        //hàm hủy like
        public async Task<bool> UnlikePostAsync(int postId, int accountId)
        {
            var existing = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.AccountId == accountId);

            if (existing == null) return false;

            _context.PostLikes.Remove(existing);
            return await _context.SaveChangesAsync() > 0;
        }

        //hàm lấy số lượng like của 1 bài viết
        public async Task<int> GetPostLikeCountAsync(int postId)
        {
            return await _context.PostLikes.CountAsync(pl => pl.PostId == postId);
        }

        // hàm tính số lượng comment
        public async Task<int> GetPostCommentCountAsync(int postId)
        {
            return await _context.PostComments.CountAsync(pl => pl.PostId == postId);
        }

        //hàm check xem account đã like bài viết chưa
        public async Task<bool> IsPostLikedAsync(int postId, int accountId)
        {
            return await _context.PostLikes.AnyAsync(pl => pl.PostId == postId && pl.AccountId == accountId);
        }

      

        // hàm lấy ra danh sách các comment theo postid
        public async Task<PagedResult<PostComment>> GetPostCommentByPostId(int postId, int pageNumber, int pageSize)
        {
            if (!await IsPostExistsAsync(postId))
                return null;

            var querry = _context.PostComments
                .Where(pc => pc.PostId == postId && pc.ParentCommentId == null);

            var totalCount = await querry.CountAsync();
            var items = await querry.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<PostComment>(items, totalCount, pageNumber, pageSize);
        }

        // hàm lấy ra comments theo parrentCommentId
        public async Task<PagedResult<PostComment>> GetPostCommentChildByPostIdAndParentCommentId(int pageNumber, int pageSize, int parrentCommentId)
        {
            var query = _context.PostComments.Where(pc => pc.ParentCommentId == parrentCommentId);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<PostComment>(items, totalCount, pageNumber, pageSize);
        }

      


        //hàm lấy ra các bài post theo accountid
        public async Task<PagedResult<Post>> GetPostsByAccountId(int accountId,int pageNumber, int pageSize, int currentAccountId)
        {
            if (!await IsAccountExistsAsync(accountId))
                return null;

            // Lấy danh sách PostId bị ẩn bởi currentAccountId
            var hiddenPostIds = await _context.PostHides
                .Where(h => h.AccountId == currentAccountId)
                .Select(h => h.PostId)
                .ToListAsync();

            // Lấy các bài post của accountId, loại các bài đã bị ẩn với currentAccountId
            var query = _context.Posts
                .Where(p => p.AccountId == accountId && !hiddenPostIds.Contains(p.PostId))
                .Include(p => p.PostMedia)
                .OrderByDescending(p => p.CreateAt);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Post>(items, totalCount, pageNumber, pageSize);
        }


        //hàm tính số comment reply 1 comment
        public async Task<int> CountChildCommentByPostCommentId(int? id)
        {
            if (id == null) return 0;
            return await _context.PostComments.Where(p => p.ParentCommentId == id).CountAsync();
        }

        // Upload post
        public async Task<bool> CreatePost(ReqPostDTO reqPostDTO)
        {
            var post = new Post();

            if(reqPostDTO.AccountId != null)
            {
                post = new Post
                {
                    AccountId = reqPostDTO.AccountId.Value,
                    Content = reqPostDTO.Content,
                    Title = reqPostDTO.Title,
                };
            }
            else if(reqPostDTO.StartupId != null)
            {
                post = new Post
                {
                    StartupId = reqPostDTO.StartupId.Value,
                    Content = reqPostDTO.Content,
                    Title = reqPostDTO.Title,
                };
            }

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            var postMedias = new List<PostMedium>();
            int displayOrder = 0;

            if (reqPostDTO.MediaFiles != null && reqPostDTO.MediaFiles.Any())
            {
                foreach (var file in reqPostDTO.MediaFiles)
                {
                    var fileUrl = await _filebaseHandler.UploadMediaFile(file);

                    postMedias.Add(new PostMedium
                    {
                        PostId = post.PostId,
                        MediaUrl = fileUrl,
                        DisplayOrder = displayOrder++,
                    });
                }
            }

            await _context.PostMedia.AddRangeAsync(postMedias);
            await _context.SaveChangesAsync();

            return true;
        }

        // xóa bài post
        public async Task<bool> DeletePostAsync(int postId)
        {
            var post = await _context.Posts
                .Include(p => p.PostMedia)
                .Include(p => p.PostComments)
                .Include(p => p.PostLikes)
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post == null) return false;

            // B1: Xóa tất cả likes
            var likes = await _context.PostLikes.Where(l => l.PostId == postId).ToListAsync();
            _context.PostLikes.RemoveRange(likes);

            // B2: Xóa tất cả comments (bao gồm comment cha và con)
            var comments = await _context.PostComments.Where(c => c.PostId == postId).ToListAsync();
            _context.PostComments.RemoveRange(comments);

            // B3: Xóa tất cả media trên cloud và trong DB
            foreach (var media in post.PostMedia)
            {
                // Gọi xóa file từ Cloudinary
                await _filebaseHandler.DeleteFileByUrlAsync(media.MediaUrl);
            }

            _context.PostMedia.RemoveRange(post.PostMedia);

            // B4: Xóa bài post
            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();
            return true;
        }

        // tìm kiếm post
        public IQueryable<resPostDTO> GetSearchPosts(string keyword, int currentUserId)
        {
            keyword = RemoveDiacritics(keyword).ToLower();

            // Danh sách ID post bị ẩn bởi người dùng hiện tại
            var hiddenPostIds = _context.PostHides
                .Where(h => h.AccountId == currentUserId)
                .Select(h => h.PostId)
                .ToList();

            // Bước 1: Lọc post có nội dung, KHÔNG bị ẩn
            var filteredPosts = _context.Posts
                .Include(p => p.PostMedia)
                .Include(p => p.Account)
                .ThenInclude(a => a.AccountProfile)
                .Where(p =>
                    (!string.IsNullOrEmpty(p.Title) || !string.IsNullOrEmpty(p.Content)) &&
                    !hiddenPostIds.Contains(p.PostId) // loại post bị ẩn
                )
                .ToList()
                .Where(p =>
                    (!string.IsNullOrEmpty(p.Title) && RemoveDiacritics(p.Title).ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(p.Content) && RemoveDiacritics(p.Content).ToLower().Contains(keyword))
                );

            // Bước 2: Mapping sang DTO
            var result = filteredPosts
                .Select(p => new resPostDTO
                {
                    PostId = p.PostId,
                    AccountId = p.AccountId,
                    Title = p.Title,
                    Content = p.Content,
                    CreateAt = p.CreateAt,
                    LikeCount = _context.PostLikes.Count(l => l.PostId == p.PostId),
                    FullName = p.Account.AccountProfile.FirstName + " " + p.Account.AccountProfile.LastName,
                    AvatarUrl = p.Account.AccountProfile.AvatarUrl,
                    PostMedia = p.PostMedia.Select(m => new PostMediaDTO
                    {
                        MediaUrl = m.MediaUrl,
                        DisplayOrder = m.DisplayOrder
                    }).ToList()
                }).AsQueryable();

            return result;
        }

        // tìm kiếm post của startup
        public IQueryable<Post> GetSearchPostsByStartup(int startupId)
        {
            return _context.Posts
                .Include(p => p.Startup)
                .Include(p => p.PostMedia)
                .Where(p => p.StartupId == startupId)
                .OrderByDescending(p => p.CreateAt);
        }

        // lấy internshippost của startup
        public IQueryable<InternshipPost> GetStartupInternshipPost(int startupId)
        {
            return _context.InternshipPosts
                .Include(p => p.Startup)
                .Include(p => p.Position)
                .Where(p => p.StartupId == startupId)
                .OrderByDescending(p => p.CreateAt);
        }

        // hàm xóa dấu
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var normalized = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    builder.Append(c);
            }
            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        public async Task<int?> GetAccountIdByPostIDAsync(int postId)
        {
            try
            {
                var accountId = await _context.Posts
                    .Where(p => p.PostId == postId)
                    .Select(p => (int?)p.AccountId) 
                    .FirstOrDefaultAsync();
                return accountId;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Lỗi khi lấy AccountId của bài viết có ID {postId}", ex);
            }
        }

        
        public async Task<int?> GetAccountIdByCommentId(int commentId)
        {
            try
            {
                var accountId = await _context.PostComments
                    .Where(p => p.PostcommentId == commentId)
                    .Select(p => (int?)p.AccountId)
                    .FirstOrDefaultAsync();
                return accountId;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Lỗi khi lấy AccountId của comment có ID {commentId}", ex);
            }
        }
        // lấy ra feed của startup
        public async Task<(List<FeedItemDTO> Items, int TotalCount)> GetStartupFeedItemsAsync(int startupId, int skip, int take)
        {
            var postsQuery = _context.Posts
                .Where(p => p.StartupId == startupId)
                .Include(p => p.Startup)
                .Include(p => p.PostMedia);

            var internshipsQuery = _context.InternshipPosts
                .Where(i => i.StartupId == startupId && i.Status == StatusInternshipPost.ACTIVE)
                .Include(i => i.Position)
                .Include(i => i.Startup);

            var postItems = await postsQuery.Select(p => new FeedItemDTO
            {
                PostId = p.PostId,
                StartupId = p.StartupId,
                name = p.Startup.StartupName,
                AvatarURL = p.Startup.Logo,
                PostShareId = p.PostShareId,
                Type = "Post",
                Title = p.Title,
                Content = p.Content,
                CreatedAt = p.CreateAt ?? DateTime.MinValue,
                PostMedia = p.PostMedia.Select(m => new PostMediaDTO
                {
                    MediaUrl = _filebaseHandler.GeneratePreSignedUrl(m.MediaUrl),
                    DisplayOrder = m.DisplayOrder
                }).ToList(),
                InteractionCount = (p.PostLikes.Count() + p.PostComments.Count()),
                Priority = 1,
                LikeCount = (p.PostLikes.Count())
            }).ToListAsync();

            var internshipItems = await internshipsQuery.Select(i => new FeedItemDTO
            {
                PostId = i.InternshipId,
                StartupId = i.StartupId,
                name = i.Startup.StartupName,
                AvatarURL = i.Startup.Logo,
                Type = "Internship",
                Title = i.Position.Title,
                Content = $"{i.Description}",
                CreatedAt = i.CreateAt ?? DateTime.MinValue,
                PostMedia = new List<PostMediaDTO>(),
                InteractionCount = 0,
                Priority = 2,
                Address = i.Address,
                DueDate = i.Deadline
            }).ToListAsync();

            var combined = postItems
                .Concat(internshipItems)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            var pagedItems = combined.Skip(skip).Take(take).ToList();

            await IncreaseStartupClick(startupId);

            return (pagedItems, combined.Count);
        }

        // tăng lượt tiếp cận khi startup được gọi
        private async Task IncreaseStartupClick(int startupId)
        {
            _context.StartupClicks.Add(new StartupClick
            {
                StartupId = startupId,
            });
            await SaveChangesAsync();
        }


        public async Task<List<FeedItemDTO>> GetRecommendedFeedAsync(int userId, int page, int pageSize)
        {          
            // Lấy danh sách ID của các tài khoản mà người dùng đang theo dõi
            var followingIds = await _context.Follows
                .Where(f => f.FollowerAccountId == userId)
                .Select(f => f.FollowingAccountId)
                .ToListAsync();
            // Lấy danh sách PostId bị ẩn bởi currentAccountId
            var hiddenPostIds = await _context.PostHides
                .Where(h => h.AccountId == userId)
                .Select(h => h.PostId)
                .ToListAsync();
            // Lấy danh sách ID của các tài khoản đã bị người dùng chặn HOẶC đã chặn người dùng
            var blockedIds = await _context.AccountBlocks
                .Where(b => b.BlockerAccountId == userId || b.BlockedAccountId == userId)
                .Select(b => b.BlockerAccountId == userId ? b.BlockedAccountId : b.BlockerAccountId)
                .ToListAsync();

            // Lấy danh sách ID của các startup mà người dùng đã theo dõi (đăng ký)
            var followedStartupIds = await _context.Subcribes
                .Where(s => s.FollowerAccountId == userId)
                .Select(s => s.FollowingStartUpId)
                .ToListAsync();
            // thời gian tạo trong vòng 7 ngày
            var newAccountCutoffDate = DateTime.UtcNow.AddDays(-7);
            // Lấy các bài đăng dựa trên các tiêu chí đã định nghĩa
            var posts = await _context.Posts
                .Where(p =>!blockedIds.Contains(p.AccountId)&&
                    // Loại trừ bài đăng bị ẩn bởi user hiện tại
                     !hiddenPostIds.Contains(p.PostId))
                .Include(p => p.PostLikes)
                .Include(p => p.PostComments)
                .Include(p => p.PostMedia)
                .Select(p => new FeedItemDTO
                {
                    PostId = p.PostId,
                    AccountID = p.AccountId,
                    PostShareId = p.PostShareId,
                    name = p.AccountId != null
            ? (p.Account.AccountProfile.FirstName + " " + p.Account.AccountProfile.LastName)
            : p.Startup.StartupName,
                    AvatarURL = p.AccountId != null
            ? p.Account.AccountProfile.AvatarUrl
            : p.Startup.Logo,
                    Type = p.AccountId != null ? "Post" : "StartupPost",
                    Title = p.Title,
                    Content = p.Content,
                    CreatedAt = (DateTime)p.CreateAt,
                    StartupId = p.StartupId,
                    PostMedia = p.PostMedia != null

                ? p.PostMedia.Select(pm => new PostMediaDTO
                {
                    MediaUrl = pm.MediaUrl,
                    DisplayOrder = pm.DisplayOrder
                }).ToList()
                : new List<PostMediaDTO>(),
                    InteractionCount = (p.PostLikes.Count() + p.PostComments.Count()),
                    Priority = p.AccountId == userId ? 1
                  : followingIds.Contains(p.AccountId) ? 2
                  : (p.StartupId != null && followedStartupIds.Contains(p.StartupId.Value)) ? 3
                  : 4,
                   
                })
                .ToListAsync();

            // Xử lý PostMedia để tạo URL được ký trước (pre-signed URLs)
            foreach (var dto in posts)
            {
                foreach (var media in dto.PostMedia)
                {
                    if (!string.IsNullOrEmpty(media.MediaUrl))
                    {
                       
                        var publicIdWithType = media.MediaUrl.Contains("/")
                            ? media.MediaUrl
                            : $"image/{media.MediaUrl}";

                        Console.WriteLine($"Generating URL for publicIdWithType: {publicIdWithType}");

                        media.MediaUrl = _filebaseHandler.GeneratePreSignedUrl(publicIdWithType);
                    }
                }
            }

            // Lấy các bài đăng tuyển thực tập có trạng thái "Open"
            var internships = await _context.InternshipPosts
                .Where(i => i.Status == StatusInternshipPost.ACTIVE)
                .Select(i => new FeedItemDTO
                {
                    PostId = i.InternshipId,
                    StartupId = i.StartupId,
                    name=i.Startup.StartupName,
                    AvatarURL =i.Startup.Logo,
                    Type = "Internship",
                    Title = i.Position.Title,
                    Content = i.Description,
                    CreatedAt = (DateTime)i.CreateAt,
                    Priority=3,
                    Address = i.Address,
                    DueDate = i.Deadline
                })
                .ToListAsync();

            // Kết hợp các bài đăng và thông báo thực tập, sắp xếp theo ngày tạo giảm dần và áp dụng phân trang
            var allFeedItems = posts.Concat(internships)
            .OrderByDescending(x => x.CreatedAt) // Bài mới lên trước, bất kể loại gì
            .ThenBy(x => x.Priority)             // Ưu tiên cao lên trước nếu cùng thời gian
            .ThenByDescending(x => x.InteractionCount) // Bài có nhiều tương tác hơn ưu tiên
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
            return allFeedItems;
        }

        // hàm ẩn bài post
        public async Task<bool> HidePostAsync(int accountId, int postId)
        {
            var alreadyHidden = await IsPostHiddenAsync(accountId, postId);

            if (alreadyHidden)
                return false;

            var entity = new PostHide
            {
                AccountId = accountId,
                PostId = postId,
                HideAt = DateTime.UtcNow
            };

            _context.PostHides.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // hàm check xem bài post có bị ẩn đi không
        public async Task<bool> IsPostHiddenAsync(int accountId, int postId)
        {
            return await _context.PostHides
                .AnyAsync(ph => ph.AccountId == accountId && ph.PostId == postId);
        }


        // hàm lấy ra tất cả ReportReason
        public async Task<List<ReportReason>> GetAllReportReasonAsync()
        {
            return await _context.ReportReasons.ToListAsync();
        }

        // hàm lấy ra RepostReason theo id
        public async Task<ReportReason?> GetReportReasonByIdAsync(int id)
        {
            return await _context.ReportReasons.FindAsync(id);
        }

        // hàm tạo mới  ReportReason
        public async Task<ReportReason> CreateReportReasonAsync(ReportReason reason)
        {
            _context.ReportReasons.Add(reason);
            await _context.SaveChangesAsync();
            return reason;
        }


        // hàm cập nhật ReportReason
        public async Task<bool> UpdateReportReasonAsync(ReportReason reason)
        {
            var existing = await _context.ReportReasons.FindAsync(reason.ReasonId);
            if (existing == null) return false;

            existing.Reason = reason.Reason;
            existing.Description = reason.Description;
            await _context.SaveChangesAsync();
            return true;
        }

        //hàm xóa ReportReason
        public async Task<bool> DeleteReportReasonAsync(int id)
        {
            var reason = await _context.ReportReasons.FindAsync(id);
            if (reason == null) return false;

            _context.ReportReasons.Remove(reason);
            await _context.SaveChangesAsync();
            return true;
        }

        // hàm tạo post report
        public async Task<PostReport> CreatePostReportAsync(PostReport report)
        {
            _context.PostReports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        // hàm lấy postreport theo id
        public async Task<PostReport?> GetPostReportByIdAsync(int reportId)
        {
            return await _context.PostReports
          .Include(r => r.Account)
          .Include(r => r.Post)
          .Include(r => r.Reason)
          .FirstOrDefaultAsync(r => r.ReportId == reportId);
        }

        // tạo mới internshippost
        public async Task AddInternshipPostAsync(InternshipPost post)
        {
            await _context.InternshipPosts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        // hàm sharepost
        public async Task ShareAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

        }

        

        // lấy ra những bài post được hẹn nhưng chưa đăng
        public async Task<List<Post>> GetScheduledPostsAsync()
        {
            return await _context.Posts
                .Where(p => p.Schedule != null && p.Schedule != p.CreateAt)
                .ToListAsync();
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // hàm lấy internshippost theo id
        public async Task<InternshipPost> GetByInternshipPostIdAsync(int id)
        {
            return await _context.InternshipPosts.FindAsync(id);
        }

        //cập nhật InternShipPost
        public async Task UpdateInternshipPostAsync(InternshipPost post)
        {
            _context.InternshipPosts.Update(post);
        }

        //lấy ra internshippost theo id
        public async Task<InternshipPost> GetInternshipPostByIdAsync(int id)
        {
            return await _context.InternshipPosts.FindAsync(id);
        }

      



        // hàm lấy ra danh sách thông tin của người like bài post
        public async Task<PagedResult<PostLike>> GetPostLikeByPostId(int postId, int pageNumber, int pageSize)
        {
            if (!await IsPostExistsAsync(postId))
                return null;

            var query = _context.PostLikes
                .Where(pl => pl.PostId == postId)
                .Include(pl => pl.Account)
                    .ThenInclude(a => a.AccountProfile);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<PostLike>(items, totalCount, pageNumber, pageSize);
        }
        public async Task<PagedResult<Post>> GetPostsByStartupId(int startupId, int pageNumber, int pageSize)
        {
            var query = _context.Posts
                .Include(p => p.Account).ThenInclude(a => a.AccountProfile)
                .Include(p => p.PostMedia)
                .Include(p => p.PostLikes)
                .Where(p => p.StartupId == startupId);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreateAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Post>(items, totalCount, pageNumber, pageSize);
        }
        public async Task<Startup?> GetStartupByIdAsync(int startupId)
        {
            return await _context.Startups
                .Include(s => s.Stage)
                .FirstOrDefaultAsync(s => s.StartupId == startupId);
        }
        public async Task<PagedResult<InternshipPost>> GetInternshipPostsAsync(int pageNumber, int pageSize,int startupid)
        {
            var query = _context.InternshipPosts.Include(x=>x.Position).Where(x=>x.StartupId==startupid).AsQueryable();

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.CreateAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<InternshipPost>(items, totalCount, pageNumber, pageSize);
        }

        //lấy ra internshippost theo id
        public async Task<InternshipPost?> GetInternshipPostWithNavigationAsync(int internshipPostId)
        {
            return await _context.InternshipPosts
                .Include(p => p.Startup)
                .Include(p => p.Position)
                .FirstOrDefaultAsync(p => p.InternshipId == internshipPostId);
        }


        public async Task<bool> UpdateInternshipPostAsync(int internhsipId, InternshipPost internshipPost)
        {
            var post = await _context.InternshipPosts.FindAsync(internhsipId);
            if (post == null)
                return false;

            post.Description = internshipPost.Description;
            post.Requirement = internshipPost.Requirement;
            post.Benefits = internshipPost.Benefits;
            post.Address = internshipPost.Address;
            post.Salary = internshipPost.Salary;
            post.Deadline = internshipPost.Deadline;

            await _context.SaveChangesAsync();
            return true;
        }

        public void UpdateInternshipPost(InternshipPost post)
        {
            _context.InternshipPosts.Update(post);
        }

        public async Task<List<TopInternshipPostDTO>> GetTopInternshipPostsByCVCountAsync(int top = 5)
        {
            var query = await _context.CandidateCvs
                .GroupBy(cv => cv.InternshipId)
                .Select(g => new
                {
                    InternshipId = g.Key,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(top)
                .Join(_context.InternshipPosts,
                      g => g.InternshipId,
                      ip => ip.InternshipId,
                      (g, ip) => new { g.Total, Internship = ip })
                .Join(_context.PositionRequirements,
                      ipg => ipg.Internship.PositionId,
                      pr => pr.PositionId,
                      (ipg, pr) => new { ipg.Total, ipg.Internship, Position = pr })
                .Join(_context.Startups,
                      all => all.Internship.StartupId,
                      st => st.StartupId,
                      (all, st) => new TopInternshipPostDTO
                      {
                          InternshipId = all.Internship.InternshipId,
                          Description = all.Internship.Description,
                          Address = all.Internship.Address,
                          Salary = all.Internship.Salary,
                          TotalCVs = all.Total,
                          PositionTitle = all.Position.Title,
                          StartupName = st.StartupName,
                          Logo = st.Logo,
                          Requirement = all.Internship.Requirement,
                          CreateAt = all.Internship.CreateAt,
                          Deadline = all.Internship.Deadline,
                          Status = all.Internship.Status,
                          StartupId = st.StartupId,
                          PositionId = all.Position.PositionId,
                          Benefits = all.Internship.Benefits
                      })
                .ToListAsync();

            return query;
        }

    }
}
