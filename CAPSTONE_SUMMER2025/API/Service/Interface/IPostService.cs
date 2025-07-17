using System.Threading.Tasks;
using API.DTO.AccountDTO;
using API.DTO.PostDTO;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface IPostService
    {
        Task<PagedResult<resPostDTO>> GetPostsByAccountIdAsync(int accountId, int pageNumber, int pageSize, int currentAccountId);
        Task<PagedResult<PostCommentDTO>> GetPostCommentByPostId(int postId, int pageNumber, int pageSize);
        Task<string> CreatePost(ReqPostDTO reqPostDTO);
        Task<PagedResult<PostLikeDTO>> GetPostLikeByPostId(int postId, int pageNumber, int pageSize);
        Task<string> CreatePostComment(reqPostCommentDTO reqPostCommentDTO);
        Task<bool> LikePostAsync(LikeRequestDTO dto);
        Task<bool> UnlikePostAsync(LikeRequestDTO dto);
        Task<int> GetPostLikeCountAsync(int postId);
        Task<bool> IsPostLikedAsync(LikeRequestDTO dto);
        Task<int> GetPostCommentCountAsync(int postId);
        Task<PagedResult<PostCommentDTO>> GetPostCommentChildByPostIdAndParentCommentId(int pageNumber, int pageSize, int parrentCommentId);
        Task<bool> UpdateCommentAsync(UpdateCommentDTO dto);
        Task<bool> DeleteCommentAsync(int commentId);
        Task<bool> UpdatePostAsync(int postId, reqUpdatePostDTO dto);
        Task<bool> DeletePostAsync(int postId);
        Task<PagedResult<resPostDTO>> SearchPostsAsync(string searchText, int pageNumber, int pageSize, int currentAccountId);
        Task<List<FeedItemDTO>> GetNewFeedAsync(int userId, int page, int pageSize);
        Task<bool> HidePostAsync(HidePostRequestDTO dto);
        Task<List<ReportReasonDTO>> GetAllReportReasonAsync();
        Task<ReportReasonDTO?> GetReportReasonByIdAsync(int id);
        Task<ReportReasonDTO> CreateReportReasonAsync(CreateReportReasonDTO dto);
        Task<bool> UpdateReportReasonAsync(int id, CreateReportReasonDTO dto);
        Task<bool> DeleteReportReasonAsync(int id);
        Task<PostReportDTO> CreateReportAsync(CreatePostReportDTO dto);
        Task CreateInternshipPostAsync(CreateInternshipPostDTO dto);
        Task<PagedResult<FeedItemDTO>> GetStartupFeedAsync(int startupId, int page, int pageSize);
        Task<Post> SharePostAsync(SharePostRequest request);
        Task<resPostDTO> GetPostByPostId(int postId);
        Task<List<PostScheduleDTO>> GetScheduledPostsAsync();
        Task<bool> PublishPostAsync(int postId);
        Task<bool> UpdateInternshipPostAsync(int internShipPostId);
        Task<PagedResult<resPostDTO>> GetPostsByStartupIdAsync(int startupId, int pageNumber, int pageSize);
        Task<PagedResult<InternshipPostDTO>> GetAllInternshipPostsAsync(int pageNumber, int pageSize, int startupid);
        Task<PagedResult<PostSearchDTO>> GetSearchPostsByStartup(int startupId, string? keyword, int pageNumber, int pageSize);
        Task<PagedResult<InternshipPostDTO>> GetSearchStartupInternshipPost(int startupId, string? keyword, int pageNumber, int pageSize);
        Task<InternshipPostDetailDTO?> GetInternshipPostDetailAsync(int internshipPostId);
        Task<bool> UpdateInternshipPostAsync(int internshipPostId, UpdateInternshipPostDTO dto);
        Task<List<TopInternshipPostDTO>> GetTopInternshipPostsAsync(int top);
    }
}
