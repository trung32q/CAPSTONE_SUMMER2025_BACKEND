using API.DTO.PostDTO;
using Infrastructure.Models;

namespace API.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<bool> CreatePost(ReqPostDTO reqPostDTO);
        Task<List<Post>> GetPostsByAccountId(int accountId);
        Task<List<PostComment>> GetPostCommentByPostId(int postId);
        Task<bool> CreatePostComment(reqPostCommentDTO reqPostCommentDTO);
        Task<List<PostLike>> GetPostLikeByPostId(int postId);
    }
}
