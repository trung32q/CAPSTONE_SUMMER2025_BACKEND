using Microsoft.AspNetCore.Mvc;
using API.Repositories.Interfaces;
using API.DTO.PostDTO;
// using Microsoft.AspNetCore.Authorization; // Remove this for now

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _postRepository;

        public PostController(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        [HttpGet("newsfeed")]
        // [Authorize] // Remove for testing
        public async Task<ActionResult<List<ResPostDTO>>> GetNewsfeed(
            [FromQuery] int? accountId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            // For testing, allow passing currentUserId as query param if no auth
            int currentUserId = accountId ?? 1; // Default to 1 if not provided
            int targetUserId = accountId ?? currentUserId;
            var posts = await _postRepository.GetPostsAsync(targetUserId, currentUserId, page, pageSize);
            return Ok(posts);
        }

        [HttpGet("{postId}")]
        // [Authorize] // Remove for testing
        public async Task<ActionResult<ResPostDTO>> GetPostById(int postId, [FromQuery] int? currentUserId = 1)
        {
            var post = await _postRepository.GetPostByIdAsync(postId, currentUserId ?? 1);
            if (post == null)
                return NotFound();
            return Ok(post);
        }
    }
} 