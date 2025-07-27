using System.Globalization;
using System.Text;
using API.DTO.AccountDTO;
using API.DTO.PostDTO;
using API.DTO.StartupDTO;
using API.Repositories.Interfaces;
using API.Utils.Constants;
using AutoMapper;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class StartupRepository : IStartupRepository
    {
        private readonly IMapper _mapper;
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly IFilebaseHandler _filebaseHandler;


        public StartupRepository(IMapper mapper, CAPSTONE_SUMMER2025Context context, IFilebaseHandler filebaseHandler)
        {
            _mapper = mapper;
            _context = context;
            _filebaseHandler = filebaseHandler;
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
        public async Task<PagedResult<Startup>> GetAllStartupsAsync(int pageNumber, int pageSize, int? categoryId = null)
        {
            var query = _context.Startups
                .Include(s => s.Stage)
                .Include(s => s.StartupCategories)
                    .ThenInclude(sc => sc.Category)
                .Include(s=>s.Subcribes)
                .Select(s => new
                {
                    Startup = s,
                    FollowCount = _context.Subcribes.Count(sub => sub.FollowingStartUpId == s.StartupId)
                });

            // Thêm điều kiện lọc theo categoryId nếu có truyền vào
            if (categoryId.HasValue)
            {
                query = query.Where(x =>
                    x.Startup.StartupCategories.Any(sc => sc.CategoryId == categoryId.Value)
                );
            }

            query = query.OrderByDescending(x => x.FollowCount);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.Startup)
                .ToListAsync();

            return new PagedResult<Startup>(items, totalCount, pageNumber, pageSize);
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

        //hàm check xem account đã follow startup chưa
        public async Task<bool> IsAccountSubcibeStartup(int accountId, int startupId)
        {
            return await _context.Subcribes
                .AnyAsync(s => s.FollowerAccountId == accountId && s.FollowingStartUpId == startupId);
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

        //lấy ra member theo chatRoomId và accountId
        public async Task<ChatRoomMember?> GetChatRoomMemberAsync(int chatRoomId, int accountId)
        {
            return await _context.ChatRoomMembers
                .FirstOrDefaultAsync(m => m.ChatRoomId == chatRoomId && m.AccountId == accountId);
        }

        //cập nhật lại memberTitle
        public async Task UpdateMemberTitleAsync(ChatRoomMember member)
        {
            _context.ChatRoomMembers.Update(member);
            await _context.SaveChangesAsync();
        }

        public async Task<Invite> AddInviteAsync(Invite invite)
        {
            _context.Invites.Add(invite);
            await _context.SaveChangesAsync();
            return invite;
        }
        public async Task<int?> GetStartupIdByAccountIdAsync(int accountId)
        {
            
            var startupId = await _context.StartupMembers
                .Where(sm => sm.AccountId == accountId)
                .Select(sm => (int?)sm.StartupId) 
                .FirstOrDefaultAsync();

            return startupId;
        }
        // Create new role
        public async Task<RoleInStartup> CreateRoleAsync(RoleInStartup role)
        {
            _context.RoleInStartups.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        // Get role by Id
        public async Task<RoleInStartup?> GetRoleAsync(int roleId)
        {
            return await _context.RoleInStartups.FindAsync(roleId);
        }

        // Update role
        public async Task<RoleInStartup> UpdateRoleAsync(RoleInStartup role)
        {
            _context.RoleInStartups.Update(role);
            await _context.SaveChangesAsync();
            return role;
        }

        // Delete role by Id
        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            var role = await _context.RoleInStartups.FindAsync(roleId);
            if (role == null) return false;
            _context.RoleInStartups.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }

        // Get all roles by StartupId
        public async Task<List<RoleInStartup>> GetRolesByStartupAsync(int startupId)
        {
            return await _context.RoleInStartups
                .Where(r => r.StartupId == startupId)
                .ToListAsync();
        }
        public IQueryable<StartupMember> SearchAndFilterMembers(int startupId, int? roleId, string? search)
        {
            var query = _context.StartupMembers
                .Include(m => m.Account)
                .Include(m => m.Role)
                .Where(m => m.StartupId == startupId);

            if (roleId.HasValue)
                query = query.Where(m => m.RoleId == roleId.Value);

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(m =>
                    m.Account.AccountProfile.FirstName.ToLower().Contains(lowerSearch) ||
                    m.Account.AccountProfile.LastName.ToLower().Contains(lowerSearch)||
                    m.Account.Email.ToLower().Contains(lowerSearch));
            }
            return query;
        }
        public async Task<StartupMember?> GetMemberAsync(int startupId, int accountId)
        {
            return await _context.StartupMembers
                .FirstOrDefaultAsync(x => x.StartupId == startupId && x.AccountId == accountId);
        }

        public async Task<StartupMember?> GetMemberByAccountAsync(int accountId)
        {
            return await _context.StartupMembers
                .FirstOrDefaultAsync(x => x.AccountId == accountId);
        }

        public async Task<bool> RemoveMemberAsync(StartupMember member)
        {
            _context.StartupMembers.Remove(member);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> UpdateMemberRoleAsync(int startupId, int accountId, int newRoleId)
        {
            var member = await _context.StartupMembers
                .FirstOrDefaultAsync(m => m.StartupId == startupId && m.AccountId == accountId);

            if (member == null) return false;

            member.RoleId = newRoleId;
            _context.StartupMembers.Update(member);
            return await _context.SaveChangesAsync() > 0;
        }

        //check quyền của của account trong chatroom
        public async Task<bool> IsAdminChatRoomAsync(int chatRoomId, int accountId)
        {
            return await _context.ChatRoomMembers.AnyAsync(m =>
                m.ChatRoomId == chatRoomId &&
                m.AccountId == accountId &&
                (bool)m.CanAdministerChannel);
        }

        //lấy ra chatroom member
        public async Task<List<ChatRoomMember>> GetChatRoomMembersAsync(int chatRoomId, List<int> accountIds)
        {
            return await _context.ChatRoomMembers
                .Where(m => m.ChatRoomId == chatRoomId && accountIds.Contains((int) m.AccountId))
                .ToListAsync();
        }

        // xóa chatroom members
        public void DeleteChatRoomMembersRange(List<ChatRoomMember> members)
        {
            _context.ChatRoomMembers.RemoveRange(members);
        }
        public async Task AddPermissionAsync(PermissionInStartup permission)
        {
            _context.PermissionInStartups.Add(permission);
            await _context.SaveChangesAsync();
        }
        public async Task<(List<Invite>, int)> GetInvitesByStartupIdPagedAsync(int startupId, int pageNumber, int pageSize)
        {
            var query = _context.Invites
                .Include(i => i.SenderAccount)
                .Include(i => i.ReceiverAccount)
                .Include(i => i.Role)
                .Where(i => i.StartupId == startupId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(i => i.InviteSentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
        public async Task<bool> ExistsPendingInviteAsync(int accountId, int startupId)
        {
            return await _context.Invites
                .AnyAsync(i => i.ReceiverAccountId == accountId
                            && i.StartupId == startupId
                            && i.InviteStatus == InviteStatus.PENDING);
        }
        public async Task<Invite?> GetInviteByIdAsync(int inviteId)
        {
            return await _context.Invites
                .Include(i => i.SenderAccount)
                .Include(i => i.ReceiverAccount)
                .Include(i => i.Role)
                .FirstOrDefaultAsync(i => i.InviteId == inviteId);
        }
        public async Task UpdateInviteAsync(Invite invite)
        {
            _context.Invites.Update(invite);
            await _context.SaveChangesAsync();
        }


        //lấy position requirment theo id
        public async Task<PositionRequirement> GetPositionRequirementByIdAsync(int id)
        {
            return await _context.PositionRequirements.FindAsync(id);
        }

        //lấy ra position requirment theo startupId
        public async Task<List<PositionRequirement>> GetPositionRequirementPagedAsync(int startupId,int pageNumber, int pageSize)
        {
            return await _context.PositionRequirements
                .Where(p => p.StartupId == startupId)
                .OrderBy(x => x.PositionId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        //lấy ra số lượng position requirment
        public async Task<int> GetTotalPositionRequirementCountAsync(int startupId)
        {
            return await _context.PositionRequirements.Where(p => p.StartupId == startupId).CountAsync();
        }

        // thêm position requirment
        public async Task AddPositionRequirementAsync(PositionRequirement entity)
        {
            _context.PositionRequirements.Add(entity);
            await _context.SaveChangesAsync();
        }

        //cập nhật position requirment
        public async Task UpdatePositionRequirementAsync(PositionRequirement entity)
        {
            _context.PositionRequirements.Update(entity);
            await _context.SaveChangesAsync();
        }

        // xóa position requirment
        public async Task DeletePositionRequirementAsync(PositionRequirement entity)
        {
            _context.PositionRequirements.Remove(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> IsSubscribedAsync(int accountId, int startupId)
        {
            return await _context.Subcribes.AnyAsync(s =>
                s.FollowerAccountId == accountId && s.FollowingStartUpId == startupId);
        }

        public async Task<Subcribe?> GetSubcribeAsync(int accountId, int startupId)
        {
            return await _context.Subcribes.FirstOrDefaultAsync(s =>
                s.FollowerAccountId == accountId && s.FollowingStartUpId == startupId);
        }

        public async Task AddSubcribeAsync(Subcribe subcribe)
        {
            _context.Subcribes.Add(subcribe);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveSubcribeAsync(Subcribe subcribe)
        {
            _context.Subcribes.Remove(subcribe);
            await _context.SaveChangesAsync();
        }


        public async Task<Startup> GetStartupByIdAsync(int id)
        {
            return await _context.Startups.FindAsync(id);
        }

        public async Task UpdateStartupAsync(Startup startup)
        {
            _context.Startups.Update(startup);
            await _context.SaveChangesAsync();
        }

        


        
        public async Task<PositionRequirement?> GetByIdAsync(int positionId)
        {
            return await _context.PositionRequirements
                .FirstOrDefaultAsync(p => p.PositionId == positionId);
        }


        public async Task AddCVRequirementEvaluationAsync(CvrequirementEvaluation evaluation)
        {
            await _context.CvrequirementEvaluations.AddAsync(evaluation);
        }


        public async Task AddStartupPitchingAsync(StartupPitching pitching)
        {
            await _context.StartupPitchings.AddAsync(pitching);
        }

        public async Task<List<StartupPitching>> GetPitchingsByTypeAndStartupAsync(int startupId, string type)
        {
            var query = _context.StartupPitchings
                .Where(p => p.StartupId == startupId);

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(p => p.Type == type);
            }

            return await query.ToListAsync();
        }

        public async Task<StartupPitching?> GetStartupPitchingByIdAsync(int pitchingId)
        {
            return await _context.StartupPitchings.FindAsync(pitchingId);
        }

        public void DeleteStartupPitching(StartupPitching pitching)
        {
            _context.StartupPitchings.Remove(pitching);
        }

        public void UpdateStartupPitching(StartupPitching pitching)
        {
            _context.StartupPitchings.Update(pitching);
        }
        public async Task<PermissionInStartup> CreatePermissionAsync(PermissionInStartup permission)
        {
            _context.PermissionInStartups.Add(permission);
            await _context.SaveChangesAsync();
            return permission;
        }
        public async Task<PermissionInStartup?> GetByRoleIdAsync(int roleId)
        {
            return await _context.PermissionInStartups
                                 .FirstOrDefaultAsync(p => p.RoleId == roleId);
        }
    }
}
