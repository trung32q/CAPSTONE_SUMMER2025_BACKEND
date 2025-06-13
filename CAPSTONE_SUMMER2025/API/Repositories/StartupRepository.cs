using API.Repositories.Interfaces;
using AutoMapper;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class StartupRepository : IStartupRepository
    {
        private readonly IMapper _mapper;
        private readonly CAPSTONE_SUMMER2025Context _context;

        public StartupRepository(IMapper mapper, CAPSTONE_SUMMER2025Context context)
        {
            _mapper = mapper;
            _context = context;
        }
        public async Task<StartupMember> AddMemberAsync(StartupMember member)
        {
            _context.StartupMembers.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<RoleInStartup> AddRoleAsync(RoleInStartup role)
        {
            _context.RoleInStartups.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Startup> AddStartupAsync(Startup startup)
        {
            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();
            return startup;
        }

        public async Task AddStartupCategoryAsync(StartupCategory startupCategory)
        {
            _context.StartupCategories.Add(startupCategory);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
        public async Task<List<Startup>> GetAllStartupsAsync()
        {
            return await _context.Startups
                .Include(s => s.StartupCategories)
                    .ThenInclude(sc => sc.Category)
                .ToListAsync();
        }

        //tạo chatroom
        public async Task<ChatRoom> CreateChatRoomAsync(ChatRoom room)
        {
            _context.ChatRooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        //thêm thành viên và chatroom
        public async Task AddMemberAsync(ChatRoomMember member)
        {
            _context.ChatRoomMembers.Add(member);
            await _context.SaveChangesAsync();
        }

        //thêm nhiều thành viên vào chatrromm
        public async Task AddMembersAsync(List<ChatRoomMember> members)
        {
            await _context.ChatRoomMembers.AddRangeAsync(members);
            await _context.SaveChangesAsync();
        }
        // hàm lấy ra chatroom theo chatRoomId
        public async Task<ChatRoom> GetChatRoomByIdAsync(int chatRoomId)
        {
            return await _context.ChatRooms.FindAsync(chatRoomId);
        }

        // hàm check xem member có thuộc startup
        public async Task<bool> IsStartupMemberAsync(int startupId, int accountId)
        {
            return await _context.StartupMembers
                .AnyAsync(sm => sm.StartupId == startupId && sm.AccountId == accountId);
        }

        //hàm check xem member có phải là addmin của chatroom
        public async Task<bool> IsChatRoomAdminAsync(int chatRoomId, int accountId)
        {
            return await _context.ChatRoomMembers
                .AnyAsync(m => m.ChatRoomId == chatRoomId && m.AccountId == accountId && (bool)m.CanAdministerChannel);
        }

        // hàm lấy ra những người thuộc startup
        public async Task<List<int>> FilterValidStartupMembersAsync(int startupId, List<int> accountIds)
        {
            return await _context.StartupMembers
                .Where(sm => sm.StartupId == startupId && accountIds.Contains((int)sm.AccountId))
                .Select(sm =>(int) sm.AccountId)
                .ToListAsync();
        }

        //hàm lấy ra các member thuộc chatroom
        public async Task<List<int>> GetExistingChatRoomMemberIdsAsync(int chatRoomId)
        {
            return await _context.ChatRoomMembers
                .Where(m => m.ChatRoomId == chatRoomId)
                .Select(m => (int)m.AccountId)
                .ToListAsync();
        }

        //lấy ra các member trong startup theo startupId
        public async Task<List<StartupMember>> GetByStartupIdAsync(int startupId)
        {
            return await _context.StartupMembers
                .Include(sm => sm.Account)
                    .ThenInclude(acc => acc.AccountProfile)
                .Include(sm => sm.Role)
                .Where(sm => sm.StartupId == startupId)
                .ToListAsync();
        }

        // hàm lấy ra các member trong 1 chatroom theo chatRoomId
        public async Task<List<ChatRoomMember>> GetMembersByChatRoomIdAsync(int chatRoomId)
        {
            return await _context.ChatRoomMembers
                .Include(m => m.Account)
                    .ThenInclude(a => a.AccountProfile)
                .Where(m => m.ChatRoomId == chatRoomId)
                .ToListAsync();
        }

        // lấy ra các chatroom mà account thuộc về
        public IQueryable<ChatRoom> GetChatRoomsByAccountId(int accountId)
        {
            return _context.ChatRoomMembers
                .Where(m => m.AccountId == accountId)
                .Select(m => m.ChatRoom)
                .Distinct()
                .AsQueryable();
        }


        // gửi tin nhắn
        public async Task<int> AddMessageAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            return message.ChatMessageId; // Trả về ID sau khi đã lưu
        }


        // lấy ra các message trong 1 chatroom
        public IQueryable<ChatMessage> GetMessagesByRoomId(int chatRoomId)
        {
            return _context.ChatMessages
                .Include(m => m.Account)
                    .ThenInclude(a => a.AccountProfile)
                .Include(m => m.Account.ChatRoomMembers)
                .Where(m => m.ChatRoomId == chatRoomId);
        }
        public async Task<bool> IsMemberOfAnyStartup(int accountId)
        {
            return await _context.StartupMembers.AnyAsync(sm => sm.AccountId == accountId);
        }
        public async Task<List<StartupStage>> GetAllAsync()
        {
            return await _context.StartupStages.ToListAsync();
        }
        //lấy ra thông tin của message thông qua messageId
        public async Task<ChatMessage?> GetMessageWithDetailsByIdAsync(int messageId)
        {
            return await _context.ChatMessages
                .Include(m => m.Account)
                    .ThenInclude(a => a.AccountProfile)
                .Include(m => m.ChatRoom) // optional nếu cần
                .FirstOrDefaultAsync(m => m.ChatMessageId == messageId);
        }
        public async Task<List<Account>> SearchByEmailAsync(string keyword)
        {
            return await _context.Accounts
                .Include(a => a.AccountProfile)
                .Where(a => a.Email.Contains(keyword))
                .Take(10)
                .ToListAsync();
        }
    }
}
