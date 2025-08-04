using API.DTO.Mesage;
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
                .OrderByDescending(m => m.SentAt)
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
        public async Task<List<ChatRoomWithLatestMessageDto>> GetChatRoomsByAccountIdAsync(int accountId)
        {
            return await _context.UserChatRooms
                .Where(room => room.UserChatRoomMembers.Any(m => m.AccountId == accountId))
                .Select(room => new ChatRoomWithLatestMessageDto
                {
                    ChatRoomId = room.ChatRoomId,
                    Type = room.Type,
                    CreatedAt = room.CreatedAt,

                    LatestMessageContent = room.UserMessages
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => m.Content)
                        .FirstOrDefault(),

                    LatestMessageTime = room.UserMessages
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => (DateTime?)m.SentAt)
                        .FirstOrDefault(),

                    // Lấy người còn lại trong chat room
                    TargetName = room.UserChatRoomMembers
                        .Where(m => m.AccountId != accountId)  // người còn lại
                        .Select(m => m.Account != null ? m.Account.AccountProfile.FirstName+" "
                        + m.Account.AccountProfile.LastName : m.Startup.StartupName)
                        .FirstOrDefault(),

                    TargetAvatar = room.UserChatRoomMembers
                        .Where(m => m.AccountId != accountId)
                        .Select(m => m.Account != null ? m.Account.AccountProfile.AvatarUrl : m.Startup.Logo)
                        .FirstOrDefault()
                })
                .OrderByDescending(r => r.LatestMessageTime)
                .ToListAsync();
        }
        public async Task<List<ChatRoomWithLatestMessageDto>> GetChatRoomsByStartupIdAsync(int startupId)
        {
            return await _context.UserChatRooms
                .Where(room => room.UserChatRoomMembers.Any(m => m.StartupId == startupId))
                .Select(room => new ChatRoomWithLatestMessageDto
                {
                    ChatRoomId = room.ChatRoomId,
                    Type = room.Type,
                    CreatedAt = room.CreatedAt,

                    LatestMessageContent = room.UserMessages
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => m.Content)
                        .FirstOrDefault(),

                    LatestMessageTime = room.UserMessages
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => (DateTime?)m.SentAt)
                        .FirstOrDefault(),

                    TargetName = room.UserChatRoomMembers
    .Where(m => m.StartupId != startupId)
    .Select(m => m.Account != null
        ? m.Account.AccountProfile.FirstName + " " + m.Account.AccountProfile.LastName
        : m.Startup != null
            ? m.Startup.StartupName
            : "N/A")
    .FirstOrDefault(),

                    TargetAvatar = room.UserChatRoomMembers
    .Where(m => m.StartupId != startupId)
    .Select(m => m.Account != null
        ? m.Account.AccountProfile.AvatarUrl
        : m.Startup != null
            ? m.Startup.Logo
            : "N/A")
    .FirstOrDefault()

                })
                .OrderByDescending(r => r.LatestMessageTime)
                .ToListAsync();
        }
        public async Task<UserCallSession> CreateAsync(UserCallSession call)
        {
            _context.UserCallSessions.Add(call);
            await _context.SaveChangesAsync();
            return call;
        }

        public async Task<UserCallSession?> GetByIdAsync(Guid id)
        {
            return await _context.UserCallSessions.FindAsync(id);
        }

        public async Task<List<UserCallSession>> GetByChatRoomIdAsync(int chatRoomId)
        {
            return await _context.UserCallSessions
                .Where(x => x.ChatRoomId == chatRoomId)
                .OrderByDescending(x => x.StartedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task UpdateStatusAsync(Guid callSessionId, string newStatus)
        {
            var call = await _context.UserCallSessions.FindAsync(callSessionId);
            if (call != null)
            {
                call.Status = newStatus;
                await _context.SaveChangesAsync();
            }
        }

    }
}
