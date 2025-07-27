using API.DTO.AccountDTO;
using API.DTO.Mesage;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Service.Interface;
using Google.Cloud.AIPlatform.V1;
using Infrastructure.Models;

namespace API.Service
{
    public class UserChatService : IUserChatService
    {
        private readonly IUserChatRepository _repo;
        private readonly IFilebaseHandler _FilebaseHandler;
        private readonly IStartupRepository _startupRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IPostRepository _postRepo;
        public UserChatService(IUserChatRepository repo, IFilebaseHandler filebaseHandler, IStartupRepository startupRepository, IAccountRepository accountRepository, IPostRepository postRepository)
        {
            _repo = repo;
            _FilebaseHandler = filebaseHandler;
            _startupRepository = startupRepository;
            _accountRepository = accountRepository;
            _postRepo = postRepository;
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


        public async Task<ResUserMessageDTO> SendMessageAsync(UserMessageDto dto)
        {


            var messagedto = new ResUserMessageDTO();
            string SenderName = string.Empty;

            if (dto.SenderAccountId != null && int.TryParse(dto.SenderAccountId.ToString(), out int accountId))
            {
                var account = await _accountRepository.GetAccountByAccountIDAsync(accountId);
                SenderName = account?.AccountProfile?.FirstName + " " + account?.AccountProfile?.LastName;
            }
            else if (dto.SenderStartupId != null && int.TryParse(dto.SenderStartupId.ToString(), out int startupId))
            {
                var startup = await _postRepo.GetStartupByIdAsync(startupId);
                SenderName = startup?.StartupName;
            }
            string SenderAvatar = string.Empty;

            if (dto.SenderAccountId != null && int.TryParse(dto.SenderAccountId.ToString(), out int avaaccountId))
            {
                var account = await _accountRepository.GetAccountByAccountIDAsync(avaaccountId);
                SenderAvatar = account?.AccountProfile?.AvatarUrl ?? string.Empty;
            }
            else if (dto.SenderStartupId != null && int.TryParse(dto.SenderStartupId.ToString(), out int avastartupId))
            {
                var startup = await _postRepo.GetStartupByIdAsync(avastartupId);
                SenderAvatar = startup.Logo;
            }

            if (dto.Type == Utils.Constants.MessageTypeConst.FILE)
            {
                var content = await _FilebaseHandler.UploadMediaFile(dto.File);

                messagedto = new ResUserMessageDTO
                {
                    ChatRoomId = dto.ChatRoomId,
                    SenderName = SenderName,
                    SenderAvatar = SenderAvatar,
                    Content = content,
                    SentAt = DateTime.Now,
                    Type= dto.Type,
                    IsRead = false,
                };


            }
            else
            {
                messagedto = new ResUserMessageDTO
                {
                    ChatRoomId = dto.ChatRoomId,
                    SenderName = SenderName,
                    SenderAvatar = SenderAvatar,
                    Content = dto.Content,
                    SentAt = DateTime.Now,
                    Type = dto.Type,
                    IsRead = false,
                };
            }

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
            return messagedto;
        }
        public async Task<List<ChatRoomWithLatestMessageDto>> GetChatRoomsByAccountAsync(int accountId)
        {
            return await _repo.GetChatRoomsByAccountIdAsync(accountId);
        }
    }
}
