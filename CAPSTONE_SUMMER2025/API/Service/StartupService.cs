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
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;

namespace API.Service
{
    public class StartupService : IStartupService
    {
        private readonly IStartupRepository _repo;
        private readonly IPostRepository _postRepo;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly IFilebaseHandler _filebaseHandler;
        private readonly ILogger<StartupService> _logger;
        private readonly INotificationService _notificationService;
        private readonly ICVRepository _cvRepository;
      
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public StartupService(ICVRepository cVRepository,IConfiguration configuration,IEmailService emailService,IStartupRepository repo, IMapper mapper, IFilebaseHandler filebaseHandler, ILogger<StartupService> logger, IAccountRepository accountRepository, CAPSTONE_SUMMER2025Context conntext, INotificationService notificationService, IPostRepository postRepo)
        {
            _cvRepository = cVRepository;
            _repo = repo;
            _mapper = mapper;
            _filebaseHandler = filebaseHandler;
            _logger = logger;
            _accountRepository = accountRepository;
            _context = conntext;
            _notificationService = notificationService;
            _postRepo = postRepo;
            _emailService = emailService;
            _config = configuration;    
        }
        public async Task<int> CreateStartupAsync(CreateStartupRequest request)
        {
            string logoUrl = null;
            string backgroundUrl = null;

            // Upload logo 
            if (request.Logo != null)
                logoUrl = await _filebaseHandler.UploadMediaFile(request.Logo);

            // Upload background
            if (request.BackgroundUrl != null)
                backgroundUrl = await _filebaseHandler.UploadMediaFile(request.BackgroundUrl);
            // 1. Tạo Startup
            var startup = _mapper.Map<Startup>(request);
            await _repo.AddStartupAsync(startup);

            // 2. Tạo role Founder
            var founderRole = new RoleInStartup
            {
                RoleName = "Founder",
                StartupId = startup.StartupId
            };
            await _repo.AddRoleAsync(founderRole);
            var founderPermission = new PermissionInStartup
            {
                RoleId = founderRole.RoleId,
                CanManagePost = true,
                CanManageCandidate = true,
                CanManageChatRoom = true,
                CanManageMember = true,
                CanManageMilestone = true
            };
            await _repo.AddPermissionAsync(founderPermission);
            // 3. Gán creator vào làm Founder
            var founderMember = new StartupMember
            {
                StartupId = startup.StartupId,
                AccountId = request.CreatorAccountId,
                RoleId = founderRole.RoleId,
                JoinedAt = DateTime.Now
            };
            await _repo.AddMemberAsync(founderMember);

            // 4. Invite member (nếu có)
            if (request.InviteAccountIds != null && request.InviteAccountIds.Any())
            {
                var memberRole = new RoleInStartup
                {
                    RoleName = "Member",
                    StartupId = startup.StartupId
                };
                await _repo.AddRoleAsync(memberRole);

                foreach (var accId in request.InviteAccountIds.Distinct())
                {
                    if (accId == request.CreatorAccountId) continue;
                    var member = new StartupMember
                    {
                        StartupId = startup.StartupId,
                        AccountId = accId,
                        RoleId = memberRole.RoleId,
                        JoinedAt = DateTime.Now
                    };
                    await _repo.AddMemberAsync(member);
                }
            }

            // 5. Gán Category cho Startup
            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                foreach (var catId in request.CategoryIds.Distinct())
                {
                    var sc = new StartupCategory
                    {
                        StartupId = startup.StartupId,
                        CategoryId = catId
                    };
                    await _repo.AddStartupCategoryAsync(sc);
                }
            }

            return startup.StartupId;
        }
        public async Task<PagedResult<ResStartupDTO>> GetAllStartupsAsync(int pageNumber, int pageSize, int? categoryId = null)
        {
            var pagedResult = await _repo.GetAllStartupsAsync(pageNumber, pageSize, categoryId);

            var dtoList = pagedResult.Items.Select(s => new ResStartupDTO
            {
                Startup_ID = s.StartupId,
                Startup_Name = s.StartupName,
                AbbreviationName = s.AbbreviationName,
                Mission = s.Mission,
                Vision = s.Vision,
                Stage = s.Stage?.StageName,
                createAt = (DateTime)s.CreateAt,
                Description = s.Description,
                Logo = !string.IsNullOrEmpty(s.Logo)
                    ? _filebaseHandler.GeneratePreSignedUrl(
                        s.Logo.Contains("/") ? s.Logo : $"image/{s.Logo}")
                    : null,
                BackgroundUrl = !string.IsNullOrEmpty(s.BackgroundUrl)
                    ? _filebaseHandler.GeneratePreSignedUrl(
                        s.BackgroundUrl.Contains("/") ? s.BackgroundUrl : $"image/{s.BackgroundUrl}")
                    : null,
                WebsiteURL = s.WebsiteUrl,
                Email = s.Email,
                Status = s.Status,
                Categories = s.StartupCategories?
                    .Select(sc => sc.Category.CategoryName).ToList() ?? new List<string>(),
                FollowerCount = s.Subcribes?.Count(x => x.FollowingStartUpId == s.StartupId) ?? 0
            }).ToList();

            return new PagedResult<ResStartupDTO>(dtoList, pagedResult.TotalCount, pageNumber, pageSize);
        }


        public async Task<bool> IsMemberOfAnyStartup(int accountId)
       => await _repo.IsMemberOfAnyStartup(accountId);

        // hàm tạo chatroom
        public async Task<ChatRoom> CreateChatRoomAsync(CreateChatRoomDTO dto)
        {
            var chatRoom = new ChatRoom
            {
                RoomName = dto.RoomName,
                StartupId = dto.StartupId
            };

            var createdRoom = await _repo.CreateChatRoomAsync(chatRoom);

            var accountInfor = await _accountRepository.GetAccountByAccountIDAsync(dto.CreatorAccountId);

            var creatorMember = new ChatRoomMember
            {
                ChatRoomId = createdRoom.ChatRoomId,
                AccountId = dto.CreatorAccountId,
                MemberTitle = string.IsNullOrWhiteSpace(dto.MemberTitle) ? accountInfor.AccountProfile.FirstName + " " + accountInfor.AccountProfile.LastName : dto.MemberTitle,
                CanAdministerChannel = true,
                JoinedAt = DateTime.Now
            };

            await _repo.AddMemberAsync(creatorMember);
            return createdRoom;
        }

        // hàm thêm người vào chatroom
        public async Task AddMembersToChatRoomAsync(AddMembersDTO dto)
        {
            var isAdmin = await _repo.IsChatRoomAdminAsync(dto.ChatRoomId, dto.CurrentUserId);
            if (!isAdmin)
                throw new UnauthorizedAccessException("Bạn không có quyền thêm thành viên.");

            var room = await _repo.GetChatRoomByIdAsync(dto.ChatRoomId);

            var accountIds = dto.MembersToAdd.Select(m => m.AccountId).ToList();
            var validMemberIds = await _repo.FilterValidStartupMembersAsync((int)room.StartupId, accountIds);

            var existingMemberIds = await _repo.GetExistingChatRoomMemberIdsAsync(dto.ChatRoomId);
            var idsToAdd = validMemberIds.Except(existingMemberIds).ToList();

            string fullname = "";

            if (!idsToAdd.Any()) return;

            var newMembers = new List<ChatRoomMember>();

            foreach (var m in dto.MembersToAdd)
            {
                if (!idsToAdd.Contains(m.AccountId)) continue;

                var accountInfor = await _accountRepository.GetAccountByAccountIDAsync(m.AccountId);
                fullname = accountInfor.AccountProfile.FirstName + " " + accountInfor.AccountProfile.LastName;
                var member = new ChatRoomMember
                {
                    ChatRoomId = dto.ChatRoomId,
                    AccountId = m.AccountId,
                    MemberTitle = string.IsNullOrWhiteSpace(m.MemberTitle) ? fullname : m.MemberTitle,
                    CanAdministerChannel = false,
                    JoinedAt = DateTime.Now
                };


                newMembers.Add(member);
            }

            await _repo.AddMembersAsync(newMembers);
        }


        //lấy ra các member trong startup theo startupId
        public async Task<List<StartupMemberDTO>> GetMembersByStartupIdAsync(int startupId)
        {
            var entities = await _repo.GetByStartupIdAsync(startupId);

            var result = entities.Select(sm => new StartupMemberDTO
            {
                AccountId = (int)sm.AccountId,
                Memberid =sm.StartupMemberId,
                FullName = $"{sm.Account?.AccountProfile?.FirstName} {sm.Account?.AccountProfile?.LastName}",
                RoleName = sm.Role?.RoleName,
                AvatarUrl = sm.Account.AccountProfile.AvatarUrl,
                JoinAT = (DateTime)sm.JoinedAt,
                Email = sm.Account.Email
            }).ToList();

            return result;
        }

        // hàm lấy ra các member trong 1 chatroom theo chatRoomId
        public async Task<List<ChatRoomMemberDTO>> GetMembersByChatRoomIdAsync(int chatRoomId)
        {
            try
            {
                var members = await _repo.GetMembersByChatRoomIdAsync(chatRoomId);

                return members.Select(m => new ChatRoomMemberDTO
                {
                    AccountId = (int)m.AccountId,
                    FullName = $"{m.Account?.AccountProfile?.FirstName} {m.Account?.AccountProfile?.LastName}",
                    MemberTitle = m.MemberTitle,
                    CanAdministerChannel = (bool)m.CanAdministerChannel,
                    AvatarUrl = m.Account.AccountProfile.AvatarUrl
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching members in chatRoomId = {chatRoomId}");
                throw new ApplicationException("Có lỗi khi lấy thành viên phòng chat.");
            }

        }

        // lấy ra các chatroom mà account thuộc về
        public async Task<PagedResult<ChatRoomDTO>> GetChatRoomsByAccountIdAsync(int accountId, int pageNumber, int pageSize)
        {
            try
            {
                var query = _repo.GetChatRoomsByAccountId(accountId);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(cr => new ChatRoomDTO
                    {
                        ChatRoomId = cr.ChatRoomId,
                        RoomName = cr.RoomName
                    })
                    .ToListAsync();

                return new PagedResult<ChatRoomDTO>(items, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting chat rooms for accountId = {accountId}");
                throw new ApplicationException("Lỗi khi lấy danh sách phòng chat.");
            }
        }

        // gửi tin nhắn
        public async Task<ChatMessageDTO> SendMessageAsync(SendMessageRequest request)
        {
            try
            {
                var message = new ChatMessage();

                if(request.TypeMessage == Utils.Constants.MessageTypeConst.FILE) 
                {
                    var content = await _filebaseHandler.UploadMediaFile(request.File);

                    message = new ChatMessage
                    {
                        ChatRoomId = request.ChatRoomId,
                        AccountId = request.AccountId,
                        MessageContent = content,
                        SentAt = DateTime.Now,
                        IsDeleted = false,
                        TypeMessage = request.TypeMessage,
                    };
                }
                else
                {
                    message = new ChatMessage
                    {
                        ChatRoomId = request.ChatRoomId,
                        AccountId = request.AccountId,
                        MessageContent = request.MessageContent,
                        SentAt = DateTime.Now,
                        IsDeleted = false,
                        TypeMessage = request.TypeMessage,
                    };
                }

                int messageId = await _repo.AddMessageAsync(message);

                return await GetMessageByIdAsync(messageId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                throw new ApplicationException("Không thể gửi tin nhắn lúc này.");
            }
        }

        // lấy ra các message trong 1 chatroom
        public async Task<PagedResult<ChatMessageDTO>> GetMessagesByRoomIdAsync(int chatRoomId, int pageNumber, int pageSize)
        {
            try
            {
                var query = _repo.GetMessagesByRoomId(chatRoomId);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(m => m.SentAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new ChatMessageDTO
                    {
                        AccountId = (int)m.AccountId,
                        MemberTitle = m.Account.ChatRoomMembers
                            .FirstOrDefault(cm => cm.ChatRoomId == chatRoomId).MemberTitle,
                        MessageContent = m.MessageContent,
                        SentAt = (DateTime)m.SentAt,
                        IsDeleted = (bool)m.IsDeleted,
                        DeletedAt = m.DeletedAt,
                        AvatarUrl = m.Account.AccountProfile.AvatarUrl,
                        MessageId = m.ChatMessageId,
                        ChatRoomId = chatRoomId,
                        MessageType = m.TypeMessage
                    })
                    .ToListAsync();

                return new PagedResult<ChatMessageDTO>(items, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged chat messages");
                throw new ApplicationException("Không thể lấy tin nhắn.");
            }
        }

        // hàm xóa dấu câu
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

        // search tin nhắn trong một room chat
        public async Task<PagedResult<ChatMessageDTO>> SearchMessageAsync(int chatRoomId, int pageNumber, int pageSize, string searchKey)
        {
            try
            {
                string key = RemoveDiacritics(searchKey).ToLower();

                // Lấy toàn bộ tin nhắn trong phòng
                var allMessages = await _repo.GetMessagesByRoomId(chatRoomId).ToListAsync();

                // Lọc bằng RemoveDiacritics trong C#
                var filtered = allMessages
                    .Where(m => !string.IsNullOrEmpty(m.MessageContent) &&
                                RemoveDiacritics(m.MessageContent).ToLower().Contains(key))
                    .OrderByDescending(m => m.SentAt)
                    .ToList();

                var totalCount = filtered.Count;

                var pagedItems = filtered
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new ChatMessageDTO
                    {
                        AccountId = (int)m.AccountId,
                        MemberTitle = m.Account.ChatRoomMembers
                            .FirstOrDefault(cm => cm.ChatRoomId == chatRoomId)?.MemberTitle,
                        MessageContent = m.MessageContent,
                        SentAt = (DateTime)m.SentAt,
                        IsDeleted = (bool)m.IsDeleted,
                        DeletedAt = m.DeletedAt,
                        AvatarUrl = m.Account.AccountProfile.AvatarUrl,
                        MessageId = m.ChatMessageId,
                        ChatRoomId = chatRoomId,
                        MessageType = m.TypeMessage
                    })
                    .ToList();

                return new PagedResult<ChatMessageDTO>(pagedItems, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching messages in chat room");
                throw new ApplicationException("Không thể tìm kiếm tin nhắn.");
            }
        }

        public async Task<List<StartupStage>> GetAllStagesAsync()
        {
            return await _repo.GetAllAsync();
        }
    

        //lấy thông tin của message thông qua messageId
        public async Task<ChatMessageDTO?> GetMessageByIdAsync(int messageId)
        {
            var entity = await _repo.GetMessageWithDetailsByIdAsync(messageId);
            if (entity == null) return null;

            // lấy MemberTitle từ ChatRoomMembers (không include được từ ChatMessage)
            var member = await _context.ChatRoomMembers
                .FirstOrDefaultAsync(m => m.ChatRoomId == entity.ChatRoomId && m.AccountId == entity.AccountId);

            var dto = new ChatMessageDTO
            {
                MessageId = entity.ChatMessageId,
                AccountId = (int)entity.AccountId,
                ChatRoomId = (int)entity.ChatRoomId,
                MessageContent = entity.MessageContent,
                SentAt = (DateTime)entity.SentAt,
                IsDeleted = (bool)entity.IsDeleted,
                DeletedAt = entity.DeletedAt,
                AvatarUrl = entity.Account?.AccountProfile?.AvatarUrl ?? "",
                MemberTitle = member?.MemberTitle ?? "",
                MessageType = entity.TypeMessage
            };

            return dto;
        }
        public async Task<List<Account>> SearchByEmailAsync(string keyword)
        {
            return await _repo.SearchByEmailAsync(keyword);
        }

        // hàm cập nhật lại membertitle theo accountid và chatroomid
        public async Task<bool> UpdateMemberTitleAsync(UpdateMemberTitleRequest request)
        {
            var member = await _repo.GetChatRoomMemberAsync(request.ChatRoom_ID, request.Account_ID);
            if (member == null) return false;

            member.MemberTitle = request.MemberTitle;
            await _repo.UpdateMemberTitleAsync(member);
            return true;
        }
        public async Task<ResInviteDto> CreateInviteAsync(CreateInviteDTO dto)
        {
            var invite = new Invite
            {
                ReceiverAccountId = dto.Account_ID,
                StartupId = dto.Startup_ID,
                RoleId = dto.Role_ID,
                SenderAccountId = dto.InviteBy,
                InviteSentAt = DateTime.UtcNow,
                InviteStatus = InviteStatus.PENDING,
            };
            var result = await _repo.AddInviteAsync(invite);

            // Send notification to invited user
            var targetUrl = $"/invite/{invite.InviteId}";
            var AccountInvite = await _accountRepository.GetAccountByAccountIDAsync((int)dto.InviteBy);
            var AccountReceive = await _accountRepository.GetAccountByAccountIDAsync((int)dto.Account_ID);
            // Gửi thông báo tới người được mời          
            if (AccountInvite != null)
            {
                var message = $"{AccountInvite.AccountProfile?.FirstName} invite you to startup.";
                await _notificationService.CreateAndSendAsync(new reqNotificationDTO
                {
                    UserId = (int)dto.Account_ID,
                    Message = message,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    senderid = dto.InviteBy,
                    NotificationType = NotiConst.Invite,
                    TargetURL = targetUrl
                });
            }
            // Map sang DTO trước khi trả về
            var resultDto = new ResInviteDto
            {
                InviteId = invite.InviteId,
                Receiveravatar = AccountReceive.AccountProfile.AvatarUrl,
                SenderAvatar = AccountInvite.AccountProfile.AvatarUrl,
                StartupId = invite.StartupId,
                RoleId = invite.RoleId,
                InviteSentAt = invite.InviteSentAt,
                InviteStatus = invite.InviteStatus
            };

            return resultDto;
        }
        public async Task<int?> GetStartupIdByAccountIdAsync(int accountId)
        {
            return await _repo.GetStartupIdByAccountIdAsync(accountId);
        }
        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
        {
            // 1. Tạo Role
            var role = new RoleInStartup
            {
                StartupId = dto.Startup_ID,
                RoleName = dto.RoleName
            };

            var createdRole = await _repo.CreateRoleAsync(role);

            // 2. Tạo Permission tương ứng, mặc định tất cả quyền là false
            var permission = new PermissionInStartup
            {
                RoleId = createdRole.RoleId, 
                CanManagePost = false,
                CanManageCandidate = false,
                CanManageChatRoom = false,
                CanManageMember = false,
                CanManageMilestone = false
            };

            await _repo.CreatePermissionAsync(permission); // bạn cần có hàm này trong Repository

            return new RoleDto
            {
                RoleId = createdRole.RoleId,
                RoleName = createdRole.RoleName,
                StartupId = (int)createdRole.StartupId
            };
        }


        public async Task<RoleInStartup?> GetRoleAsync(int roleId)
        {
            return await _repo.GetRoleAsync(roleId);
        }

        public async Task<RoleInStartup> UpdateRoleAsync(UpdateRoleDto dto)
        {
            var role = await _repo.GetRoleAsync(dto.Role_ID);
            if (role == null) throw new Exception("Role not found");
            role.RoleName = dto.RoleName;
            return await _repo.UpdateRoleAsync(role);
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            return await _repo.DeleteRoleAsync(roleId);
        }

        public async Task<List<RoleInStartup>> GetRolesByStartupAsync(int startupId)
        {
            return await _repo.GetRolesByStartupAsync(startupId);
        }
        public async Task<List<StartupMemberDTO>> SearchAndFilterMembersAsync(int startupId, int? roleId, string? search)
        {
            var query = _repo.SearchAndFilterMembers(startupId, roleId, search);

            var result = await query.Select(m => new StartupMemberDTO
            {
                AccountId = (int)m.AccountId,
                FullName = m.Account.AccountProfile.FirstName + m.Account.AccountProfile.LastName,
                RoleName = m.Role.RoleName,
                AvatarUrl = m.Account.AccountProfile.AvatarUrl,
                JoinAT = (DateTime)m.JoinedAt,
                Email = m.Account.Email
            }).ToListAsync();

            return result;
        }
        // Kick member (admin dùng)
        public async Task<bool> KickMemberAsync(int startupId, int accountId)
        {
            var member = await _repo.GetMemberAsync(startupId, accountId);
            if (member == null) return false;
            return await _repo.RemoveMemberAsync(member);
        }

        // Out startup (thành viên tự rời)
        public async Task<bool> OutStartupAsync(int accountId)
        {
            var member = await _repo.GetMemberByAccountAsync(accountId);
            if (member == null) return false;
            return await _repo.RemoveMemberAsync(member);
        }
        public async Task<bool> UpdateMemberRoleAsync(int startupId, int accountId, int newRoleId)
        {
            return await _repo.UpdateMemberRoleAsync(startupId, accountId, newRoleId);
        }

        // kick member
        public async Task<bool> KickMembersAsync(KickChatRoomMembersRequestDTO dto)
        {
            var isAdmin = await _repo.IsAdminChatRoomAsync(dto.ChatRoomId, dto.RequesterAccountId);
            if (!isAdmin) return false;

            var membersToKick = await _repo.GetChatRoomMembersAsync(dto.ChatRoomId, dto.TargetAccountIds);
            if (membersToKick.Count == 0) return false;

            _repo.DeleteChatRoomMembersRange(membersToKick);
            await _repo.SaveChangesAsync();
            return true;
        }
        public async Task<PagedResult<ResInviteDto>> GetInvitesByStartupIdPagedAsync(int startupId, int pageNumber, int pageSize)
        {
            var (invites, totalCount) = await _repo.GetInvitesByStartupIdPagedAsync(startupId, pageNumber, pageSize);

            var dtos = new List<ResInviteDto>();
            foreach (var i in invites)
            {
                // Lấy avatar của sender
                string? senderAvatar = null;
                string? reciverAvatar = null;
                if (i.SenderAccountId.HasValue)
                {
                    var senderAccount = await _accountRepository.GetAccountByAccountIDAsync(i.SenderAccountId.Value);
                    senderAvatar = senderAccount?.AccountProfile?.AvatarUrl;
                    var reciverAccount = await _accountRepository.GetAccountByAccountIDAsync(i.ReceiverAccountId.Value);
                    reciverAvatar = reciverAccount?.AccountProfile?.AvatarUrl;
                }

                dtos.Add(new ResInviteDto
                {
                    InviteId = i.InviteId,
                    SenderAvatar = senderAvatar,
                    SenderEmail = i.SenderAccount?.Email, // Có thể null nếu không include
                    Receiveravatar = reciverAvatar,
                    ReceiverEmail = i.ReceiverAccount?.Email, // Có thể null nếu không include
                    RoleId = i.RoleId,
                    RoleName = i.Role?.RoleName,
                    StartupId = i.StartupId,
                    InviteSentAt = i.InviteSentAt,
                    InviteStatus = i.InviteStatus
                });
            }

            return new PagedResult<ResInviteDto>(dtos, totalCount, pageNumber, pageSize);
        }

        public async Task<bool> CanInviteAgainAsync(int accountId, int startupId)
        {        
            // Đang có lời mời chờ xử lý
            bool hasPendingInvite = await _repo.ExistsPendingInviteAsync(accountId, startupId);
            if (hasPendingInvite) return false;
            return true;
        }
        public async Task<ResInviteDto?> GetInviteByIdAsync(int inviteId)
        {
            var invite = await _repo.GetInviteByIdAsync(inviteId);
            if (invite == null) return null;
            var AccountInvite = await _accountRepository.GetAccountByAccountIDAsync((int)invite.SenderAccountId);
            var AccountReceive = await _accountRepository.GetAccountByAccountIDAsync((int)invite.ReceiverAccountId);
            return new ResInviteDto
            {
                InviteId = invite.InviteId,
                SenderAvatar = AccountInvite.AccountProfile.AvatarUrl,
                SenderEmail = invite.SenderAccount?.Email,
                Receiveravatar = AccountReceive.AccountProfile.AvatarUrl,
                ReceiverEmail = invite.ReceiverAccount?.Email,
                RoleId = invite.RoleId,
                RoleName = invite.Role?.RoleName,
                StartupId = invite.StartupId,
                InviteSentAt = invite.InviteSentAt,
                InviteStatus = invite.InviteStatus
            };
        }
        public async Task<bool> UpdateInviteAsync(int inviteId, string response) // response: "accept" hoặc "reject"
        {
            var invite = await _repo.GetInviteByIdAsync(inviteId);
            if (invite == null || invite.InviteStatus != InviteStatus.PENDING)
                return false;

            if (response == "Approved")
            {
                invite.InviteStatus = InviteStatus.ACCEPTED;
             
                    var member = new StartupMember
                    {
                        AccountId = invite.ReceiverAccountId.Value,
                        StartupId = invite.StartupId.Value,
                        RoleId = invite.RoleId.Value,
                        JoinedAt = DateTime.Now
                    };
                    await _repo.AddMemberAsync(member);
                
            }
            else if (response == "Rejected")
            {
                invite.InviteStatus = InviteStatus.REJECTED;
            }
            else
            {
                return false;
            }

            await _repo.UpdateInviteAsync(invite);
            return true;
        }

        //lấy position requirment theo id
        public async Task<PositionRequirementResponseDto?> GetPositionRequirementByIdAsync(int id)
        {
            var entity = await _repo.GetPositionRequirementByIdAsync(id);
            if (entity == null)
                return null;

            return MapToResponseDto(entity);
        }

        //lấy ra position requirment theo startupId
        public async Task<PagedResult<PositionRequirementResponseDto>> GetPositionRequirementPagedAsync(int startupId, int pageNumber, int pageSize)
        {
            var totalCount = await _repo.GetTotalPositionRequirementCountAsync(startupId);
            var items = await _repo.GetPositionRequirementPagedAsync(startupId, pageNumber, pageSize);

            var dtoItems = items.Select(MapToResponseDto).ToList();

            return new PagedResult<PositionRequirementResponseDto>(dtoItems, totalCount, pageNumber, pageSize);
        }

        //thêm position requirment
        public async Task<PositionRequirementResponseDto> AddPositionRequirementAsync(PositionRequirementCreateDto dto)
        {
            var entity = new PositionRequirement
            {
                StartupId = dto.StartupId,
                Title = dto.Title,
                Description = dto.Description,
                Requirement = dto.Requirement
            };

            await _repo.AddPositionRequirementAsync(entity);

            return MapToResponseDto(entity);
        }

        //update position requirment
        public async Task<bool> UpdatePositionRequirementAsync(int id, PositionRequirementUpdateDto dto)
        {
            var entity = await _repo.GetPositionRequirementByIdAsync(id);
            if (entity == null)
                return false;

            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.Requirement = dto.Requirement;

            await _repo.UpdatePositionRequirementAsync(entity);
            return true;
        }

        // xóa position requirment
        public async Task<bool> DeletePositionRequirementAsync(int id)
        {
            var entity = await _repo.GetPositionRequirementByIdAsync(id);
            if (entity == null)
                return false;

            await _repo.DeletePositionRequirementAsync(entity);
            return true;
        }
        //mapping chay
        private PositionRequirementResponseDto MapToResponseDto(PositionRequirement entity)
        {
            return new PositionRequirementResponseDto
            {
                PositionId = entity.PositionId,
                StartupId = entity.StartupId,
                Title = entity.Title,
                Description = entity.Description,
                Requirement = entity.Requirement
            };
        }
        public async Task<bool> SubscribeAsync(int accountId, int startupId)
        {
            if (await _repo.IsSubscribedAsync(accountId, startupId))
                return false;

            var sub = new Subcribe
            {
                FollowerAccountId = accountId,
                FollowingStartUpId = startupId,
                FollowDate = DateTime.UtcNow
            };
            await _repo.AddSubcribeAsync(sub);
            return true;
        }

        public async Task<bool> UnsubscribeAsync(int accountId, int startupId)
        {
            var sub = await _repo.GetSubcribeAsync(accountId, startupId);
            if (sub == null)
                return false;

            await _repo.RemoveSubcribeAsync(sub);
            return true;
        }

        //lấy ra thông tin của startup
        public async Task<StartupDetailDTO?> GetStartupByIdAsync(int startupId)
        {
            var startup = await _postRepo.GetStartupByIdAsync(startupId);
            if (startup == null)
                return null;

            return new StartupDetailDTO
            {
                StartupId = startup.StartupId,
                StartupName = startup.StartupName,
                AbbreviationName = startup.AbbreviationName,
                Description = startup.Description,
                Vision = startup.Vision,
                Mission = startup.Mission,
                Logo = startup.Logo,
                BackgroundURL = startup.BackgroundUrl,
                WebsiteURL = startup.WebsiteUrl,
                Email = startup.Email,
                Status = startup.Status,
                StageId = startup.StageId,
                StageName = startup.Stage.StageName,
                CreateAt = (DateTime)startup.CreateAt
            };
        }


        public async Task<bool> UpdateStartupAsync(int id, UpdateStartupDto dto)
        {
            var startup = await _repo.GetStartupByIdAsync(id);
            if (startup == null)
                return false;

            var logo = startup.Logo;
            var backgroundUrl = startup.BackgroundUrl;

            if (dto.Logo != null && dto.Logo.Length != 0)
            {
                logo = await _filebaseHandler.UploadMediaFile(dto.Logo);
            }

            if(dto.Background != null && dto.Background.Length != 0)
            {
                backgroundUrl = await _filebaseHandler.UploadMediaFile(dto.Background);
            }

            startup.StartupName = dto.StartupName;
            startup.AbbreviationName = dto.AbbreviationName;
            startup.Description = dto.Description;
            startup.Vision = dto.Vision;
            startup.Mission = dto.Mission;
            startup.Logo = logo;
            startup.BackgroundUrl = backgroundUrl;
            startup.WebsiteUrl = dto.WebsiteURL;
            startup.Email = dto.Email;
            startup.StageId = startup.StageId;

            await _repo.UpdateStartupAsync(startup);
            return true;
        }

        public async Task<PagedResult<CandidateCVResponseDTO>> GetCVsOfStartupAsync(int startupId,int positionId, int page, int pageSize)
        {
            return await _cvRepository.GetCandidateCVsByStartupIdAsync(startupId, positionId, page, pageSize);
        }

        public async Task<PositionRequirementDto?> GetRequirementInfoAsync(int positionId)
        {
            var entity = await _repo.GetByIdAsync(positionId);
            if (entity == null)
                return null;

            return new PositionRequirementDto
            {
                Description = entity.Description,
                Requirement = entity.Requirement
            };
        }

        public async Task AddEvaluationAsync(CVRequirementEvaluationResultDto evaluation)
        {
            await _repo.AddCVRequirementEvaluationAsync(
                new CvrequirementEvaluation
                {
                    CandidateCvId = (int)evaluation.CandidateCVID,
                    InternshipId = (int)evaluation.InternshipId,
                    EvaluationTechSkills = evaluation.Evaluation_TechSkills,
                    EvaluationExperience = evaluation.Evaluation_Experience,
                    EvaluationSoftSkills = evaluation.Evaluation_SoftSkills,
                    EvaluationOverallSummary = evaluation.Evaluation_OverallSummary,

                });
            await _repo.SaveChangesAsync();
        }

        public async Task<StartupPitching> AddStartupPitchingAsync(StartupPitchingCreateDTO dto)
        {
            string type;
            string link;

            if (dto.File.ContentType.StartsWith("video/"))
            {
                type = Utils.Constants.StartupPitchingConst.Video;
                link = await _filebaseHandler.UploadMediaFile(dto.File);
            }
            else if (dto.File.ContentType == "application/pdf")
            {
                type = Utils.Constants.StartupPitchingConst.PDF;
                link = await _filebaseHandler.UploadPdfToBackblazeAsync(dto.File);
            }
            else
            {
                throw new ArgumentException("Unsupported file type. Only video and PDF are allowed.");
            }

            var entity = new StartupPitching
            {
                StartupId = dto.StartupId,
                Type = type,
                Link = link
            };

            await _repo.AddStartupPitchingAsync(entity);
            await _repo.SaveChangesAsync();

            return entity;
        }

        public async Task<List<GetStartupPitchingDTO>> GetPitchingsByTypeAndStartupAsync(int startupId, string type)
        {
            var pitchings = await _repo.GetPitchingsByTypeAndStartupAsync(startupId, type);

            var result = pitchings.Select(p =>
            {
                string link = p.Type switch
                {
                    Utils.Constants.StartupPitchingConst.PDF => _filebaseHandler.GeneratePresignedBackblazePDFUrl(p.Link),
                    Utils.Constants.StartupPitchingConst.Video => _filebaseHandler.GeneratePreSignedUrl(p.Link),
                    _ => p.Link
                };

                return new GetStartupPitchingDTO
                {
                    PitchingId = p.PitchingId,
                    Type = p.Type,
                    Link = link,
                    CreateAt = p.CreateAt ?? DateTime.MinValue // fallback nếu null
                };
            }).ToList();

            return result;
        }

        public async Task<bool> DeleteStartupPitchingAsync(int pitchingId)
        {
            var pitching = await _repo.GetStartupPitchingByIdAsync(pitchingId);
            if (pitching == null)
                return false;

            if (pitching.Type == Utils.Constants.StartupPitchingConst.Video)
            {
                _filebaseHandler.DeleteFileByUrlAsync(pitching.Link);
            }
            else
            {
                _filebaseHandler.DeleteFileOnBackblazeAsync(pitching.Link);
            }

            _repo.DeleteStartupPitching(pitching);
            await _repo.SaveChangesAsync();
            return true;
        }


        public async Task<bool> UpdateStartupPitchingAsync(int startupPitchingId, IFormFile file)
        {
            var pitching = await _repo.GetStartupPitchingByIdAsync(startupPitchingId);
            if (pitching == null) return false;

            string type = "";
            string link = "";


            if(pitching.Type == Utils.Constants.StartupPitchingConst.Video)
            {
                _filebaseHandler.DeleteFileByUrlAsync(pitching.Link);
            }
            else
            {
                _filebaseHandler.DeleteFileOnBackblazeAsync(pitching.Link);
            }

            if (file.ContentType.StartsWith("video/"))
            {
                type = Utils.Constants.StartupPitchingConst.Video;
                link = await _filebaseHandler.UploadMediaFile(file);
            }
            else if (file.ContentType == "application/pdf")
            {
                type = Utils.Constants.StartupPitchingConst.PDF;
                link = await _filebaseHandler.UploadPdfToBackblazeAsync(file);
                
            }
            else
            {
                throw new ArgumentException("File không hợp lệ. Chỉ chấp nhận PDF hoặc video.");
            }

            pitching.Type = type;
            pitching.Link = link;
            pitching.CreateAt = DateTime.UtcNow;

            _repo.UpdateStartupPitching(pitching);
            await _repo.SaveChangesAsync();

            return true;
        }
      

        public async Task<bool> IsAccountSubcibeStartup(int accountId, int startupId)
        {
            return await _repo.IsAccountSubcibeStartup(accountId, startupId);
        }
    }
}