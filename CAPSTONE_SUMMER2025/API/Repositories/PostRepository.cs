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
        private readonly IMapper _mapper;
        private readonly IFilebaseHandler _filebaseHandler;

        public PostRepository(IFilebaseHandler filebaseHandler ,CAPSTONE_SUMMER2025Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
