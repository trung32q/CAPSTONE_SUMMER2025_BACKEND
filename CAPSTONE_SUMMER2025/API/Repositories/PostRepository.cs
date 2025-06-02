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

        // hàm lấy ra danh sách thông tin của người like bài post
        public async Task<PagedResult<PostLike>> GetPostLikeByPostId(int postId, int pageNumber, int pageSize)
        {
            if (!await IsPostExistsAsync(postId))
                return null;

            var querry = _context.PostLikes.Where(pl => pl.PostId == postId);

            var totalCount = await querry.CountAsync();
            var items = await querry.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<PostLike>(items, totalCount, pageNumber, pageSize);
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
        public async Task<PagedResult<Post>> GetPostsByAccountId(int accountId, int pageNumber, int pageSize)
        {
            if (!await IsAccountExistsAsync(accountId))
                return null;

            var query = _context.Posts
                .Where(p => p.AccountId == accountId)
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
            if (!await IsAccountExistsAsync(reqPostDTO.AccountId.Value))
                return false;

            var post = new Post
            {
                AccountId = reqPostDTO.AccountId.Value,
                Content = reqPostDTO.Content,
                Title = reqPostDTO.Title,
            };

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
                    PostMedia = p.PostMedia.Select(m => new PostMediaDTO
                    {
                        MediaUrl = m.MediaUrl,
                        DisplayOrder = m.DisplayOrder
                    }).ToList()
                }).AsQueryable();

            return result;
        }


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



    }
}
