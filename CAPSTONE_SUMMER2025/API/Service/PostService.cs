using System.Globalization;
using System.Text;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

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
        private readonly CAPSTONE_SUMMER2025Context _context;

        public PostService(IPostRepository repository, IMapper mapper, IChatGPTService chatGPTService, IPolicyService policyService, IFilebaseHandler filebase, IAccountRepository accountRepository, INotificationService notificationService, CAPSTONE_SUMMER2025Context context)
        {
            _repository = repository;
            _mapper = mapper;
            _chatGPTService = chatGPTService;
            _policyService = policyService;
            _filebase = filebase;
            _accountRepository = accountRepository;
            _notificationService = notificationService;
            _context = context;
        }

        public async Task<resPostDTO> GetPostByPostId(int postId)
        {
            var post = await _repository.GetPostByPostIdAsync(postId);
            if (post == null) return null;

            var dto = new resPostDTO
            {
                PostId = post.PostId,
                AccountId = post.AccountId,
                Content = post.Content,
                Title = post.Title,
                CreateAt = post.CreateAt,
                PostMedia = post.PostMedia.Select(m => new PostMediaDTO
                {
                    MediaUrl = m.MediaUrl,
                    DisplayOrder = m.DisplayOrder
                }).ToList()
            };

            return dto;
        }

        public async Task<PagedResult<resPostDTO>> GetPostsByAccountIdAsync(int accountId, int pageNumber, int pageSize, int currentAccountId)
        {
            var pagedPosts = await _repository.GetPostsByAccountId(accountId, pageNumber, pageSize, currentAccountId);

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

        // hàm lấy ra comment theo postid
        public async Task<PagedResult<PostCommentDTO>> GetPostCommentByPostId(int postId, int pageNumber, int pageSize)
        {
            try
            {
                var pagedPostComments = await _repository.GetPostCommentByPostId(postId, pageNumber, pageSize);

                if (pagedPostComments == null)
                    return null;

                var postCommentDTOs = new List<PostCommentDTO>();

                foreach (var pc in pagedPostComments.Items)
                {
                    var profile = await _context.AccountProfiles
                        .Where(ap => ap.AccountId == pc.AccountId)
                        .Select(ap => new AccountInforDTOcs
                        {
                            AccountId = (int) ap.AccountId,
                            AvatarUrl = ap.AvatarUrl,
                            FullName = ap.FirstName + " " + ap.LastName
                        })
                        .FirstOrDefaultAsync();

                    var dto = new PostCommentDTO
                    {
                        PostcommentId = pc.PostcommentId,
                        PostId = pc.PostId,
                        Content = pc.Content,
                        CommentAt = pc.CommentAt,
                        ParentCommentId = pc.ParentCommentId,
                        numChildComment = await _repository.CountChildCommentByPostCommentId(pc.ParentCommentId),
                        AccountInfor = profile
                    };

                    postCommentDTOs.Add(dto);
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
            var accountID = await _repository.GetAccountIdByPostIDAsync(dto.PostId);
            if ( success && accountID.HasValue && accountID.Value != dto.AccountId )
            {
                var likerer = await _accountRepository.GetAccountByIdAsync(dto.AccountId);
                if (likerer != null)
                {
                    var message = $"{likerer.AccountProfile?.FirstName} has liked your post.";
                    await _notificationService.CreateAndSendAsync(new reqNotificationDTO
                    {
                        UserId = accountID.Value,
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

        // hàm lấy ra danh sách người like bài post
        public async Task<PagedResult<PostLikeDTO>> GetPostLikeByPostId(int postId, int pageNumber, int pageSize)
        {
            var pagedPostLikes = await _repository.GetPostLikeByPostId(postId, pageNumber, pageSize);

            if (pagedPostLikes == null)
                return null;

            var postLikeDTOs = _mapper.Map<List<PostLikeDTO>>(pagedPostLikes.Items);

            return new PagedResult<PostLikeDTO>(
                postLikeDTOs,
                pagedPostLikes.TotalCount,
                pagedPostLikes.PageNumber,
                pagedPostLikes.PageSize
            );
        }


        //hàm lấy ra comment con theo parrentPostCommentId
        public async Task<PagedResult<PostCommentDTO>> GetPostCommentChildByPostIdAndParentCommentId(
       int pageNumber, int pageSize, int parrentCommentId)
        {
            try
            {
                var pagedPostComments = await _repository
                    .GetPostCommentChildByPostIdAndParentCommentId(pageNumber, pageSize, parrentCommentId);

                if (pagedPostComments == null)
                    return null;

                // Lấy danh sách Account_ID duy nhất
                var accountIds = pagedPostComments.Items
                    .Select(p => p.AccountId)
                    .Distinct()
                    .ToList();

                // Truy vấn thông tin profile từ _context
                var accountInfoMap = await _context.AccountProfiles
                    .Where(ap => accountIds.Contains(ap.AccountId))
                    .Select(ap => new AccountInforDTOcs
                    {
                        AccountId = (int) ap.AccountId,
                        AvatarUrl = ap.AvatarUrl,
                        FullName = ap.FirstName + " " + ap.LastName
                    })
                    .ToDictionaryAsync(ap => ap.AccountId);

                var postCommentDTOs = new List<PostCommentDTO>();

                foreach (var pc in pagedPostComments.Items)
                {
                    accountInfoMap.TryGetValue((int)pc.AccountId, out var accountDto);

                    postCommentDTOs.Add(new PostCommentDTO
                    {
                        PostcommentId = pc.PostcommentId,
                        PostId = pc.PostId,
                        Content = pc.Content,
                        CommentAt = pc.CommentAt,
                        ParentCommentId = pc.ParentCommentId,
                        numChildComment = await _repository.CountChildCommentByPostCommentId(pc.PostcommentId),
                        AccountInfor = accountDto
                    });
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


        // hàm tạo bài post
        public async Task<string> CreatePost(ReqPostDTO reqPostDTO)
        {
            try
            {
                // Danh sách chính sách kiểm duyệt
                var policies = await _policyService.GetAllActivePoliciesAsync();

                // Kiểm duyệt nội dung(comment lại để tránh tốn token)
                //var result = await _chatGPTService.ModeratePostContentAsync(reqPostDTO, policies);
                //if (result.Contains("Violation"))
                //{
                //    return result;
                //}

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

        // hàm tạo post comment
        public async Task<string> CreatePostComment(reqPostCommentDTO reqPostCommentDTO)
        {
            try
            {
                var success = await _repository.CreatePostComment(reqPostCommentDTO);
                if(reqPostCommentDTO.ParentCommentId == null)
                {
                    var accountID = await _repository.GetAccountIdByPostIDAsync(reqPostCommentDTO.PostId);
                    if (success)
                    {
                        var commenter = await _accountRepository.GetAccountByIdAsync(accountID.Value);
                        if (commenter != null)
                        {
                            var message = $"{commenter.AccountProfile?.FirstName} has comment on your post.";
                            await _notificationService.CreateAndSendAsync(new reqNotificationDTO
                            {
                                UserId = accountID.Value,
                                Message = message,
                                CreatedAt = DateTime.UtcNow,
                                IsRead = false
                            });
                        }
                    }
                }
                else
                {
                    var accountID = await _repository.GetAccountIdByCommentId((int)reqPostCommentDTO.ParentCommentId);
                    if (success)
                    {
                        var commenter = await _accountRepository.GetAccountByIdAsync(accountID.Value);
                        if (commenter != null)
                        {
                            var message = $"{commenter.AccountProfile?.FirstName} has comment on your comment.";
                            await _notificationService.CreateAndSendAsync(new reqNotificationDTO
                            {
                                UserId = accountID.Value,
                                Message = message,
                                CreatedAt = DateTime.UtcNow,
                                IsRead = false
                            });
                        }
                    }
                }

                if (!success)
                    return "Account hoặc Post không tồn tại";

                return "Successfully";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // hàm cập nhật comment
        public async Task<bool> UpdateCommentAsync(UpdateCommentDTO dto)
        {
            return await _repository.UpdateCommentAsync(dto.PostcommentId, dto.Content);
        }
        // hàm xóa comment
        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            return await _repository.DeleteCommentAsync(commentId);
        }

        // hàm cập nhật post
        public async Task<bool> UpdatePostAsync(int postId, reqUpdatePostDTO dto)
        {
            return await _repository.UpdatePostAsync(postId, dto.Title, dto.Content);
        }

        //hàm xóa post
        public async Task<bool> DeletePostAsync(int postId)
        {
            return await _repository.DeletePostAsync(postId);
        }


        // tìm kiếm bài post
        public async Task<PagedResult<resPostDTO>> SearchPostsAsync(string searchText, int pageNumber, int pageSize, int currentAccountId)
        {

            // ✅ Check user tồn tại và còn hoạt động
            var account = await _accountRepository.GetAccountByIdAsync(currentAccountId);
            if (account == null )
                throw new UnauthorizedAccessException("Tài khoản không hợp lệ hoặc đã bị vô hiệu hóa.");

            var query = _repository.GetSearchPosts(searchText, currentAccountId)
                .OrderByDescending(p => p.LikeCount); // 🔥 Ưu tiên bài nhiều like

            var totalCount = query.Count();

            var pagedItems = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList(); // hoặc ToListAsync nếu IQueryable EF vẫn dùng

            return new PagedResult<resPostDTO>(pagedItems, totalCount, pageNumber, pageSize);
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var normalized = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    builder.Append(c);
            }
            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
        public async Task<List<FeedItemDTO>> GetNewFeedAsync(int userId, int page, int pageSize)
        {
            return await _repository.GetRecommendedFeedAsync(userId, page, pageSize);
        }

        // hàm ẩn bài post
        public async Task<bool> HidePostAsync(HidePostRequestDTO dto)
        {
            return await _repository.HidePostAsync(dto.AccountId, dto.PostId);
        }

        // hàm ra tất cả ReportReason
        public async Task<List<ReportReasonDTO>> GetAllReportReasonAsync()
        {
            var list = await _repository.GetAllReportReasonAsync();
            return list.Select(r => new ReportReasonDTO
            {
                ReasonId = r.ReasonId,
                Reason = r.Reason,
                Description = r.Description
            }).ToList();
        }

        // hàm lấy ra ReportReason theo id
        public async Task<ReportReasonDTO?> GetReportReasonByIdAsync(int id)
        {
            var r = await _repository.GetReportReasonByIdAsync(id);
            if (r == null) return null;

            return new ReportReasonDTO
            {
                ReasonId = r.ReasonId,
                Reason = r.Reason,
                Description = r.Description
            };
        }

        // hàm tạo ReportReason
        public async Task<ReportReasonDTO> CreateReportReasonAsync(CreateReportReasonDTO dto)
        {
            var r = new ReportReason
            {
                Reason = dto.Reason,
                Description = dto.Description
            };
            var created = await _repository.CreateReportReasonAsync(r);
            return new ReportReasonDTO
            {
                ReasonId = created.ReasonId,
                Reason = created.Reason,
                Description = created.Description
            };
        }

        //hàm cập nhật ReportReason
        public async Task<bool> UpdateReportReasonAsync(int id, CreateReportReasonDTO dto)
        {
            return await _repository.UpdateReportReasonAsync(new ReportReason
            {
                ReasonId = id,
                Reason = dto.Reason,
                Description = dto.Description
            });
        }

        //hàm xóa ReportReason
        public async Task<bool> DeleteReportReasonAsync(int id)
        {
            return await _repository.DeleteReportReasonAsync(id);
        }

        // hàm tạo post report
        public async Task<PostReportDTO> CreateReportAsync(CreatePostReportDTO dto)
        {

            var account = await _accountRepository.GetAccountByAccountIDAsync(dto.AccountId);
            if (account == null)
                throw new ArgumentException("Account does not exist.");

            var post = await _repository.GetPostByPostIdAsync(dto.PostId);
            if (post == null)
                throw new ArgumentException("Post does not exist.");

            var reason = await _repository.GetReportReasonByIdAsync(dto.ReasonId);
            if (reason == null)
                throw new ArgumentException("Reason does not exist.");

            var entity = new PostReport
            {
                AccountId = dto.AccountId,
                PostId = dto.PostId,
                ReasonId = dto.ReasonId,
                Status = Utils.Constants.PostReportStatusConst.PENDING,
            };

            var created = await _repository.CreatePostReportAsync(entity);
            return _mapper.Map<PostReportDTO>(created);
        }


        // tạo mới internshippost
        public async Task CreateInternshipPostAsync(CreateInternshipPostDTO dto)
        {
            var post = new InternshipPost
            {
                StartupId = dto.Startup_ID,
                PositionId = dto.Position_ID,
                Description = dto.Description,
                Requirement = dto.Requirement,
                Benefits = dto.Benefits,
                Deadline = dto.Deadline,
                CreateAt = DateTime.Now,
                Status = Utils.Constants.StatusInternshipPost.ACTIVE
            };

            await _repository.AddInternshipPostAsync(post);
        }
        //lấy ra feed của startup
        public async Task<PagedResult<FeedItemDTO>> GetStartupFeedAsync(int startupId, int page, int pageSize)
        {
            int skip = (page - 1) * pageSize;
            var (items, totalCount) = await _repository.GetStartupFeedItemsAsync(startupId, skip, pageSize);
            return new API.DTO.AccountDTO.PagedResult<FeedItemDTO>(items, totalCount, page, pageSize);
        }

        // hàm share post
        public async Task<Post> SharePostAsync(SharePostRequest request)
        {
            var sharedPost = new Post
            {
                AccountId = request.AccountId ?? throw new ArgumentException("AccountId không được null"),
                StartupId = request.StartupId,
                PostShareId = request.OriginalPostId,
                Content = request.Content,
                Title = request.Title,
                CreateAt = DateTime.Now
            };

            await _repository.ShareAsync(sharedPost);
            return sharedPost;
        }
    }
}
