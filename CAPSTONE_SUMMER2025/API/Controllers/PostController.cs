using API.DTO.PostDTO;
using API.Repositories.Interfaces;
using API.Service.Interface;
using AutoMapper;
using Google.Api;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class PostController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IPostRepository _postRepository;
        private readonly IPostService _postService;
        public PostController(IMapper mapper, IPostRepository postRepository, IPostService postService) 
        {
            _mapper = mapper;
            _postRepository = postRepository;
            _postService = postService;
        }

      
        // GET: api/Post/GetPostsByAccountId?accountId=1&pageNumber=1&pageSize=10
        [HttpGet("GetPostsByAccountId")]
        public async Task<IActionResult> GetPostsByAccountId(int accountId, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _postService.GetPostsByAccountIdAsync(accountId, pageNumber, pageSize);

            if (result == null || result.Items.Count == 0)
            {
                return NotFound(new { error = "No posts found or account does not exist" });
            }

            return Ok(result);
        }


        //get post comments by post id
        [HttpGet("GetPostCommentsByPostId")]
        public async Task<IActionResult> GetPostCommentsByPostId(int postId, int pageNumber = 1,int pageSize = 10)
        {
            var result = await _postService.GetPostCommentByPostId(postId, pageNumber, pageSize);
            if (result == null)
            {
                return NotFound(new {error = "Post not found"});
            }

            return Ok(result);
        }

        //get post comments by post id
        [HttpGet("GetPostChidComments")]
        public async Task<IActionResult> GetPostCommentChildByPostIdAndParentCommentId(int parrentCommentId, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _postService.GetPostCommentChildByPostIdAndParentCommentId(pageNumber, pageSize, parrentCommentId);
            if (result == null)
            {
                return NotFound(new { error = "parrent comment not found" });
            }

            return Ok(result);
        }

        //get post likes by postid
        [HttpGet("GetPostLikeByPostId")]
        public async Task<IActionResult> GetPostLikeByPostId(int postId, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _postService.GetPostLikeByPostId(postId, pageNumber, pageSize);
            if (result == null)
            {
                return NotFound(new { error = "Post not found" });
            }

            return Ok(result);
        }

        //create post comment
        [HttpPost("CreatePostComment")]
        public async Task<IActionResult> CreatePostComment(reqPostCommentDTO reqPostCommentDTO)
        {
            var result = await _postService.CreatePostComment(reqPostCommentDTO);

            if (result == null)
            {
                return NotFound(new { error = "Account or post not found" });
            }

            if (result.StartsWith("Error:"))
            {
                return StatusCode(500, new { error = result });
            }

            return Ok(new { message = result });
        }

        // Create post
        [HttpPost("CreatePost")]
        public async Task<IActionResult> CreatePost([FromForm] ReqPostDTO reqPostDTO)
        {
            if (reqPostDTO == null)
            {
                return BadRequest(new { error = "Request body is missing or malformed" });
            }

            var result = await _postService.CreatePost(reqPostDTO);

            if (result == null)
            {
                return NotFound(new { error = "Account not found" });
            }

            if (result.StartsWith("Error:"))
            {
                return StatusCode(500, new { error = result });
            }

            if (result.StartsWith("Violation"))
            {
                // Tách lỗi nếu có file vi phạm
                var parts = result.Split(" (File:");
                var message = parts[0].Trim();
                var fileInfo = parts.Length > 1 ? parts[1].Replace(")", "").Trim() : null;

                return BadRequest(new
                {
                    violated = true,
                    message,
                    file = fileInfo
                });
            }

            return Ok(new { message = result });
        }

        // hàm like bài viết
        [HttpPost("like")]
        public async Task<IActionResult> LikePost([FromBody] LikeRequestDTO dto)
        {
            var success = await _postService.LikePostAsync(dto);
            return success ? Ok("Liked") : BadRequest("Already liked");
        }

        // hàm hủy like bài viết
        [HttpPost("unlike")]
        public async Task<IActionResult> UnlikePost([FromBody] LikeRequestDTO dto)
        {
            var success = await _postService.UnlikePostAsync(dto);
            return success ? Ok("Unliked") : NotFound("Like not found");
        }

        // hàm đếm số lượng like bài viết
        [HttpGet("{postId}/like-count")]
        public async Task<IActionResult> GetPostLikeCount(int postId)
        {
            var count = await _postService.GetPostLikeCountAsync(postId);
            return Ok(count);
        }

        // hàm đếm số lượng like bài viết
        [HttpGet("{postId}/comment-count")]
        public async Task<IActionResult> GetPostCommentCount(int postId)
        {
            var count = await _postService.GetPostCommentCountAsync(postId);
            return Ok(count);
        }

        //hàm kiểm tra xem bài viết được like chưa
        [HttpGet("{postId}/liked")]
        public async Task<IActionResult> IsPostLikedAsync([FromQuery] LikeRequestDTO dto)
        {
            var success = await _postService.IsPostLikedAsync(dto);
            return Ok(success);
        }

        // cập nhật post comment
        [HttpPut("update-post-comment")]
        public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentDTO dto)
        {
            var result = await _postService.UpdateCommentAsync(dto);
            if (!result)
                return NotFound(new { message = "Comment not found." });

            return Ok(new { message = "Comment updated successfully." });
        }

        // xóa post comment
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var result = await _postService.DeleteCommentAsync(commentId);
            if (!result)
                return NotFound(new { message = "Comment not found." });

            return Ok(new { message = "Comment deleted successfully." });
        }

        // cập nhật bài post
        [HttpPut("update-post/{postId}")]
        public async Task<IActionResult> UpdatePost(int postId, [FromBody] reqUpdatePostDTO dto)
        {
            var success = await _postService.UpdatePostAsync(postId, dto);
            if (!success)
                return NotFound(new { message = "Post not found" });

            return Ok(new { message = "Post updated successfully" });
        }

        // xóa post
        [HttpDelete("delete-post{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var result = await _postService.DeletePostAsync(postId);
            if (!result)
                return NotFound(new { message = "Post không tồn tại hoặc đã bị xóa." });

            return Ok(new { message = "Xóa bài viết thành công." });
        }

        //serch post
        [HttpGet("search-posts")]
        public async Task<IActionResult> SearchPosts(string searchText, int currentAccountId, int pageNumber = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return BadRequest(new { message = "searchText is required." });

            try
            {
                var result = await _postService.SearchPostsAsync(searchText, pageNumber, pageSize, currentAccountId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
        [HttpGet("NewFeed")]
        public async Task<IActionResult> GetNewFeed(int userId, int page = 1, int pageSize = 10)
        {
            var feed = await _postService.GetNewFeedAsync(userId, page, pageSize);
            return Ok(feed);
        }

    }
}
