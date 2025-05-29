using API.DTO.AccountDTO;
using API.DTO.PostDTO;
using Infrastructure.Models;

namespace API.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<bool> CreatePost(ReqPostDTO reqPostDTO);
        Task<PagedResult<Post>> GetPostsByAccountId(int accountId, int pageNumber, int pageSize);
        Task<PagedResult<PostComment>> GetPostCommentByPostId(int postId, int pageNumber, int pageSize);
        Task<bool> CreatePostComment(reqPostCommentDTO reqPostCommentDTO);
        Task<List<PostLike>> GetPostLikeByPostId(int postId);
        Task<bool> LikePostAsync(int postId, int accountId);
        Task<bool> UnlikePostAsync(int postId, int accountId);
        Task<int> GetPostLikeCountAsync(int postId);
        Task<bool> IsPostLikedAsync(int postId, int accountId);
        Task<int> GetPostCommentCountAsync(int postId);

        Task<int> CountChildCommentByPostCommentId(int? id);

        Task<PagedResult<PostComment>> GetPostCommentChildByPostIdAndParentCommentId(int pageNumber, int pageSize, int parrentCommentId);
    }
}
