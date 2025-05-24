using API.DTO.PostDTO;
using API.Repositories.Interfaces;
using API.Service.Interface;
using AutoMapper;
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

        //create post
        [HttpPost("CreatePost")]
        public async Task<IActionResult> CreatePost(ReqPostDTO reqPostDTO)
        {
            var result = await _postService.CreatePost(reqPostDTO);

            if (result == null)
            {
                return NotFound(new { error = "Account not found" });
            }

            if (result.StartsWith("Error:"))
            {
                return StatusCode(500, new { error = result });
            }

            return Ok(new { message = result });
        }

       
    }
}
