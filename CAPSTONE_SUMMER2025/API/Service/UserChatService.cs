using API.DTO.Mesage;
using API.Repositories.Interfaces;
using API.Service.Interface;
using Infrastructure.Models;

namespace API.Service
{
    public class UserChatService : IUserChatService
    {
        private readonly IUserChatRepository _repo;

        public UserChatService(IUserChatRepository repo)
        {
            _repo = repo;
        }

        public async Task<int> EnsureChatRoomAsync(int accountId, int? targetAccountId, int? targetStartupId)
        {
            var room = await _repo.GetChatRoomAsync(accountId, targetAccountId, targetStartupId);
            if (room != null) return room.ChatRoomId;

            var newRoom = await _repo.CreateChatRoomAsync(accountId, targetAccountId, targetStartupId);
            return newRoom.ChatRoomId;
        }

        public async Task<List<UserMessage>> GetMessagesAsync(int chatRoomId)
        {
            return await _repo.GetMessagesAsync(chatRoomId);
        }

        public async Task SendMessageAsync(UserMessageDto dto)
        {
            var msg = new UserMessage
            {
                ChatRoomId = dto.ChatRoomId,
                SenderAccountId = dto.SenderAccountId,
                SenderStartupId = dto.SenderStartupId,
                Content = dto.Content,
                FileUrl = dto.FileUrl,
                FileType = dto.FileType,
                SentAt = DateTime.Now,
                IsRead = false
            };

            await _repo.SendMessageAsync(msg);
        }
    }
}
