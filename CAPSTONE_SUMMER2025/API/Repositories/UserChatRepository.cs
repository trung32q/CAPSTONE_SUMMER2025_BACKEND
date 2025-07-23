using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class UserChatRepository : IUserChatRepository
    {
        private readonly CAPSTONE_SUMMER2025Context _context;

        public UserChatRepository(CAPSTONE_SUMMER2025Context context)
        {
            _context = context;
        }
        public async Task<UserChatRoom?> GetChatRoomAsync(int accountId, int? targetAccountId, int? targetStartupId)
        {
            return await _context.UserChatRooms
                .Include(c => c.UserChatRoomMembers)
                .Where(c => c.UserChatRoomMembers.Any(m => m.AccountId == accountId) &&
                            ((targetAccountId != null && c.UserChatRoomMembers.Any(m => m.AccountId == targetAccountId)) ||
                             (targetStartupId != null && c.UserChatRoomMembers.Any(m => m.StartupId == targetStartupId))))
                .FirstOrDefaultAsync();
        }

        public async Task<UserChatRoom> CreateChatRoomAsync(int accountId, int? targetAccountId, int? targetStartupId)
        {
            var chatRoom = new UserChatRoom
            {
                Type = targetStartupId != null ? "UserToStartup" : "UserToUser",
                UserChatRoomMembers = new List<UserChatRoomMember>
            {
                new UserChatRoomMember { AccountId = accountId },
                targetAccountId != null
                    ? new UserChatRoomMember { AccountId = targetAccountId }
                    : new UserChatRoomMember { StartupId = targetStartupId }
            }
            };

            _context.UserChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();
            return chatRoom;
        }

        public async Task<List<UserMessage>> GetMessagesAsync(int chatRoomId, int pageNumber, int pageSize)
        {
            return await _context.UserMessages
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderBy(m => m.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalMessagesAsync(int chatRoomId)
        {
            return await _context.UserMessages.CountAsync(m => m.ChatRoomId == chatRoomId);
        }


        public async Task SendMessageAsync(UserMessage message)
        {
            _context.UserMessages.Add(message);
            await _context.SaveChangesAsync();
        }
    }
}
