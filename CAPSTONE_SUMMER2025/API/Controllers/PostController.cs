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


        //get posts by accountId
        [HttpGet("GetPostsByAccountId")]
        public async Task<IActionResult> GetPostsByAccountId(int accountId)
        {
            var result = await _postService.GetPostsByAccountId(accountId);

            if (result == null)
            {
                return NotFound(new { error = "Account not found" });
            }

            return Ok(result);
        }

        //get post comments by post id
        [HttpGet("GetPostCommentsByPostId")]
        public async Task<IActionResult> GetPostCommentsByPostId(int postId)
        {
            var result = await _postService.GetPostCommentByPostId(postId);
            if (result == null)
            {
                return NotFound(new {error = "Post not found"});
            }

            return Ok(result);
        }

        //get post likes by postid
        [HttpGet("GetPostLikeByPostId")]
        public async Task<IActionResult> GetPostLikeByPostId(int postId)
        {
            var result = await _postService.GetPostLikeByPostId(postId);
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

            if (result.StartsWith("Vi phạm"))
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
        public async Task<IActionResult> IsPostLikedAsync([FromBody] LikeRequestDTO dto)
        {
            var success = await _postService.IsPostLikedAsync(dto);
            return Ok(success);
        }
    }
}
