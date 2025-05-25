using API.DTO.PostDTO;
using API.Repositories.Interfaces;
using API.Service.Interface;
using AutoMapper;
using Infrastructure.Models;

namespace API.Service
{
    public class PostService : IPostService
    {

        private readonly IPostRepository _repository;
        private readonly IMapper _mapper;
        private readonly IChatGPTService _chatGPTService;
        private readonly IPolicyService _policyService;

        public PostService(IPostRepository repository, IMapper mapper, IChatGPTService chatGPTService, IPolicyService policyService)
        {
            _repository = repository;
            _mapper = mapper;
            _chatGPTService = chatGPTService;
            _policyService = policyService;
        }
        public async Task<List<resPostDTO>> GetPostsByAccountId(int accountId)
        {
            var posts = await _repository.GetPostsByAccountId(accountId);

            if (posts == null)
                return null;

            var postDTOs = _mapper.Map<List<resPostDTO>>(posts);

            return postDTOs;
        }

        public async Task<List<PostCommentDTO>> GetPostCommentByPostId(int postId)
        {
            try
            {
                var postComments = await _repository.GetPostCommentByPostId(postId);

                if (postComments == null)
                    return null;

                var postCommentDTOs = _mapper.Map<List<PostCommentDTO>>(postComments);

                return postCommentDTOs;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }

        //hàm like bài viết
        public async Task<bool> LikePostAsync(LikeRequestDTO dto)
        {
            return await _repository.LikePostAsync(dto.PostId, dto.AccountId);
        }

        //hàm hủy like bài viết
        public async Task<bool> UnlikePostAsync(LikeRequestDTO dto)
        {
            return await _repository.UnlikePostAsync(dto.PostId, dto.AccountId);
        }

        //hàm lấy ra số lượng like ở 1 bài viết
        public async Task<int> GetPostLikeCountAsync(int postId)
        {
            return await _repository.GetPostLikeCountAsync(postId);
        }

        // hàm lấy số lượng comment ở 1 bài viết
        public async Task<int> GetPostCommentCountAsync(int postId)
        {
            return await _repository.GetPostCommentCountAsync(postId);
        }

        // hàm  check xem bài viết có bao nhiêu like
        public async Task<bool> IsPostLikedAsync(LikeRequestDTO dto)
        {
            return await _repository.IsPostLikedAsync(dto.PostId, dto.AccountId);
        }

        public async Task<List<PostLikeDTO>> GetPostLikeByPostId(int postId)
        {
            try
            {
                var postLikes = await _repository.GetPostLikeByPostId(postId);

                if (postLikes == null)
                    return null;

                var postLikeDTOs = _mapper.Map<List<PostLikeDTO>>(postLikes);
                return postLikeDTOs;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }

        public async Task<string> CreatePost(ReqPostDTO reqPostDTO)
        {
            try
            {
                // Danh sách chính sách kiểm duyệt
                var policies = await _policyService.GetAllPoliciesAsync();

                // Kiểm duyệt nội dung
                var result = await _chatGPTService.ModeratePostContentAsync(reqPostDTO, policies);
                if (result.Contains("Vi phạm"))
                {
                    return result;
                }

                // Tạo bài viết
                var success = await _repository.CreatePost(reqPostDTO);
                if (!success)
                    return "Account không tồn tại";

                return "Successfully";
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "";
                return $"Error: {ex.Message} | Inner: {inner}";
            }
        }


        public async Task<string> CreatePostComment(reqPostCommentDTO reqPostCommentDTO)
        {
            try
            {
                var success = await _repository.CreatePostComment(reqPostCommentDTO);

                if (!success)
                    return "Account hoặc Post không tồn tại";

                return "Successfully";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        
    }
}
