using AutoMapper;
using Infrastructure.Models;
using API.DTO;
using API.DTO.PostDTO;
using Microsoft.EntityFrameworkCore;

// API/Repositories/PostRepository.cs
public class PostRepository : IPostRepository
{
    private readonly CAPSTONE_SUMMER2025Context _context;
    private readonly IMapper _mapper;

    public PostRepository(CAPSTONE_SUMMER2025Context context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ResPostDTO>> GetPostsAsync(int targetUserId, int currentUserId, int page, int pageSize)
    {
        // Lấy danh sách account bị block
        var blockedAccounts = await _context.AccountBlocks
            .Where(b => b.BlockerAccountId == currentUserId || b.BlockedAccountId == currentUserId)
            .Select(b => b.BlockedAccountId)
            .ToListAsync();

        // Lấy danh sách account đang follow
        var followedAccounts = await _context.Follows
            .Where(f => f.FollowerAccountId == targetUserId)
            .Select(f => f.FollowingAccountId)
            .ToListAsync();

        // Query posts với các điều kiện
        var posts = await _context.Posts
            .Include(p => p.Account)
                .ThenInclude(a => a.AccountProfile)
            .Include(p => p.PostLikes)
            .Include(p => p.PostComments)
            .Where(p => !blockedAccounts.Contains(p.AccountId))
            .OrderByDescending(p => followedAccounts.Contains(p.AccountId))
            .ThenByDescending(p => p.CreateAt)
            .ThenByDescending(p => p.PostLikes.Count + p.PostComments.Count)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Map to DTO
        var postDtos = _mapper.Map<List<ResPostDTO>>(posts);

        // Set IsLiked status
        var likedPostIds = await _context.PostLikes
            .Where(l => l.AccountId == currentUserId && posts.Select(p => p.PostId).ToList().Contains(l.PostId.GetValueOrDefault()))
            .Select(l => l.PostId)
            .ToListAsync();

        foreach (var dto in postDtos)
        {
            dto.IsLiked = likedPostIds.Contains(dto.PostId);
        }

        return postDtos;
    }

    public async Task<ResPostDTO> GetPostByIdAsync(int postId, int currentUserId)
    {
        var post = await _context.Posts
            .Include(p => p.Account)
                .ThenInclude(a => a.AccountProfile)
            .Include(p => p.PostLikes)
            .Include(p => p.PostComments)
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null) return null;

        var dto = _mapper.Map<ResPostDTO>(post);
        dto.IsLiked = await _context.PostLikes
            .AnyAsync(l => l.PostId == postId && l.AccountId == currentUserId);

        return dto;
    }

    public async Task<ResPostDTO> CreatePostAsync(int accountId, ReqPostDTO post)
    {
        var newPost = new Post
        {
            AccountId = accountId,
            Content = post.Content,
            CreateAt = DateTime.UtcNow
        };

        _context.Posts.Add(newPost);
        await _context.SaveChangesAsync();

        return await GetPostByIdAsync(newPost.PostId, accountId);
    }

    public async Task<bool> DeletePostAsync(int postId, int accountId)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.PostId == postId && p.AccountId == accountId);

        if (post == null) return false;

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
        return true;
    }

}