using API.DTO.AccountDTO;
using API.DTO.Mesage;
using API.DTO.VideoCall;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Service.Interface;
using Google.Cloud.AIPlatform.V1;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace API.Service
{
    public class UserChatService : IUserChatService
    {
        private readonly IUserChatRepository _repo;
        private readonly IFilebaseHandler _FilebaseHandler;
        private readonly IStartupRepository _startupRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly CAPSTONE_SUMMER2025Context _context;

        private readonly IPostRepository _postRepo;
        public UserChatService(IUserChatRepository repo, IFilebaseHandler filebaseHandler, IStartupRepository startupRepository, IAccountRepository accountRepository, IPostRepository postRepository, CAPSTONE_SUMMER2025Context context)
        {
            _repo = repo;
            _FilebaseHandler = filebaseHandler;
            _startupRepository = startupRepository;
            _accountRepository = accountRepository;
            _postRepo = postRepository;
            _context = context;
        }

        public async Task<int> EnsureChatRoomAsync(int accountId, int? targetAccountId, int? targetStartupId)
        {
            var room = await _repo.GetChatRoomAsync(accountId, targetAccountId, targetStartupId);
            if (room != null) return room.ChatRoomId;

            var newRoom = await _repo.CreateChatRoomAsync(accountId, targetAccountId, targetStartupId);
            return newRoom.ChatRoomId;
        }

        public async Task<PagedResult<GetUserMessageDTO>> GetMessagesAsync(int chatRoomId, int pageNumber, int pageSize)
        {
            var totalCount = await _repo.GetTotalMessagesAsync(chatRoomId);
            var resultRaw = await _repo.GetMessagesAsync(chatRoomId, pageNumber, pageSize);

            var result = new List<GetUserMessageDTO>();

            foreach (var message in resultRaw) {
                var dto = new GetUserMessageDTO();

                if (message.SenderAccountId != null)
                {
                    var account = await _accountRepository.GetAccountByAccountIDAsync((int)message.SenderAccountId);
                    dto = new GetUserMessageDTO
                    {
                        ChatRoomId = chatRoomId,
                        Content = message.FileType == Utils.Constants.MessageTypeConst.FILE
                    ? _FilebaseHandler.GeneratePreSignedUrl(message.Content)
                    : message.Content,
                        IsRead = message.IsRead,
                        SenderAccountId = message.SenderAccountId,
                        SenderStartupId = message.SenderStartupId,
                        MessageId = message.MessageId,
                        SentAt = message.SentAt,
                        Type = message.FileType,
                        Name = account.AccountProfile.FirstName + " " + account.AccountProfile.LastName,
                        AvatarUrl = account.AccountProfile.AvatarUrl
                    };
                    result.Add(dto);
                }
                else if(message.SenderStartupId != null)
                {
                    var startup = await _startupRepository.GetStartupByIdAsync((int)message.SenderStartupId);
                    dto = new GetUserMessageDTO
                    {
                        ChatRoomId = chatRoomId,
                        Content = message.FileType == Utils.Constants.MessageTypeConst.FILE
                    ? _FilebaseHandler.GeneratePreSignedUrl(message.Content)
                    : message.Content,
                        IsRead = message.IsRead,
                        SenderAccountId = message.SenderAccountId,
                        SenderStartupId = message.SenderStartupId,
                        MessageId = message.MessageId,
                        SentAt = message.SentAt,
                        Type = message.FileType,
                        Name = startup.StartupName,
                        AvatarUrl = startup.Logo
                    };
                    result.Add(dto);
                }
            }

    

            return new PagedResult<GetUserMessageDTO>(result, totalCount, pageNumber, pageSize);
        }


        public async Task<UserMessage> SendMessageAsync(UserMessageDto dto)
        {


           
            var message = new UserMessage();

            if (dto.Type == Utils.Constants.MessageTypeConst.FILE)
            {
                var content = await _FilebaseHandler.UploadMediaFile(dto.File);

                message = new UserMessage
                {
                    ChatRoomId = dto.ChatRoomId,
                    SenderAccountId = dto.SenderAccountId,
                    Content = content,
                    SentAt = DateTime.Now,
                    FileType = dto.Type,
                    IsRead = false,
                    SenderStartupId = dto.SenderStartupId,
                };

            }
            else
            {
                message = new UserMessage
                {
                    ChatRoomId = dto.ChatRoomId,
                    SenderAccountId = dto.SenderAccountId,
                    Content = dto.Content,
                    SentAt = DateTime.Now,
                    FileType = dto.Type,
                    IsRead = false,
                    SenderStartupId = dto.SenderStartupId,
                };
            }
            await _repo.SendMessageAsync(message);
            return message;
        }
        public async Task<List<ChatRoomWithLatestMessageDto>> GetChatRoomsByAccountAsync(int accountId)
        {
            return await _repo.GetChatRoomsByAccountIdAsync(accountId);
        }
        public async Task<List<ChatRoomWithLatestMessageDto>> GetChatRoomsByStartupAsync(int startupId)
        {
            return await _repo.GetChatRoomsByStartupIdAsync(startupId);
        }
        public async Task<ResStartCallDto> StartCallAsync(StartCallDto dto)
        {
            var members = await _context.UserChatRoomMembers
                .Where(x => x.ChatRoomId == dto.ChatRoomId)
                .Select(x => x.AccountId)
                .ToListAsync();

            var calleeId = members.FirstOrDefault(x => x != dto.AccountId);
            if (calleeId == 0)
                throw new Exception("Không tìm thấy người nhận trong phòng.");

            var users = await _context.AccountProfiles
                .Where(x => x.AccountId == dto.AccountId || x.AccountId == calleeId)
                .Select(x => new { x.AccountId, x.FirstName, x.LastName, x.AvatarUrl })
                .ToListAsync();

            var caller = users.First(x => x.AccountId == dto.AccountId);
            var callee = users.First(x => x.AccountId == calleeId);

            var call = new UserCallSession
            {
                CallSessionId = Guid.NewGuid(),
                ChatRoomId = dto.ChatRoomId,
                StartedByAccountId = dto.AccountId,
                StartedAt = DateTime.Now,
                Status = "Pending",
                RoomToken = Guid.NewGuid().ToString()
            };

            await _repo.CreateAsync(call);

            return new ResStartCallDto
            {
                CallSessionId = call.CallSessionId,
                RoomToken = call.RoomToken,
                StartedAt = call.StartedAt,
                ChatRoomId = call.ChatRoomId,
                Status = call.Status,
                Caller = new UserShortDto
                {
                    AccountId = (int)caller.AccountId,
                    FullName = caller.FirstName+" "+caller.LastName,
                    AvatarUrl = caller.AvatarUrl
                },
                Callee = new UserShortDto
                {
                    AccountId = (int)callee.AccountId,
                    FullName = callee.FirstName + " " + callee.LastName,
                    AvatarUrl = callee.AvatarUrl
                }
            };
        }

        public async Task EndCallAsync(Guid callSessionId)
        {
            var call = await _repo.GetByIdAsync(callSessionId);
            if (call == null) throw new Exception("Cuộc gọi không tồn tại.");

            call.Status = "Ended";
            call.EndedAt = DateTime.Now;
            await _repo.SaveChangesAsync();
        }

        public async Task<List<UserCallSession>> GetCallHistoryAsync(int chatRoomId)
        {
            return await _repo.GetByChatRoomIdAsync(chatRoomId);
        }
        public async Task AcceptCallAsync(Guid callSessionId)
        {
            await _repo.UpdateStatusAsync(callSessionId, "Accepted");
        }

        public async Task RejectCallAsync(Guid callSessionId)
        {
            await _repo.UpdateStatusAsync(callSessionId, "Rejected");
        }

    }
}
