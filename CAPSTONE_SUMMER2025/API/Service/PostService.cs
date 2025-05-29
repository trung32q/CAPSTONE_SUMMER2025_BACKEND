using API.DTO.AccountDTO;
using API.DTO.NotificationDTO;
using API.DTO.PostDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Service.Interface;
using AutoMapper;
using Google.Cloud.AIPlatform.V1;
using Infrastructure.Models;
using Infrastructure.Repository;

namespace API.Service
{
    public class PostService : IPostService
    {

        private readonly IPostRepository _repository;
        private readonly IFilebaseHandler _filebase;
        private readonly IMapper _mapper;
        private readonly IChatGPTService _chatGPTService;
        private readonly IPolicyService _policyService;
        private readonly IAccountRepository _accountRepository;
        private readonly INotificationService _notificationService;

        public PostService(IPostRepository repository, IMapper mapper, IChatGPTService chatGPTService, IPolicyService policyService, IFilebaseHandler filebase, IAccountRepository accountRepository, INotificationService notificationService)
        {
            _repository = repository;
            _mapper = mapper;
            _chatGPTService = chatGPTService;
            _policyService = policyService;
            _filebase = filebase;
            _accountRepository = accountRepository;
            _notificationService = notificationService;
        }
        public async Task<PagedResult<resPostDTO>> GetPostsByAccountIdAsync(int accountId, int pageNumber, int pageSize)
        {
            var pagedPosts = await _repository.GetPostsByAccountId(accountId, pageNumber, pageSize);

            if (pagedPosts == null)
                return null;

            var postDTOs = _mapper.Map<List<resPostDTO>>(pagedPosts.Items);
            foreach (var dto in postDTOs)
            {
                foreach (var media in dto.PostMedia)
                {
                    if (!string.IsNullOrEmpty(media.MediaUrl))
                    {
                        // Nếu MediaUrl không chứa resourceType (không có dấu "/"), thêm tiền tố mặc định "image/"
                        var publicIdWithType = media.MediaUrl.Contains("/")
                            ? media.MediaUrl
                            : $"image/{media.MediaUrl}";

                        // Ghi log để debug
                        Console.WriteLine($"Generating URL for publicIdWithType: {publicIdWithType}");

                        // Tạo URL bằng GeneratePreSignedUrl
                        media.MediaUrl = _filebase.GeneratePreSignedUrl(publicIdWithType);
                    }
                }
            }

            return new PagedResult<resPostDTO>(
                postDTOs,
                pagedPosts.TotalCount,
                pagedPosts.PageNumber,
                pagedPosts.PageSize
            );
        }

        public async Task<PagedResult<PostCommentDTO>> GetPostCommentByPostId(int postId, int pageNumber, int pageSize)
        {
            try
            {
                var pagedPostComments = await _repository.GetPostCommentByPostId(postId, pageNumber, pageSize);

                if (pagedPostComments == null)
                    return null;

                var postCommentDTOs = _mapper.Map<List<PostCommentDTO>>(pagedPostComments.Items);

                foreach (var dto in postCommentDTOs)
                {
                    dto.numChildComment = await _repository.CountChildCommentByPostCommentId(dto.PostcommentId);
                }

                return new PagedResult<PostCommentDTO>(
                        postCommentDTOs,
                        pagedPostComments.TotalCount,
                        pagedPostComments.PageNumber,
                        pagedPostComments.PageSize

                    );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }

        public async Task<PagedResult<PostCommentDTO>> GetPostCommentChildByPostIdAndParentCommentId(int pageNumber, int pageSize, int parrentCommentId)
        {
            try
            {
                var pagedPostComments = await _repository.GetPostCommentChildByPostIdAndParentCommentId(pageNumber, pageSize, parrentCommentId);

                if (pagedPostComments == null)
                    return null;

                var postCommentDTOs = _mapper.Map<List<PostCommentDTO>>(pagedPostComments.Items);

                foreach (var dto in postCommentDTOs)
                {
                    dto.numChildComment = await _repository.CountChildCommentByPostCommentId(dto.PostcommentId);
                }

                return new PagedResult<PostCommentDTO>(
                        postCommentDTOs,
                        pagedPostComments.TotalCount,
                        pagedPostComments.PageNumber,
                        pagedPostComments.PageSize

                    );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }

        //hàm like bài viết
        public async Task<bool> LikePostAsync(LikeRequestDTO dto)
        {
            var success = await _repository.LikePostAsync(dto.PostId, dto.AccountId);
            if (success)
            {
                // Gửi thông báo tới người được follow
                var likerer = await _accountRepository.GetAccountByIdAsync(dto.AccountId);
                if (likerer != null)
                {
                    var message = $"{likerer.AccountProfile?.FirstName} has liked your post.";
                    await _notificationService.CreateAndSendAsync(new reqNotificationDTO
                    {
                        UserId = dto.AccountId,
                        Message = message,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    });
                }
            }

            return success;
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
                var policies = await _policyService.GetAllActivePoliciesAsync();

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
