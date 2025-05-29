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

namespace API.Repositories
{
    public class PostRepository : IPostRepository
    {
        
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly IFilebaseHandler _filebaseHandler;

        public PostRepository(IFilebaseHandler filebaseHandler ,CAPSTONE_SUMMER2025Context context)
        {
            _context = context;
            _filebaseHandler = filebaseHandler;
        }

        //hàm tạo comment
        public async Task<bool> CreatePostComment(reqPostCommentDTO reqPostCommentDTO)
        {
            var accountExists = await _context.Accounts
       .AnyAsync(acc => acc.AccountId == reqPostCommentDTO.AccountId);

            var postExists = await _context.Posts
                .AnyAsync(post => post.PostId == reqPostCommentDTO.PostId);

            if (!accountExists || !postExists)
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
        public async Task<List<PostLike>> GetPostLikeByPostId(int postId)
        {
            var postExists = await _context.Posts.AnyAsync(p => p.PostId == postId);
            if (!postExists)
                return null;

            var postLikes = await _context.PostLikes
                .Where(pl => pl.PostId == postId)
                .ToListAsync();

            return postLikes;
        }

        // Upload post
        public async Task<bool> CreatePost(ReqPostDTO reqPostDTO)
        {
            var accountExists = await _context.Accounts
        .AnyAsync(acc => acc.AccountId == reqPostDTO.AccountId);

            if (!accountExists)
                return false;

            var post = new Post
            {
                AccountId = reqPostDTO.AccountId,
                Content = reqPostDTO.Content,
                Title = reqPostDTO.Title,
            };

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            var postMedias = new List<PostMedium>();
            int displayOrder = 0;

            if (!(reqPostDTO.MediaFiles == null || !reqPostDTO.MediaFiles.Any()))
            {
                foreach (var file in reqPostDTO.MediaFiles)
                {
                    //Console.WriteLine($"[DEBUG] File: {file.FileName}, ContentType: {file.ContentType}, Length: {file.Length}");
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


        // hàm lấy ra danh sách các comment theo postid
        public async Task<PagedResult<PostComment>> GetPostCommentByPostId(int postId, int pageNumber, int pageSize)
        {
            var postExists = await _context.Posts
        .AnyAsync(p => p.PostId == postId);

            if (!postExists)
                return null;

            var query = _context.PostComments
                .Where(pc => pc.PostId == postId && pc.ParentCommentId == null);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<PostComment>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<PostComment>> GetPostCommentChildByPostIdAndParentCommentId(int pageNumber, int pageSize, int parrentCommentId)
        {
          

            var query = _context.PostComments
                .Where(pc => pc.ParentCommentId == parrentCommentId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<PostComment>(items, totalCount, pageNumber, pageSize);
        }


        //hàm lấy ra các bài post theo accountid
        public async Task<PagedResult<Post>> GetPostsByAccountId(int accountId, int pageNumber, int pageSize)
        {
            var accountExists = await _context.Accounts
                .AnyAsync(acc => acc.AccountId == accountId);

            if (!accountExists)
                return null;

            var query = _context.Posts
                .Where(p => p.AccountId == accountId)
                .Include(p => p.PostMedia)
                .OrderByDescending(p => p.CreateAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Post>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<int> CountChildCommentByPostCommentId(int? id)
        {
            if (id == null)
            {
                return 0;
            }
            return await _context.PostComments.Where(p => p.ParentCommentId == id).CountAsync() ;
            
        }
    }
}
