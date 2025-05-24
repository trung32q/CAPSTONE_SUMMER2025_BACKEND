using API.DTO.PostDTO;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface IPostService
    {
        Task<List<resPostDTO>> GetPostsByAccountId(int accountId);
        Task<List<PostCommentDTO>> GetPostCommentByPostId(int postId);
        Task<string> CreatePost(ReqPostDTO reqPostDTO);
        Task<List<PostLikeDTO>> GetPostLikeByPostId(int postId);
        Task<string> CreatePostComment(reqPostCommentDTO reqPostCommentDTO);
    }
}
