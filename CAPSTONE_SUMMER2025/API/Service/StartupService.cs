using API.DTO.AccountDTO;
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
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly IFilebaseHandler _filebaseHandler;
        private readonly ILogger<StartupService> _logger;
        private readonly INotificationService _notificationService;
        private readonly CAPSTONE_SUMMER2025Context _context;

        public StartupService(IStartupRepository repo, IMapper mapper, IFilebaseHandler filebaseHandler, ILogger<StartupService> logger, IAccountRepository accountRepository, CAPSTONE_SUMMER2025Context conntext, INotificationService notificationService)
        {
            _repo = repo;
            _mapper = mapper;
            _filebaseHandler = filebaseHandler;
            _logger = logger;
            _accountRepository = accountRepository;
            _context = conntext;
            _notificationService = notificationService;
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
        public async Task<List<ResStartupDTO>> GetAllStartupsAsync()
        {
            var startups = await _repo.GetAllStartupsAsync();
            var result = startups.Select(s => new ResStartupDTO
            {
                Startup_ID = s.StartupId,
                Startup_Name = s.StartupName,
                Description = s.Description,
                // Nếu Logo không rỗng, tạo PreSignedUrl
                Logo = !string.IsNullOrEmpty(s.Logo) ? _filebaseHandler.GeneratePreSignedUrl(
                    s.Logo.Contains("/") ? s.Logo : $"image/{s.Logo}"
                ) : null,
                BackgroundUrl = !string.IsNullOrEmpty(s.BackgroundUrl) ? _filebaseHandler.GeneratePreSignedUrl(
                     s.BackgroundUrl.Contains("/") ? s.BackgroundUrl : $"image/{s.BackgroundUrl}"
                ) : null,

                WebsiteURL = s.WebsiteUrl,
                Email = s.Email,
                Status = s.Status,
                Categories = s.StartupCategories?
                    .Select(sc => sc.Category.CategoryName).ToList() ?? new List<string>()
            }).ToList();

            return result;
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
                FullName = $"{sm.Account?.AccountProfile?.FirstName} {sm.Account?.AccountProfile?.LastName}",
                RoleName = sm.Role?.RoleName,
                AvatarUrl = sm.Account.AccountProfile.AvatarUrl,
                JoinAT = (DateTime)sm.JoinedAt
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
                var message = new ChatMessage
                {
                    ChatRoomId = request.ChatRoomId,
                    AccountId = request.AccountId,
                    MessageContent = request.MessageContent,
                    SentAt = DateTime.Now,
                    IsDeleted = false
                };

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
                MemberTitle = member?.MemberTitle ?? ""
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
        public async Task<Invite> CreateInviteAsync(CreateInviteDTO dto)
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
            //await _notificationService.CreateAndSendAsync(invite);
            return result;
        }
        public async Task<int?> GetStartupIdByAccountIdAsync(int accountId)
        {
            return await _repo.GetStartupIdByAccountIdAsync(accountId);
        }
    }
}