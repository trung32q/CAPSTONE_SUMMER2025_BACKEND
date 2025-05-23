using API.DTO.PostDTO;

// API/Repositories/Interfaces/IPostRepository.cs
public interface IPostRepository
{
    Task<List<ResPostDTO>> GetPostsAsync(int targetUserId, int currentUserId, int page, int pageSize);
    Task<ResPostDTO> GetPostByIdAsync(int postId, int currentUserId);
    Task<ResPostDTO> CreatePostAsync(int accountId, ReqPostDTO post);
    Task<bool> DeletePostAsync(int postId, int accountId);
}