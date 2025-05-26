using API.DTO.AccountDTO;
using API.DTO.PostDTO;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface IPostService
    {
        Task<PagedResult<resPostDTO>> GetPostsByAccountIdAsync(int accountId, int pageNumber, int pageSize);
        Task<List<PostCommentDTO>> GetPostCommentByPostId(int postId);
        Task<string> CreatePost(ReqPostDTO reqPostDTO);
        Task<List<PostLikeDTO>> GetPostLikeByPostId(int postId);
        Task<string> CreatePostComment(reqPostCommentDTO reqPostCommentDTO);
        Task<bool> LikePostAsync(LikeRequestDTO dto);
        Task<bool> UnlikePostAsync(LikeRequestDTO dto);
        Task<int> GetPostLikeCountAsync(int postId);
        Task<bool> IsPostLikedAsync(LikeRequestDTO dto);
        Task<int> GetPostCommentCountAsync(int postId);

    }
}
