using API.DTO.AccountDTO;
using API.DTO.PostDTO;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface IPostService
    {
        Task<PagedResult<resPostDTO>> GetPostsByAccountIdAsync(int accountId, int pageNumber, int pageSize);
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
    }
}
