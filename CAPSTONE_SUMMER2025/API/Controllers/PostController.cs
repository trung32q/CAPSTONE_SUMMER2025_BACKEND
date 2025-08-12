using API.DTO.PostDTO;
using API.Repositories.Interfaces;
using API.Service.Interface;
using AutoMapper;
using Google.Api;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<IActionResult> GetPostsByAccountId(int accountId, int currentAccountId, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _postService.GetPostsByAccountIdAsync(accountId, pageNumber, pageSize, currentAccountId);

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

        // Create post cho cả statup với account
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

        // hàm đếm số lượng comment bài viết
        [HttpGet("{postId}/comment-count")]
        public async Task<IActionResult> GetPostCommentCount(int postId)
        {
            var count = await _postService.GetPostCommentCountAsync(postId);
            return Ok(count);
        }

        //hàm kiểm tra xem bài viết được like chưa
        [HttpGet("liked")]
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


        //serch post của startupId
        [HttpGet("search-startup-posts")]
        public async Task<IActionResult> SearchStatupPosts(
        [FromQuery] int startupId,
        [FromQuery] string? keyword,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            var result = await _postService.GetSearchPostsByStartup(startupId, keyword, pageNumber, pageSize);

            return Ok(new
            {
                message = "Search posts successfully",
                data = result
            });
        }

        [HttpGet("search-startup-internship-post")]
        public async Task<IActionResult> SearchInternshipPosts(
     [FromQuery] int startupId,
     [FromQuery] string? keyword,
     [FromQuery] int pageNumber = 1,
     [FromQuery] int pageSize = 10)
        {
            var result = await _postService.GetSearchStartupInternshipPost(startupId, keyword, pageNumber, pageSize);

            return Ok(new
            {
                message = "Search internship posts successfully",
                data = result
            });
        }


        [HttpGet("NewFeed")]
        public async Task<IActionResult> GetNewFeed(int userId, int page = 1, int pageSize = 10)
        {
            var feed = await _postService.GetNewFeedAsync(userId, page, pageSize);
            return Ok(feed);
        }

        // ẩn bài post
        [HttpPost("hide")]
        public async Task<IActionResult> HidePost([FromBody] HidePostRequestDTO dto)
        {
            var result = await _postService.HidePostAsync(dto);
            if (result)
                return Ok(new { message = "Post hidden successfully" });

            return BadRequest(new { message = "Post already hidden or invalid" });
        }

        //lấy ra tất cả các report reason
        [HttpGet("report-reasons")]
        public async Task<IActionResult> GetAllReportReason()
        {
            var reasons = await _postService.GetAllReportReasonAsync();
            return Ok(reasons);
        }

        //lấy ra report reason theo id
        [HttpGet("report-reasons/{id}")]
        public async Task<IActionResult> GetReportReasonById(int id)
        {
            var reason = await _postService.GetReportReasonByIdAsync(id);
            if (reason == null) return NotFound();
            return Ok(reason);
        }

        // tạo mới report reason
        [HttpPost("report-reasons")]
        public async Task<IActionResult> CreateReportReason([FromBody] CreateReportReasonDTO dto)
        {
            var created = await _postService.CreateReportReasonAsync(dto);
            return Ok(created); 
        }

        //cập nhật report reason
        [HttpPut("report-reasons/{id}")]
        public async Task<IActionResult> UpdateReportReason(int id, [FromBody] CreateReportReasonDTO dto)
        {
            var result = await _postService.UpdateReportReasonAsync(id, dto);
            return result ? NoContent() : NotFound();
        }

        //xóa reaport reason
        [HttpDelete("report-reasons/{id}")]
        public async Task<IActionResult> DeleteReportReason(int id)
        {
            var result = await _postService.DeleteReportReasonAsync(id);
            return result ? NoContent() : NotFound();
        }

        //report post
        [HttpPost("post-reports")]
        public async Task<IActionResult> ReportPost([FromBody] CreatePostReportDTO dto)
        {
            try
            {
                var result = await _postService.CreateReportAsync(dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // tạo mới internshipPost
        [HttpPost("create-internship-post")]
        public async Task<IActionResult> Create([FromBody] CreateInternshipPostDTO dto)
        {
            try
            {
                await _postService.CreateInternshipPostAsync(dto);
                return Ok(new { message = "Tạo bài tuyển dụng thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
            }
        }

        //lấy ra feed của startup
        [HttpGet("startup-feeds/{startupId}")]
        public async Task<IActionResult> GetStartupFeed(int startupId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _postService.GetStartupFeedAsync(startupId, page, pageSize);
            return Ok(result);
        }


        // share bài viết
        [HttpPost("share")]
        public async Task<IActionResult> SharePost([FromBody] SharePostRequest request)
        {
            try
            {
                var sharedPost = await _postService.SharePostAsync(request);
                return Ok(new
                {
                    message = "Chia sẻ thành công",
                    sharedPostId = sharedPost.PostId
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        //lấy ra post theo postid
        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetPostById(int postId)
        {
            var post = await _postService.GetPostByPostId(postId);
            if (post == null)
                return NotFound(new { message = "Post not found" });

            return Ok(post);
        }
     

        //lấy ra những bài post được hẹn nhưng chưa đăng
        [HttpGet("scheduled")]
        public async Task<IActionResult> GetScheduledPosts()
        {
            var posts = await _postService.GetScheduledPostsAsync();
            return Ok(posts);
        }

        //publish bài post
        [HttpPut("publish/{postId}")]
        public async Task<IActionResult> PublishPost(int postId)
        {
            var success = await _postService.PublishPostAsync(postId);
            if (!success)
                return NotFound(new { message = "Post not found or already published." });

            return Ok(new { message = "Post published successfully." });
        }

        //cập nhật status của internshippost (nếu đang là active thì sẽ thành deactive và ngược lại)
        [HttpPut("update-internshippost-status")]
        public async Task<IActionResult> UpdateStatus(int internshipPostId)
        {
            var result = await _postService.UpdateInternshipPostAsync(internshipPostId);
            if (!result) return NotFound("Internship post not found");

            return Ok("Status updated successfully");
        }

   

    
        [HttpGet("GetPostsByStartupId")]
        public async Task<IActionResult> GetPostsByStartupId(int startupId, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _postService.GetPostsByStartupIdAsync(startupId, pageNumber, pageSize);

            if (result == null || result.Items.Count == 0)
            {
                return NotFound(new { error = "No posts found or startup does not exist" });
            }

            return Ok(result);
        }
        [HttpGet("GetAllInternshipPosts")]
        public async Task<IActionResult> GetAllInternshipPosts(int startupid, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _postService.GetAllInternshipPostsAsync(pageNumber, pageSize, startupid);

            if (result == null || result.Items.Count == 0)
                return NotFound(new { error = "No internship posts found" });

            return Ok(result);
        }

        [HttpGet("internshippost/{internshipPostId}")]
        public async Task<IActionResult> GetInternshipPostDetail(int internshipPostId)
        {
            var result = await _postService.GetInternshipPostDetailAsync(internshipPostId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        //update internshipPost
        [HttpPut("internShipPost/{id}")]
        public async Task<IActionResult> UpdateInternshipPost(int id, [FromBody] UpdateInternshipPostDTO dto)
        {
            

            var success = await _postService.UpdateInternshipPostAsync(id, dto);
            if (!success)
                return NotFound("Internship post not found.");

            return Ok("update thành công");
        }
    }
}
