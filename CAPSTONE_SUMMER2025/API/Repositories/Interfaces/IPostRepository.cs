using API.DTO.AccountDTO;
using API.DTO.PostDTO;
using Infrastructure.Models;

namespace API.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<bool> CreatePost(ReqPostDTO reqPostDTO);
        Task<PagedResult<Post>> GetPostsByAccountId(int accountId, int pageNumber, int pageSize, int currentAccountId);
        Task<PagedResult<PostComment>> GetPostCommentByPostId(int postId, int pageNumber, int pageSize);
        Task<bool> CreatePostComment(reqPostCommentDTO reqPostCommentDTO);
        Task<PagedResult<PostLike>> GetPostLikeByPostId(int postId, int pageNumber, int pageSize);
        Task<bool> LikePostAsync(int postId, int accountId);
        Task<bool> UnlikePostAsync(int postId, int accountId);
        Task<int> GetPostLikeCountAsync(int postId);
        Task<bool> IsPostLikedAsync(int postId, int accountId);
        Task<int> GetPostCommentCountAsync(int postId);
        Task<int> CountChildCommentByPostCommentId(int? id);
        Task<PagedResult<PostComment>> GetPostCommentChildByPostIdAndParentCommentId(int pageNumber, int pageSize, int parrentCommentId);
        Task<bool> UpdateCommentAsync(int commentId, string newContent);
        Task<bool> DeleteCommentAsync(int commentId);
        Task<bool> UpdatePostAsync(int postId, string title, string content);
        Task<bool> DeletePostAsync(int postId);
        IQueryable<resPostDTO> GetSearchPosts(string keyword, int currentUserId);
        Task<int?> GetAccountIdByPostIDAsync(int postId);
        Task<int?> GetAccountIdByCommentId(int commentId);
        Task<List<FeedItemDTO>> GetRecommendedFeedAsync(int userId, int page, int pageSize);
        Task<bool> HidePostAsync(int accountId, int postId);
        Task<List<ReportReason>> GetAllReportReasonAsync();
        Task<ReportReason?> GetReportReasonByIdAsync(int id);
        Task<ReportReason> CreateReportReasonAsync(ReportReason reason);
        Task<bool> UpdateReportReasonAsync(ReportReason reason);
        Task<bool> DeleteReportReasonAsync(int id);
        Task<PostReport> CreatePostReportAsync(PostReport report);
        Task<PostReport?> GetPostReportByIdAsync(int reportId);
        Task<Post> GetPostByPostIdAsync(int id);
        Task AddInternshipPostAsync(InternshipPost post);
        Task<(List<FeedItemDTO> Items, int TotalCount)> GetStartupFeedItemsAsync(int startupId, int skip, int take);
        Task ShareAsync(Post post);
        Task<List<Post>> GetScheduledPostsAsync();
        Task<InternshipPost> GetInternshipPostByIdAsync(int id);
        Task UpdateInternshipPostAsync(InternshipPost post);
        Task SaveChangesAsync();

        Task<PagedResult<Post>> GetPostsByStartupId(int startupId, int pageNumber, int pageSize);
        Task<Startup?> GetStartupByIdAsync(int startupId);
        Task<PagedResult<InternshipPost>> GetInternshipPostsAsync(int pageNumber, int pageSize,int startupid);
        IQueryable<Post> GetSearchPostsByStartup(int startupId);
        IQueryable<InternshipPost> GetStartupInternshipPost(int startupId);
        Task<InternshipPost?> GetInternshipPostWithNavigationAsync(int internshipPostId);
        Task<bool> UpdateInternshipPostAsync(int internhsipId, InternshipPost internshipPost);
        void UpdateInternshipPost(InternshipPost post);
        Task<List<TopInternshipPostDTO>> GetTopInternshipPostsByCVCountAsync(int top = 5);
    }
}
