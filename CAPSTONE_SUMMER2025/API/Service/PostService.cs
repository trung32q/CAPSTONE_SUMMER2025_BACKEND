using System.Globalization;
using System.Text;
using API.DTO.AccountDTO;
using API.DTO.NotificationDTO;
using API.DTO.PostDTO;
using API.DTO.StartupDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Service.Interface;
using API.Utils.Constants;
using AutoMapper;
using Google.Cloud.AIPlatform.V1;
using Infrastructure.Models;
using Infrastructure.Repository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using static Google.Cloud.AIPlatform.V1.LogprobsResult.Types;

namespace API.Service
{
    public class PostService : IPostService
    {

        private readonly IPostRepository _repository;
        private readonly IFilebaseHandler _filebase;
        private readonly IMapper _mapper;
        private readonly IChatGPTService _chatGPTService;
        private readonly IChatGPTRepository _chatGPTRepository;
        private readonly IFileHandlerService _fileHandlerService;
        private readonly IPolicyService _policyService;
        private readonly IAccountRepository _accountRepository;
        private readonly INotificationService _notificationService;
        private readonly IStartupService _startupService;
        private readonly CAPSTONE_SUMMER2025Context _context;

        public PostService(IStartupService startupService,IChatGPTRepository chatGPTRepository, IFileHandlerService fileHandlerService,IPostRepository repository, IMapper mapper, IChatGPTService chatGPTService, IPolicyService policyService, IFilebaseHandler filebase, IAccountRepository accountRepository, INotificationService notificationService, CAPSTONE_SUMMER2025Context context)
        {
            _repository = repository;
            _mapper = mapper;
            _chatGPTService = chatGPTService;
            _policyService = policyService;
            _filebase = filebase;
            _accountRepository = accountRepository;
            _notificationService = notificationService;
            _context = context;
            _fileHandlerService = fileHandlerService;
            _chatGPTRepository = chatGPTRepository;
            _startupService = startupService;
        }

        public async Task<resPostDTO> GetPostByPostId(int postId)
        {
            var post = await _repository.GetPostByPostIdAsync(postId);
            if (post == null) return null;

            var dto = new resPostDTO
            {
                PostId = post.PostId,
                StartupId = (int) post.StartupId,
                AccountId = post.AccountId,
                Content = post.Content,
                Title = post.Title,
                CreateAt = post.CreateAt,
                Schedule = post.Schedule,
                FullName =  post.StartupId == null ?  (post.Account.AccountProfile.FirstName + " " + post.Account.AccountProfile.LastName) : post.Startup.StartupName,
                AvatarUrl = post.StartupId == null ?  post.Account.AccountProfile.AvatarUrl : post.Startup.Logo,
                PostShareId = post.PostShareId,
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
            var account = await _accountRepository.GetAccountByAccountIDAsync(accountId);
            string fullName = account?.AccountProfile != null
    ? account.AccountProfile.FirstName + " " + account.AccountProfile.LastName
    : "";
            string avatarUrl = account?.AccountProfile?.AvatarUrl;
            if (pagedPosts == null)
                return null;

            var postDTOs = pagedPosts.Items.Select(post => new resPostDTO
            {
                PostId = post.PostId,
                AccountId = post.AccountId,
                Content = post.Content,
                Title = post.Title,
                CreateAt = post.CreateAt,
                PostShareId = post.PostShareId,
                Schedule = post.Schedule,
                LikeCount = post.PostLikes?.Count ?? 0,
                FullName = fullName,
                AvatarUrl = avatarUrl,
                PostMedia = post.PostMedia != null
                    ? post.PostMedia.Select(pm => new PostMediaDTO
                    {
                        MediaUrl = pm.MediaUrl,
                        DisplayOrder = pm.DisplayOrder
                    }).ToList()
                    : new List<PostMediaDTO>()
            }).ToList();

            foreach (var dto in postDTOs)
            {
                if (dto.PostMedia != null)
                {
                    foreach (var media in dto.PostMedia)
                    {
                        if (!string.IsNullOrEmpty(media.MediaUrl))
                        {
                            var publicIdWithType = media.MediaUrl.Contains("/")
                                ? media.MediaUrl
                                : $"image/{media.MediaUrl}";

                            Console.WriteLine($"Generating URL for publicIdWithType: {publicIdWithType}");
                            media.MediaUrl = _filebase.GeneratePreSignedUrl(publicIdWithType);
                        }
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

        //tìm kiếm internshippost của startup
        public async Task<PagedResult<InternshipPostDTO>> GetSearchStartupInternshipPost(int startupId, string? keyword, int pageNumber, int pageSize)
        {
            var query = _repository.GetStartupInternshipPost(startupId);

            // Lọc sơ bộ nếu có keyword
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p =>
                    p.Description.Contains(keyword) ||
                    p.Requirement.Contains(keyword) ||
                    p.Benefits.Contains(keyword));
            }

            // Tải dữ liệu về bộ nhớ
            var allItems = await query
                .Select(p => new
                {
                    Post = p,
                    StartupName = p.Startup.StartupName,
                    StartupLogo = p.Startup.Logo
                })
                .ToListAsync();

            // Lọc không dấu
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var normKeyword = RemoveDiacritics(keyword);
                allItems = allItems.Where(x =>
                    RemoveDiacritics(x.Post.Description ?? "").Contains(normKeyword) ||
                    RemoveDiacritics(x.Post.Requirement ?? "").Contains(normKeyword) ||
                    RemoveDiacritics(x.Post.Benefits ?? "").Contains(normKeyword))
                    .ToList();
            }

            var totalCount = allItems.Count;

            var items = allItems
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InternshipPostDTO
                {
                    InternshipId = x.Post.InternshipId,
                    StartupId = (int)x.Post.StartupId,
                    PositionId = (int) x.Post.PositionId,
                    Position = x.Post.Position.Title,
                    Description = x.Post.Description,
                    Requirement = x.Post.Requirement,
                    Benefits = x.Post.Benefits,
                    CreateAt = x.Post.CreateAt,
                    Deadline = x.Post.Deadline,
                    Status = x.Post.Status,
                    Address = x.Post.Address,
                    Salary = x.Post.Salary,
                    StartupName = x.StartupName,
                    StartupLogo = x.StartupLogo
                })
                .ToList();

            return new PagedResult<InternshipPostDTO>(items, totalCount, pageNumber, pageSize);
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
            var targetUrl = $"/post/{dto.PostId}";
            if ( success && accountID.HasValue && accountID.Value != dto.AccountId )
            {
                var likerer = await _accountRepository.GetAccountByIdAsync(dto.AccountId);
                if (likerer != null)
                {
                    var message = $" has liked your post.";
                    await _notificationService.CreateAndSendAsync(new reqNotificationDTO
                    {
                        UserId = accountID.Value,
                        Message = message,
                        CreatedAt = DateTime.Now,
                        IsRead = false,
                        senderid = dto.AccountId,
                        NotificationType = NotiConst.LIKE,
                        TargetURL = targetUrl
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
                var result = await _chatGPTService.ModeratePostContentAsync(reqPostDTO, policies);
                if (result.Contains("Violation"))
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
                        if(accountID == null)
                        {
                            return "Successfully";
                        }

                        var commenter = await _accountRepository.GetAccountByIdAsync(accountID.Value);
                        if (commenter != null&& reqPostCommentDTO.AccountId != accountID)
                        {
                            var targetUrl = $"/post/{reqPostCommentDTO.PostId}";
                            var message = $" has comment on your post.";
                            await _notificationService.CreateAndSendAsync(new reqNotificationDTO
                            {
                                UserId = accountID.Value,
                                Message = message,
                                CreatedAt = DateTime.Now,
                                IsRead = false,
                                senderid = reqPostCommentDTO.AccountId,
                                NotificationType = NotiConst.COMMENT,
                                TargetURL = targetUrl
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
                            var targetUrl = $"/post/{reqPostCommentDTO.PostId}";
                            var message = $"has comment on your comment.";
                            await _notificationService.CreateAndSendAsync(new reqNotificationDTO
                            {
                                UserId = accountID.Value,
                                Message = message,
                                CreatedAt = DateTime.Now,
                                IsRead = false,
                                senderid = reqPostCommentDTO.AccountId,
                                NotificationType = NotiConst.COMMENT,
                                TargetURL = targetUrl
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

        // tìm kiếm bài post theo startupId
        public async Task<PagedResult<PostSearchDTO>> GetSearchPostsByStartup(int startupId, string? keyword, int pageNumber, int pageSize)
        {
            var query = _repository.GetSearchPostsByStartup(startupId);

            // Nếu có keyword, lọc sơ bộ Contains để giảm dữ liệu tải về
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p =>
                    p.Title.Contains(keyword) ||
                    p.Content.Contains(keyword));
            }

            // Lấy toàn bộ danh sách sau khi lọc sơ bộ
            var allItems = await query
                .Select(p => new
                {
                    Post = p,
                    Medias = p.PostMedia.OrderBy(m => m.DisplayOrder).ToList(),
                    StartupName = p.Startup.StartupName,
                    StartupLogo = p.Startup.Logo
                })
                .ToListAsync();

            // Nếu có keyword, lọc không dấu trong C#
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var normKeyword = RemoveDiacritics(keyword);
                allItems = allItems.Where(x =>
                    RemoveDiacritics(x.Post.Title ?? "").Contains(normKeyword) ||
                     RemoveDiacritics(x.Post.Content ?? "").Contains(normKeyword))
                    .ToList();
            }

            var totalCount = allItems.Count;

            // Phân trang dữ liệu
            var items = allItems
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PostSearchDTO
                {
                    PostID = x.Post.PostId,
                    Title = x.Post.Title,
                    CreateAt = (DateTime)x.Post.CreateAt,
                    Content = x.Post.Content,
                    StartupName = x.StartupName,
                    StartupLogo = x.StartupLogo,
                    Media = x.Medias.Select(m => new PostMediaDTO
                    {
                        MediaUrl = m.MediaUrl,
                        DisplayOrder = m.DisplayOrder
                    }).ToList()
                })
                .ToList();

            return new PagedResult<PostSearchDTO>(items, totalCount, pageNumber, pageSize);
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
                Status = Utils.Constants.StatusInternshipPost.ACTIVE,
                Salary = dto.Salary,
                Address = dto.Address,
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
                PostShareId = request.OriginalPostId,
                Content = request.Content,
                Title = request.Title,
                CreateAt = DateTime.Now
            };

            await _repository.ShareAsync(sharedPost);
            var targetUrl = $"/post/{request.OriginalPostId}";
            var Account = await _accountRepository.GetAccountByAccountIDAsync((int)request.AccountId);
                // Gửi thông báo tới người được share
                var Accountid = await _repository.GetAccountIdByPostIDAsync(request.OriginalPostId);
                if (Accountid != null)
                {
                    var message = $" has share your post.";
                    await _notificationService.CreateAndSendAsync(new reqNotificationDTO
                    {
                        UserId = (int)request.AccountId,
                        Message = message,
                        CreatedAt = DateTime.Now,
                        IsRead = false,
                        senderid = Account.AccountId,
                        NotificationType = NotiConst.Share,
                        TargetURL = targetUrl
                    });
                }
            
            return sharedPost;
        }
       
        //lấy ra những bài post được hẹn nhưng chưa đăng
        public async Task<List<PostScheduleDTO>> GetScheduledPostsAsync()
        {
            var posts = await _repository.GetScheduledPostsAsync();
            return posts.Select(p => new PostScheduleDTO
            {
                PostId = p.PostId,
                Title = p.Title,
                CreateAt = p.CreateAt,
                Schedule = p.Schedule
            }).ToList();
        }

        //publish bài post
        public async Task<bool> PublishPostAsync(int postId)
        {
            var post = await _repository.GetPostByPostIdAsync(postId);
            if (post == null)
                return false;
        
            // Nếu chưa đến giờ lên lịch thì không cho đăng
            //if (post.Schedule.HasValue && post.Schedule > DateTime.Now)
            //    return false;

            // Cho đăng bài
            post.CreateAt = post.Schedule;
            await _repository.SaveChangesAsync();
            return true;
        }

        //cập nhật status của internshippost
        public async Task<bool> UpdateInternshipPostAsync(int internshipPostId)
        {
            var post = await _repository.GetInternshipPostByIdAsync(internshipPostId);
            if (post == null) return false;

            if (post.Status == Utils.Constants.StatusInternshipPost.ACTIVE)
            {
                post.Status = Utils.Constants.StatusInternshipPost.DEACTIVE;
            }
            else
            {
                post.Status = Utils.Constants.StatusInternshipPost.ACTIVE;
            }
            await _repository.UpdateInternshipPostAsync(post);
            await _repository.SaveChangesAsync();
            return true;
        }

        //apply cv
        

      
        public async Task<PagedResult<resPostDTO>> GetPostsByStartupIdAsync(int startupId, int pageNumber, int pageSize)
        {
            var pagedPosts = await _repository.GetPostsByStartupId(startupId, pageNumber, pageSize);
            var startup = await _repository.GetStartupByIdAsync(startupId);
             if (pagedPosts == null)
                return null;

            // Map thủ công từng Post sang resPostDTO như hướng dẫn ở trên, ví dụ:
            var postDTOs = pagedPosts.Items.Select(post => new resPostDTO
            {
                PostId = post.PostId,
                StartupId = (int)post.StartupId,
                Content = post.Content,
                Title = post.Title,
                CreateAt = post.CreateAt,
                PostShareId = post.PostShareId,
                Schedule = post.Schedule,
                LikeCount = post.PostLikes?.Count ?? 0,
                FullName = startup.StartupName,
                AvatarUrl = startup.Logo,
                PostMedia = post.PostMedia != null
                    ? post.PostMedia.Select(pm => new PostMediaDTO
                    {
                        MediaUrl = pm.MediaUrl,
                    }).ToList()
                    : new List<PostMediaDTO>()
            }).ToList();

            return new PagedResult<resPostDTO>(
                postDTOs,
                pagedPosts.TotalCount,
                pagedPosts.PageNumber,
                pagedPosts.PageSize
            );
        }
        public async Task<PagedResult<InternshipPostDTO>> GetAllInternshipPostsAsync(int pageNumber, int pageSize,int startupid)
        {
            var paged = await _repository.GetInternshipPostsAsync(pageNumber, pageSize,startupid);
            var dtos = paged.Items.Select(x => new InternshipPostDTO
            {
                InternshipId = x.InternshipId,
                StartupId = (int)x.StartupId,
                Position = x.Position.Title,
                Description = x.Description,
                Requirement = x.Requirement,
                Benefits = x.Benefits,
                CreateAt = x.CreateAt,
                Deadline = x.Deadline,
                Status = x.Status,
                Address = x.Address,
                Salary = x.Salary
            }).ToList();

            return new PagedResult<InternshipPostDTO>(dtos, paged.TotalCount, paged.PageNumber, paged.PageSize);
        }

        //lấy ra internship post theo id
        public async Task<InternshipPostDetailDTO?> GetInternshipPostDetailAsync(int internshipPostId)
        {
            var entity = await _repository.GetInternshipPostWithNavigationAsync(internshipPostId);
            if (entity == null)
                return null;

            return new InternshipPostDetailDTO
            {
                InternshipId = entity.InternshipId,
                StartupId = (int) entity.StartupId,
                PositionId = (int) entity.PositionId,
                Description = entity.Description,
                Requirement = entity.Requirement,
                Benefits = entity.Benefits,
                CreateAt = entity.CreateAt,
                Deadline = entity.Deadline,
                Status = entity.Status,
                Address = entity.Address,
                Salary = entity.Salary,
                StartupName = entity.Startup?.StartupName,
                Logo = entity.Startup?.Logo,
                PositionTitle = entity.Position?.Title
            };
        }

        public async Task<bool> UpdateInternshipPostAsync(int internshipPostId, UpdateInternshipPostDTO dto)
        {
            return await _repository.UpdateInternshipPostAsync(
                internshipPostId,
                new InternshipPost
                {
                    Deadline = dto.Deadline,
                    Requirement = dto.Requirement,
                    Salary = dto.Salary,
                    Address = dto.Address,
                    Benefits = dto.Benefits,
                    Description = dto.Description
                });
           
        }


        public async Task<bool> UpdateAsync(int id, InternshipPostUpdateDTO dto)
        {
            var post = await _repository.GetInternshipPostByIdAsync(id);
            if (post == null) return false;

            post.Description = dto.Description ?? post.Description;
            post.Requirement = dto.Requirement ?? post.Requirement;
            post.Benefits = dto.Benefits ?? post.Benefits;
            post.Deadline = dto.Deadline ?? post.Deadline;
            post.Address = dto.Address ?? post.Address;
            post.Salary = dto.Salary ?? post.Salary;

            _repository.UpdateInternshipPost(post);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<List<TopInternshipPostDTO>> GetTopInternshipPostsAsync(int top)
        {
            return await _repository.GetTopInternshipPostsByCVCountAsync(top);
        }
    }
}
