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
        public async Task<List<PostComment>> GetPostCommentByPostId(int postId)
        {
            var postExists = await _context.Posts
        .AnyAsync(p => p.PostId == postId);

            if (!postExists)
                return null;

            var postComments = await _context.PostComments
                .Where(pc => pc.PostId == postId)
                .ToListAsync();

            return postComments;
        }

        //hàm lấy ra các bài post theo accountid
        public async Task<List<Post>> GetPostsByAccountId(int accountId)
        {
            var accountExists = await _context.Accounts
        .AnyAsync(acc => acc.AccountId == accountId);

            if (!accountExists)
                return null;

            var posts = await _context.Posts
                .Where(p => p.AccountId == accountId)
                .Include(p => p.PostMedia)
                .ToListAsync();

            return posts;
        }    
    }
}
