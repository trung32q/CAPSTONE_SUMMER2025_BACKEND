using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTO.StartupDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using AutoMapper;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.IO;
using API.Utils.Constants;

namespace UnitTest.Repositories
{
    public class StartupRepositoryTest
    {
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly IStartupRepository _startupRepository;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IFilebaseHandler> _filebaseHandlerMock;

        public StartupRepositoryTest()
        {
            _context = TestHelper.CreateInMemoryDbContext();
            _mapperMock = new Mock<IMapper>();
            _mapperMock.Setup(m => m.Map<Startup>(It.IsAny<Startup>())).Returns<Startup>(s => s);
            _mapperMock.Setup(m => m.Map<RoleInStartup>(It.IsAny<RoleInStartup>())).Returns<RoleInStartup>(r => r);
            _mapperMock.Setup(m => m.Map<ChatRoom>(It.IsAny<ChatRoom>())).Returns<ChatRoom>(c => c);
            _mapperMock.Setup(m => m.Map<ChatRoomMember>(It.IsAny<ChatRoomMember>())).Returns<ChatRoomMember>(m => m);
            _mapperMock.Setup(m => m.Map<Invite>(It.IsAny<Invite>())).Returns<Invite>(i => i);
            _filebaseHandlerMock = new Mock<IFilebaseHandler>();
            _filebaseHandlerMock.Setup(f => f.UploadMediaFile(It.IsAny<IFormFile>())).ReturnsAsync("http://mocked-url.com/media");
            _filebaseHandlerMock.Setup(f => f.DeleteFileByUrlAsync(It.IsAny<string>())).ReturnsAsync(true);
            _filebaseHandlerMock.Setup(f => f.GeneratePreSignedUrl(It.IsAny<string>())).Returns<string>(url => url);
            _startupRepository = new StartupRepository(_mapperMock.Object, _context, _filebaseHandlerMock.Object);
        }

        #region AddStartupAsync
        [Fact]
        public async Task AddStartupAsync_ValidStartup_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup
            {
                StartupId = 1,
                StartupName = "Test Startup",
                Description = "Test Description",
                Logo = "logo.jpg",
                Status = "Active",
                CreateAt = DateTime.UtcNow
            };

            // Act
            var result = await _startupRepository.AddStartupAsync(startup);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Startup", result.StartupName);
            var savedStartup = await _context.Startups.FindAsync(1);
            Assert.NotNull(savedStartup);
            Assert.Equal("Test Description", savedStartup.Description);
            Assert.Equal("logo.jpg", savedStartup.Logo);
        }
        #endregion

        #region AddMemberAsync
        [Fact]
        public async Task AddMemberAsync_ValidMember_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var role = new RoleInStartup { RoleId = 1, RoleName = "Member", StartupId = 1 };
            _context.Startups.Add(startup);
            _context.Accounts.Add(account);
            _context.RoleInStartups.Add(role);
            await _context.SaveChangesAsync();
            var member = new StartupMember
            {
                StartupId = 1,
                AccountId = 1,
                RoleId = 1,
                JoinedAt = DateTime.UtcNow
            };

            // Act
            var result = await _startupRepository.AddMemberAsync(member);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.StartupId);
            Assert.Equal(1, result.AccountId);
            var savedMember = await _context.StartupMembers.FirstOrDefaultAsync(m => m.StartupId == 1 && m.AccountId == 1);
            Assert.NotNull(savedMember);
            Assert.Equal(1, savedMember.RoleId);
        }
        #endregion

        #region AddRoleAsync
        [Fact]
        public async Task AddRoleAsync_ValidRole_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();
            var role = new RoleInStartup
            {
                RoleId = 1,
                RoleName = "Admin",
                StartupId = 1
            };

            // Act
            var result = await _startupRepository.AddRoleAsync(role);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Admin", result.RoleName);
            var savedRole = await _context.RoleInStartups.FindAsync(1);
            Assert.NotNull(savedRole);
            Assert.Equal(1, savedRole.StartupId);
        }
        #endregion

        #region AddStartupCategoryAsync
        [Fact]
        public async Task AddStartupCategoryAsync_ValidCategory_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var category = new Category { CategoryId = 1, CategoryName = "Tech" };
            _context.Startups.Add(startup);
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            var startupCategory = new StartupCategory
            {
                StartupId = 1,
                CategoryId = 1
            };

            // Act
            await _startupRepository.AddStartupCategoryAsync(startupCategory);

            // Assert
            var savedCategory = await _context.StartupCategories.FirstOrDefaultAsync(sc => sc.StartupId == 1 && sc.CategoryId == 1);
            Assert.NotNull(savedCategory);
        }
        #endregion

        #region GetAllStartupsAsync
        [Fact]
        public async Task GetAllStartupsAsync_ValidPage_ReturnsPagedStartupsOrderedByFollowCount()
        {
            // Arrange
            var startups = new List<Startup>
            {
                new Startup { StartupId = 1, StartupName = "Startup 1", CreateAt = DateTime.UtcNow },
                new Startup { StartupId = 2, StartupName = "Startup 2", CreateAt = DateTime.UtcNow.AddDays(-1) }
            };
            var subcribes = new List<Subcribe>
            {
                new Subcribe { SubcribeId = 1, FollowerAccountId = 1, FollowingStartUpId = 1 },
                new Subcribe { SubcribeId = 2, FollowerAccountId = 2, FollowingStartUpId = 1 },
                new Subcribe { SubcribeId = 3, FollowerAccountId = 3, FollowingStartUpId = 2 }
            };
            _context.Startups.AddRange(startups);
            _context.Subcribes.AddRange(subcribes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetAllStartupsAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal("Startup 1", result.Items.First().StartupName); 
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(1, result.PageSize);
        }

        [Fact]
        public async Task GetAllStartupsAsync_WithCategoryId_ReturnsFilteredStartups()
        {
            // Arrange
            var startups = new List<Startup>
            {
                new Startup { StartupId = 1, StartupName = "Startup 1", CreateAt = DateTime.UtcNow },
                new Startup { StartupId = 2, StartupName = "Startup 2", CreateAt = DateTime.UtcNow.AddDays(-1) }
            };
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, CategoryName = "Tech" }
            };
            var startupCategories = new List<StartupCategory>
            {
                new StartupCategory { StartupId = 1, CategoryId = 1 }
            };
            var subcribes = new List<Subcribe>
            {
                new Subcribe { SubcribeId = 1, FollowerAccountId = 1, FollowingStartUpId = 1 }
            };
            _context.Startups.AddRange(startups);
            _context.Categories.AddRange(categories);
            _context.StartupCategories.AddRange(startupCategories);
            _context.Subcribes.AddRange(subcribes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetAllStartupsAsync(1, 10, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal("Startup 1", result.Items.First().StartupName);
            Assert.NotNull(result.Items.First().StartupCategories);
            Assert.Contains(result.Items.First().StartupCategories, sc => sc.CategoryId == 1);
        }

        [Fact]
        public async Task GetAllStartupsAsync_NonExistingCategoryId_ReturnsEmptyList()
        {
            // Arrange
            var startups = new List<Startup>
            {
                new Startup { StartupId = 1, StartupName = "Startup 1", CreateAt = DateTime.UtcNow }
            };
            _context.Startups.AddRange(startups);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetAllStartupsAsync(1, 10, 999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }
        #endregion

        #region CreateChatRoomAsync
        [Fact]
        public async Task CreateChatRoomAsync_ValidRoom_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();
            var chatRoom = new ChatRoom
            {
                ChatRoomId = 1,
                RoomName = "Test Room",
                StartupId = 1
            };

            // Act
            var result = await _startupRepository.CreateChatRoomAsync(chatRoom);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Room", result.RoomName);
            var savedRoom = await _context.ChatRooms.FindAsync(1);
            Assert.NotNull(savedRoom);
            Assert.Equal(1, savedRoom.StartupId);
        }
        #endregion

        #region AddMemberAsync (ChatRoomMember)
        [Fact]
        public async Task AddMemberAsync_ValidChatRoomMember_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var chatRoom = new ChatRoom { ChatRoomId = 1, RoomName = "Test Room", StartupId = 1 };
            _context.Startups.Add(startup);
            _context.Accounts.Add(account);
            _context.ChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();
            var member = new ChatRoomMember
            {
                ChatRoomId = 1,
                AccountId = 1,
                CanAdministerChannel = true,
                JoinedAt = DateTime.UtcNow
            };

            // Act
            await _startupRepository.AddMemberAsync(member);

            // Assert
            var savedMember = await _context.ChatRoomMembers.FirstOrDefaultAsync(m => m.ChatRoomId == 1 && m.AccountId == 1);
            Assert.NotNull(savedMember);
            Assert.True(savedMember.CanAdministerChannel);
        }
        #endregion

        #region AddMembersAsync
        [Fact]
        public async Task AddMembersAsync_MultipleMembers_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var chatRoom = new ChatRoom { ChatRoomId = 1, RoomName = "Test Room", StartupId = 1 };
            var accounts = new List<Account>
            {
                new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } },
                new Account { AccountId = 2, AccountProfile = new AccountProfile { FirstName = "Jane", LastName = "Doe" } }
            };
            _context.Startups.Add(startup);
            _context.ChatRooms.Add(chatRoom);
            _context.Accounts.AddRange(accounts);
            await _context.SaveChangesAsync();
            var members = new List<ChatRoomMember>
            {
                new ChatRoomMember { ChatRoomId = 1, AccountId = 1, CanAdministerChannel = false },
                new ChatRoomMember { ChatRoomId = 1, AccountId = 2, CanAdministerChannel = true }
            };

            // Act
            await _startupRepository.AddMembersAsync(members);

            // Assert
            var savedMembers = await _context.ChatRoomMembers.Where(m => m.ChatRoomId == 1).ToListAsync();
            Assert.Equal(2, savedMembers.Count);
            Assert.Contains(savedMembers, m => m.AccountId == 1 && !m.CanAdministerChannel.GetValueOrDefault(false));
            Assert.Contains(savedMembers, m => m.AccountId == 2 && m.CanAdministerChannel.GetValueOrDefault(false));
        }
        #endregion

        #region GetChatRoomByIdAsync
        [Fact]
        public async Task GetChatRoomByIdAsync_ValidId_ReturnsChatRoom()
        {
            // Arrange
            var chatRoom = new ChatRoom { ChatRoomId = 1, RoomName = "Test Room", StartupId = 1 };
            _context.ChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetChatRoomByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Room", result.RoomName);
        }

        [Fact]
        public async Task GetChatRoomByIdAsync_NonExistingId_ReturnsNull()
        {
            // Act
            var result = await _startupRepository.GetChatRoomByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region IsStartupMemberAsync
        [Fact]
        public async Task IsStartupMemberAsync_ValidMember_ReturnsTrue()
        {
            // Arrange
            var startupMember = new StartupMember { StartupId = 1, AccountId = 1, RoleId = 1 };
            _context.StartupMembers.Add(startupMember);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.IsStartupMemberAsync(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsStartupMemberAsync_NonExistingMember_ReturnsFalse()
        {
            // Act
            var result = await _startupRepository.IsStartupMemberAsync(999, 999);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region IsChatRoomAdminAsync
        [Fact]
        public async Task IsChatRoomAdminAsync_ValidAdmin_ReturnsTrue()
        {
            // Arrange
            var member = new ChatRoomMember
            {
                ChatRoomId = 1,
                AccountId = 1,
                CanAdministerChannel = true
            };
            _context.ChatRoomMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.IsChatRoomAdminAsync(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsChatRoomAdminAsync_NonAdmin_ReturnsFalse()
        {
            // Arrange
            var member = new ChatRoomMember
            {
                ChatRoomId = 1,
                AccountId = 1,
                CanAdministerChannel = false 
            };
            _context.ChatRoomMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.IsChatRoomAdminAsync(1, 1);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region IsAccountSubcibeStartup
        [Fact]
        public async Task IsAccountSubcibeStartup_AccountSubscribed_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var subcribe = new Subcribe { SubcribeId = 1, FollowerAccountId = 1, FollowingStartUpId = 1 };
            _context.Accounts.Add(account);
            _context.Startups.Add(startup);
            _context.Subcribes.Add(subcribe);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.IsAccountSubcibeStartup(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsAccountSubcibeStartup_AccountNotSubscribed_ReturnsFalse()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            _context.Accounts.Add(account);
            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.IsAccountSubcibeStartup(1, 1);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region GetExistingChatRoomMemberIdsAsync
        [Fact]
        public async Task GetExistingChatRoomMemberIdsAsync_ValidChatRoom_ReturnsMemberIds()
        {
            // Arrange
            var members = new List<ChatRoomMember>
            {
                new ChatRoomMember { ChatRoomId = 1, AccountId = 1 },
                new ChatRoomMember { ChatRoomId = 1, AccountId = 2 }
            };
            _context.ChatRoomMembers.AddRange(members);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetExistingChatRoomMemberIdsAsync(1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(1, result);
            Assert.Contains(2, result);
        }
        #endregion

        #region GetByStartupIdAsync
        [Fact]
        public async Task GetByStartupIdAsync_ValidStartupId_ReturnsMembers()
        {
            // Arrange
            var startup = new Startup { StartupId = 1 };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var role = new RoleInStartup { RoleId = 1, RoleName = "Member" };
            var member = new StartupMember { StartupId = 1, AccountId = 1, RoleId = 1 };
            _context.Startups.Add(startup);
            _context.Accounts.Add(account);
            _context.RoleInStartups.Add(role);
            _context.StartupMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetByStartupIdAsync(1);

            // Assert
            Assert.Single(result);
            Assert.Equal("John", result.First().Account.AccountProfile.FirstName);
            Assert.Equal("Member", result.First().Role.RoleName);
        }
        #endregion

        #region GetMembersByChatRoomIdAsync
        [Fact]
        public async Task GetMembersByChatRoomIdAsync_ValidChatRoomId_ReturnsMembers()
        {
            // Arrange
            var chatRoom = new ChatRoom { ChatRoomId = 1 };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var member = new ChatRoomMember { ChatRoomId = 1, AccountId = 1 };
            _context.ChatRooms.Add(chatRoom);
            _context.Accounts.Add(account);
            _context.ChatRoomMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetMembersByChatRoomIdAsync(1);

            // Assert
            Assert.Single(result);
            Assert.Equal("John", result.First().Account.AccountProfile.FirstName);
        }
        #endregion

        #region GetChatRoomsByAccountId
        [Fact]
        public async Task GetChatRoomsByAccountId_ValidAccountId_ReturnsChatRooms()
        {
            // Arrange
            var chatRoom = new ChatRoom { ChatRoomId = 1, RoomName = "Test Room" };
            var member = new ChatRoomMember { ChatRoomId = 1, AccountId = 1 };
            _context.ChatRooms.Add(chatRoom);
            _context.ChatRoomMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetChatRoomsByAccountId(1).ToListAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Room", result.First().RoomName);
        }
        #endregion

        #region AddMessageAsync
        [Fact]
        public async Task AddMessageAsync_ValidMessage_AddsSuccessfully()
        {
            // Arrange
            var chatRoom = new ChatRoom { ChatRoomId = 1 };
            var account = new Account { AccountId = 1 };
            _context.ChatRooms.Add(chatRoom);
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            var message = new ChatMessage
            {
                ChatRoomId = 1,
                AccountId = 1,
                MessageContent = "Hello",
                SentAt = DateTime.UtcNow
            };

            // Act
            var result = await _startupRepository.AddMessageAsync(message);

            // Assert
            Assert.True(result > 0);
            var savedMessage = await _context.ChatMessages.FirstOrDefaultAsync(m => m.ChatRoomId == 1);
            Assert.NotNull(savedMessage);
            Assert.Equal("Hello", savedMessage.MessageContent);
        }
        #endregion

        #region GetMessagesByRoomId
        [Fact]
        public async Task GetMessagesByRoomId_ValidRoomId_ReturnsMessages()
        {
            // Arrange
            var chatRoom = new ChatRoom { ChatRoomId = 1 };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John" } };
            var message = new ChatMessage { ChatRoomId = 1, AccountId = 1, MessageContent = "Hello", SentAt = DateTime.UtcNow };
            _context.ChatRooms.Add(chatRoom);
            _context.Accounts.Add(account);
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetMessagesByRoomId(1).ToListAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Hello", result.First().MessageContent);
            Assert.Equal("John", result.First().Account.AccountProfile.FirstName);
        }
        #endregion

        #region IsMemberOfAnyStartup
        [Fact]
        public async Task IsMemberOfAnyStartup_ValidMember_ReturnsTrue()
        {
            // Arrange
            var member = new StartupMember { StartupId = 1, AccountId = 1 };
            _context.StartupMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.IsMemberOfAnyStartup(1);

            // Assert
            Assert.True(result);
        }
        #endregion

        #region GetAllAsync (StartupStage)
        [Fact]
        public async Task GetAllAsync_ReturnsAllStages()
        {
            // Arrange
            var stages = new List<StartupStage>
            {
                new StartupStage { StageId = 1, StageName = "Seed" },
                new StartupStage { StageId = 2, StageName = "Growth" }
            };
            _context.StartupStages.AddRange(stages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, s => s.StageName == "Seed");
        }
        #endregion

        #region GetMessageWithDetailsByIdAsync
        [Fact]
        public async Task GetMessageWithDetailsByIdAsync_ValidId_ReturnsMessage()
        {
            // Arrange
            var message = new ChatMessage
            {
                ChatMessageId = 1,
                ChatRoomId = 1,
                AccountId = 1,
                MessageContent = "Hello",
                Account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John" } }
            };
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetMessageWithDetailsByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Hello", result.MessageContent);
            Assert.Equal("John", result.Account.AccountProfile.FirstName);
        }

        [Fact]
        public async Task GetMessageWithDetailsByIdAsync_NonExistingId_ReturnsNull()
        {
            // Act
            var result = await _startupRepository.GetMessageWithDetailsByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region SearchByEmailAsync
        [Fact]
        public async Task SearchByEmailAsync_ValidKeyword_ReturnsAccounts()
        {
            // Arrange
            var accounts = new List<Account>
            {
                new Account { AccountId = 1, Email = "john@example.com", AccountProfile = new AccountProfile { FirstName = "John" } },
                new Account { AccountId = 2, Email = "jane@example.com", AccountProfile = new AccountProfile { FirstName = "Jane" } }
            };
            _context.Accounts.AddRange(accounts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.SearchByEmailAsync("john");

            // Assert
            Assert.Single(result);
            Assert.Equal("john@example.com", result.First().Email);
        }
        #endregion

        #region GetChatRoomMemberAsync
        [Fact]
        public async Task GetChatRoomMemberAsync_ValidIds_ReturnsMember()
        {
            // Arrange
            var member = new ChatRoomMember { ChatRoomId = 1, AccountId = 1, MemberTitle = "Admin" };
            _context.ChatRoomMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetChatRoomMemberAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Admin", result.MemberTitle);
        }

        [Fact]
        public async Task GetChatRoomMemberAsync_NonExistingIds_ReturnsNull()
        {
            // Act
            var result = await _startupRepository.GetChatRoomMemberAsync(999, 999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region UpdateMemberTitleAsync
        [Fact]
        public async Task UpdateMemberTitleAsync_ValidMember_UpdatesSuccessfully()
        {
            // Arrange
            var member = new ChatRoomMember { ChatGroupMembersId = 1, ChatRoomId = 1, AccountId = 1, MemberTitle = "Old Title" };
            _context.ChatRoomMembers.Add(member);
            await _context.SaveChangesAsync();
            member.MemberTitle = "New Title";

            // Act
            await _startupRepository.UpdateMemberTitleAsync(member);

            // Assert
            var updatedMember = await _context.ChatRoomMembers.FindAsync(1);
            Assert.NotNull(updatedMember);
            Assert.Equal("New Title", updatedMember.MemberTitle);
        }
        #endregion

        #region AddInviteAsync
        [Fact]
        public async Task AddInviteAsync_ValidInvite_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1 };
            var account = new Account { AccountId = 1 };
            var role = new RoleInStartup { RoleId = 1 };
            _context.Startups.Add(startup);
            _context.Accounts.Add(account);
            _context.RoleInStartups.Add(role);
            await _context.SaveChangesAsync();
            var invite = new Invite
            {
                SenderAccountId = 1,
                ReceiverAccountId = 2,
                StartupId = 1,
                RoleId = 1,
                InviteStatus = InviteStatus.PENDING
            };

            // Act
            var result = await _startupRepository.AddInviteAsync(invite);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(InviteStatus.PENDING, result.InviteStatus);
            var savedInvite = await _context.Invites.FirstOrDefaultAsync(i => i.StartupId == 1);
            Assert.NotNull(savedInvite);
        }
        #endregion

        #region GetStartupIdByAccountIdAsync
        [Fact]
        public async Task GetStartupIdByAccountIdAsync_ValidAccountId_ReturnsStartupId()
        {
            // Arrange
            var member = new StartupMember { StartupId = 1, AccountId = 1 };
            _context.StartupMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetStartupIdByAccountIdAsync(1);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task GetStartupIdByAccountIdAsync_NonExistingAccountId_ReturnsNull()
        {
            // Act
            var result = await _startupRepository.GetStartupIdByAccountIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region CreateRoleAsync
        [Fact]
        public async Task CreateRoleAsync_ValidRole_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1 };
            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();
            var role = new RoleInStartup { RoleName = "Founder", StartupId = 1 };

            // Act
            var result = await _startupRepository.CreateRoleAsync(role);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Founder", result.RoleName);
            var savedRole = await _context.RoleInStartups.FirstOrDefaultAsync(r => r.StartupId == 1);
            Assert.NotNull(savedRole);
        }
        #endregion

        #region GetRoleAsync
        [Fact]
        public async Task GetRoleAsync_ValidRoleId_ReturnsRole()
        {
            // Arrange
            var role = new RoleInStartup { RoleId = 1, RoleName = "Founder", StartupId = 1 };
            _context.RoleInStartups.Add(role);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetRoleAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Founder", result.RoleName);
        }

        [Fact]
        public async Task GetRoleAsync_NonExistingRoleId_ReturnsNull()
        {
            // Act
            var result = await _startupRepository.GetRoleAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region UpdateRoleAsync
        [Fact]
        public async Task UpdateRoleAsync_ValidRole_UpdatesSuccessfully()
        {
            // Arrange
            var role = new RoleInStartup { RoleId = 1, RoleName = "Old Role", StartupId = 1 };
            _context.RoleInStartups.Add(role);
            await _context.SaveChangesAsync();

            // Detach the original role to avoid tracking conflict
            _context.Entry(role).State = EntityState.Detached;

            var updatedRole = new RoleInStartup { RoleId = 1, RoleName = "New Role", StartupId = 1 };

            // Act
            var result = await _startupRepository.UpdateRoleAsync(updatedRole);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Role", result.RoleName);
            var savedRole = await _context.RoleInStartups.FindAsync(1);
            Assert.NotNull(savedRole);
            Assert.Equal("New Role", savedRole.RoleName);
        }
        #endregion

        #region DeleteRoleAsync
        [Fact]
        public async Task DeleteRoleAsync_ValidRoleId_DeletesSuccessfully()
        {
            // Arrange
            var role = new RoleInStartup { RoleId = 1, RoleName = "Founder", StartupId = 1 };
            _context.RoleInStartups.Add(role);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.DeleteRoleAsync(1);

            // Assert
            Assert.True(result);
            Assert.Empty(await _context.RoleInStartups.ToListAsync());
        }

        [Fact]
        public async Task DeleteRoleAsync_NonExistingRoleId_ReturnsFalse()
        {
            // Act
            var result = await _startupRepository.DeleteRoleAsync(999);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region GetRolesByStartupAsync
        [Fact]
        public async Task GetRolesByStartupAsync_ValidStartupId_ReturnsRoles()
        {
            // Arrange
            var roles = new List<RoleInStartup>
            {
                new RoleInStartup { RoleId = 1, RoleName = "Founder", StartupId = 1 },
                new RoleInStartup { RoleId = 2, RoleName = "Member", StartupId = 1 }
            };
            _context.RoleInStartups.AddRange(roles);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetRolesByStartupAsync(1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.RoleName == "Founder");
            Assert.Contains(result, r => r.RoleName == "Member");
        }
        #endregion

        #region SearchAndFilterMembers
        [Fact]
        public async Task SearchAndFilterMembers_ValidSearch_ReturnsFilteredMembers()
        {
            // Arrange
            var startup = new Startup { StartupId = 1 };
            var accounts = new List<Account>
            {
                new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } },
                new Account { AccountId = 2, AccountProfile = new AccountProfile { FirstName = "Jane", LastName = "Smith" } }
            };
            var members = new List<StartupMember>
            {
                new StartupMember { StartupId = 1, AccountId = 1, RoleId = 1 },
                new StartupMember { StartupId = 1, AccountId = 2, RoleId = 2 }
            };
            _context.Startups.Add(startup);
            _context.Accounts.AddRange(accounts);
            _context.StartupMembers.AddRange(members);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.SearchAndFilterMembers(1, null, "John").ToListAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("John", result.First().Account.AccountProfile.FirstName);
        }
        #endregion

        #region GetMemberAsync
        [Fact]
        public async Task GetMemberAsync_ValidIds_ReturnsMember()
        {
            // Arrange
            var member = new StartupMember { StartupId = 1, AccountId = 1, RoleId = 1 };
            _context.StartupMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetMemberAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.AccountId);
        }

        [Fact]
        public async Task GetMemberAsync_NonExistingIds_ReturnsNull()
        {
            // Act
            var result = await _startupRepository.GetMemberAsync(999, 999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetMemberByAccountAsync
        [Fact]
        public async Task GetMemberByAccountAsync_ValidAccountId_ReturnsMember()
        {
            // Arrange
            var member = new StartupMember { StartupId = 1, AccountId = 1, RoleId = 1 };
            _context.StartupMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetMemberByAccountAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.AccountId);
        }
        #endregion

        #region RemoveMemberAsync
        [Fact]
        public async Task RemoveMemberAsync_ValidMember_RemovesSuccessfully()
        {
            // Arrange
            var member = new StartupMember { StartupMemberId = 1, StartupId = 1, AccountId = 1 };
            _context.StartupMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.RemoveMemberAsync(member);

            // Assert
            Assert.True(result);
            Assert.Empty(await _context.StartupMembers.ToListAsync());
        }
        #endregion

        #region UpdateMemberRoleAsync
        [Fact]
        public async Task UpdateMemberRoleAsync_ValidMember_UpdatesSuccessfully()
        {
            // Arrange
            var member = new StartupMember { StartupId = 1, AccountId = 1, RoleId = 1 };
            _context.StartupMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.UpdateMemberRoleAsync(1, 1, 2);

            // Assert
            Assert.True(result);
            var updatedMember = await _context.StartupMembers.FirstOrDefaultAsync(m => m.StartupId == 1 && m.AccountId == 1);
            Assert.Equal(2, updatedMember.RoleId);
        }

        [Fact]
        public async Task UpdateMemberRoleAsync_NonExistingMember_ReturnsFalse()
        {
            // Act
            var result = await _startupRepository.UpdateMemberRoleAsync(999, 999, 1);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region IsAdminChatRoomAsync
        [Fact]
        public async Task IsAdminChatRoomAsync_ValidAdmin_ReturnsTrue()
        {
            // Arrange
            var member = new ChatRoomMember { ChatRoomId = 1, AccountId = 1, CanAdministerChannel = true };
            _context.ChatRoomMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.IsAdminChatRoomAsync(1, 1);

            // Assert
            Assert.True(result);
        }
        #endregion

        #region GetChatRoomMembersAsync
        [Fact]
        public async Task GetChatRoomMembersAsync_ValidIds_ReturnsMembers()
        {
            // Arrange
            var members = new List<ChatRoomMember>
            {
                new ChatRoomMember { ChatRoomId = 1, AccountId = 1 },
                new ChatRoomMember { ChatRoomId = 1, AccountId = 2 }
            };
            _context.ChatRoomMembers.AddRange(members);
            await _context.SaveChangesAsync();
            var accountIds = new List<int> { 1, 2 };

            // Act
            var result = await _startupRepository.GetChatRoomMembersAsync(1, accountIds);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, m => m.AccountId == 1);
            Assert.Contains(result, m => m.AccountId == 2);
        }
        #endregion

        #region DeleteChatRoomMembersRange
        [Fact]
        public async Task DeleteChatRoomMembersRange_ValidMembers_DeletesSuccessfully()
        {
            // Arrange
            var members = new List<ChatRoomMember>
            {
                new ChatRoomMember { ChatGroupMembersId = 1, ChatRoomId = 1, AccountId = 1 },
                new ChatRoomMember { ChatGroupMembersId = 2, ChatRoomId = 1, AccountId = 2 }
            };
            _context.ChatRoomMembers.AddRange(members);
            await _context.SaveChangesAsync();

            // Act
            _startupRepository.DeleteChatRoomMembersRange(members);
            await _context.SaveChangesAsync();

            // Assert
            Assert.Empty(await _context.ChatRoomMembers.ToListAsync());
        }
        #endregion

        #region AddPermissionAsync
        [Fact]
        public async Task AddPermissionAsync_ValidPermission_AddsSuccessfully()
        {
            // Arrange
            var permission = new PermissionInStartup
            {
                RoleId = 1,
                CanManagePost = true,
                CanManageCandidate = false
            };

            // Act
            await _startupRepository.AddPermissionAsync(permission);

            // Assert
            var savedPermission = await _context.PermissionInStartups.FirstOrDefaultAsync(p => p.RoleId == 1);
            Assert.NotNull(savedPermission);
            Assert.True(savedPermission.CanManagePost);
            Assert.False(savedPermission.CanManageCandidate);
        }
        #endregion

        #region GetInvitesByStartupIdPagedAsync
        [Fact]
        public async Task GetInvitesByStartupIdPagedAsync_ValidPage_ReturnsPagedInvites()
        {
            // Arrange
            var invites = new List<Invite>
            {
                new Invite { InviteId = 1, StartupId = 1, InviteSentAt = DateTime.UtcNow },
                new Invite { InviteId = 2, StartupId = 1, InviteSentAt = DateTime.UtcNow.AddDays(-1) }
            };
            _context.Invites.AddRange(invites);
            await _context.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _startupRepository.GetInvitesByStartupIdPagedAsync(1, 1, 1);

            // Assert
            Assert.Single(items);
            Assert.Equal(2, totalCount);
            Assert.Equal(1, items.First().InviteId);
        }
        #endregion

        #region ExistsPendingInviteAsync
        [Fact]
        public async Task ExistsPendingInviteAsync_ValidInvite_ReturnsTrue()
        {
            // Arrange
            var invite = new Invite { ReceiverAccountId = 1, StartupId = 1, InviteStatus = InviteStatus.PENDING };
            _context.Invites.Add(invite);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.ExistsPendingInviteAsync(1, 1);

            // Assert
            Assert.True(result);
        }
        #endregion

        #region GetInviteByIdAsync
        [Fact]
        public async Task GetInviteByIdAsync_ValidId_ReturnsInvite()
        {
            // Arrange
            var invite = new Invite
            {
                InviteId = 1,
                StartupId = 1,
                SenderAccountId = 1,
                ReceiverAccountId = 2,
                RoleId = 1,
                InviteStatus = InviteStatus.PENDING
            };
            _context.Invites.Add(invite);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetInviteByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(InviteStatus.PENDING, result.InviteStatus);
        }
        #endregion

        #region UpdateInviteAsync
        [Fact]
        public async Task UpdateInviteAsync_ValidInvite_UpdatesSuccessfully()
        {
            // Arrange
            var invite = new Invite { InviteId = 1, StartupId = 1, InviteStatus = InviteStatus.PENDING };
            _context.Invites.Add(invite);
            await _context.SaveChangesAsync();
            invite.InviteStatus = InviteStatus.ACCEPTED;

            // Act
            await _startupRepository.UpdateInviteAsync(invite);

            // Assert
            var updatedInvite = await _context.Invites.FindAsync(1);
            Assert.Equal(InviteStatus.ACCEPTED, updatedInvite.InviteStatus);
        }
        #endregion

        #region GetPositionRequirementByIdAsync
        [Fact]
        public async Task GetPositionRequirementByIdAsync_ValidId_ReturnsPosition()
        {
            // Arrange
            var position = new PositionRequirement { PositionId = 1, StartupId = 1, Title = "Developer" };
            _context.PositionRequirements.Add(position);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetPositionRequirementByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Developer", result.Title);
        }
        #endregion

        #region GetPositionRequirementPagedAsync
        [Fact]
        public async Task GetPositionRequirementPagedAsync_ValidPage_ReturnsPagedPositions()
        {
            // Arrange
            var positions = new List<PositionRequirement>
            {
                new PositionRequirement { PositionId = 1, StartupId = 1, Title = "Developer" },
                new PositionRequirement { PositionId = 2, StartupId = 1, Title = "Designer" }
            };
            _context.PositionRequirements.AddRange(positions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetPositionRequirementPagedAsync(1, 1, 1);

            // Assert
            Assert.Single(result);
            Assert.Equal("Developer", result.First().Title);
        }
        #endregion

        #region GetTotalPositionRequirementCountAsync
        [Fact]
        public async Task GetTotalPositionRequirementCountAsync_ValidStartupId_ReturnsCount()
        {
            // Arrange
            var positions = new List<PositionRequirement>
            {
                new PositionRequirement { PositionId = 1, StartupId = 1 },
                new PositionRequirement { PositionId = 2, StartupId = 1 }
            };
            _context.PositionRequirements.AddRange(positions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetTotalPositionRequirementCountAsync(1);

            // Assert
            Assert.Equal(2, result);
        }
        #endregion

        #region AddPositionRequirementAsync
        [Fact]
        public async Task AddPositionRequirementAsync_ValidPosition_AddsSuccessfully()
        {
            // Arrange
            var position = new PositionRequirement { StartupId = 1, Title = "Developer" };

            // Act
            await _startupRepository.AddPositionRequirementAsync(position);

            // Assert
            var savedPosition = await _context.PositionRequirements.FirstOrDefaultAsync(p => p.Title == "Developer");
            Assert.NotNull(savedPosition);
        }
        #endregion

        #region UpdatePositionRequirementAsync
        [Fact]
        public async Task UpdatePositionRequirementAsync_ValidPosition_UpdatesSuccessfully()
        {
            // Arrange
            var position = new PositionRequirement { PositionId = 1, StartupId = 1, Title = "Old Title" };
            _context.PositionRequirements.Add(position);
            await _context.SaveChangesAsync();
            position.Title = "New Title";

            // Act
            await _startupRepository.UpdatePositionRequirementAsync(position);

            // Assert
            var updatedPosition = await _context.PositionRequirements.FindAsync(1);
            Assert.Equal("New Title", updatedPosition.Title);
        }
        #endregion

        #region DeletePositionRequirementAsync
        [Fact]
        public async Task DeletePositionRequirementAsync_ValidPosition_DeletesSuccessfully()
        {
            // Arrange
            var position = new PositionRequirement { PositionId = 1, StartupId = 1, Title = "Developer" };
            _context.PositionRequirements.Add(position);
            await _context.SaveChangesAsync();

            // Act
            await _startupRepository.DeletePositionRequirementAsync(position);

            // Assert
            Assert.Empty(await _context.PositionRequirements.ToListAsync());
        }
        #endregion

        #region IsSubscribedAsync
        [Fact]
        public async Task IsSubscribedAsync_ValidSubscription_ReturnsTrue()
        {
            // Arrange
            var subscribe = new Subcribe { FollowerAccountId = 1, FollowingStartUpId = 1 };
            _context.Subcribes.Add(subscribe);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.IsSubscribedAsync(1, 1);

            // Assert
            Assert.True(result);
        }
        #endregion

        #region GetSubcribeAsync
        [Fact]
        public async Task GetSubcribeAsync_ValidIds_ReturnsSubcribe()
        {
            // Arrange
            var subscribe = new Subcribe { SubcribeId = 1, FollowerAccountId = 1, FollowingStartUpId = 1 };
            _context.Subcribes.Add(subscribe);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetSubcribeAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.SubcribeId);
        }
        #endregion

        #region AddSubcribeAsync
        [Fact]
        public async Task AddSubcribeAsync_ValidSubcribe_AddsSuccessfully()
        {
            // Arrange
            var subscribe = new Subcribe { FollowerAccountId = 1, FollowingStartUpId = 1 };

            // Act
            await _startupRepository.AddSubcribeAsync(subscribe);

            // Assert
            var savedSubcribe = await _context.Subcribes.FirstOrDefaultAsync(s => s.FollowerAccountId == 1);
            Assert.NotNull(savedSubcribe);
        }
        #endregion

        #region RemoveSubcribeAsync
        [Fact]
        public async Task RemoveSubcribeAsync_ValidSubcribe_RemovesSuccessfully()
        {
            // Arrange
            var subscribe = new Subcribe { SubcribeId = 1, FollowerAccountId = 1, FollowingStartUpId = 1 };
            _context.Subcribes.Add(subscribe);
            await _context.SaveChangesAsync();

            // Act
            await _startupRepository.RemoveSubcribeAsync(subscribe);

            // Assert
            Assert.Empty(await _context.Subcribes.ToListAsync());
        }
        #endregion

        #region GetStartupByIdAsync
        [Fact]
        public async Task GetStartupByIdAsync_ValidId_ReturnsStartup()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetStartupByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Startup", result.StartupName);
        }

        [Fact]
        public async Task GetStartupByIdAsync_NonExistingId_ReturnsNull()
        {
            // Act
            var result = await _startupRepository.GetStartupByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region UpdateStartupAsync
        [Fact]
        public async Task UpdateStartupAsync_ValidStartup_UpdatesSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Old Name" };
            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();
            startup.StartupName = "New Name";

            // Act
            await _startupRepository.UpdateStartupAsync(startup);

            // Assert
            var updatedStartup = await _context.Startups.FindAsync(1);
            Assert.Equal("New Name", updatedStartup.StartupName);
        }
        #endregion

        #region GetByIdAsync (PositionRequirement)
        [Fact]
        public async Task GetByIdAsync_ValidPositionId_ReturnsPosition()
        {
            // Arrange
            var position = new PositionRequirement { PositionId = 1, Title = "Developer" };
            _context.PositionRequirements.Add(position);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Developer", result.Title);
        }
        #endregion

        #region AddCVRequirementEvaluationAsync
        [Fact]
        public async Task AddCVRequirementEvaluationAsync_ValidEvaluation_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var internship = new InternshipPost
            {
                InternshipId = 1,
                StartupId = 1,
                Description = "Test Internship",
                CreateAt = DateTime.UtcNow
            };
            var candidateCv = new CandidateCv
            {
                CandidateCvId = 1,
                InternshipId = 1,
                AccountId = 1,
                Cvurl = "http://example.com/cv",
                CreateAt = DateTime.UtcNow
            };
            var account = new Account
            {
                AccountId = 1,
                AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" }
            };
            _context.Startups.Add(startup);
            _context.Accounts.Add(account);
            _context.InternshipPosts.Add(internship);
            _context.CandidateCvs.Add(candidateCv);
            await _context.SaveChangesAsync();

            var evaluation = new CvrequirementEvaluation
            {
                CandidateCvId = 1,
                InternshipId = 1,
                EvaluationTechSkills = "Good",
                EvaluationExperience = "Moderate",
                EvaluationSoftSkills = "Excellent",
                EvaluationOverallSummary = "Strong candidate",
                CandidateCv = candidateCv, 
                Internship = internship  
            };

            // Act
            try
            {
                await _startupRepository.AddCVRequirementEvaluationAsync(evaluation);
                await _context.SaveChangesAsync(); 
            }
            catch (Exception ex)
            {
                Assert.Fail($"AddCVRequirementEvaluationAsync hoặc SaveChangesAsync ném ngoại lệ: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }

            var evaluationCount = await _context.CvrequirementEvaluations.CountAsync();

            // Assert
            var savedEvaluation = await _context.CvrequirementEvaluations
                .FirstOrDefaultAsync(e => e.CandidateCvId == 1 && e.InternshipId == 1);
            Assert.NotNull(savedEvaluation); 
            Assert.Equal(1, savedEvaluation.CandidateCvId);
            Assert.Equal(1, savedEvaluation.InternshipId);
            Assert.Equal("Good", savedEvaluation.EvaluationTechSkills);
            Assert.Equal("Moderate", savedEvaluation.EvaluationExperience);
            Assert.Equal("Excellent", savedEvaluation.EvaluationSoftSkills);
            Assert.Equal("Strong candidate", savedEvaluation.EvaluationOverallSummary);
            Assert.Equal(1, evaluationCount); 
        }
        #endregion

        #region AddStartupPitchingAsync
        [Fact]
        public async Task AddStartupPitchingAsync_ValidPitching_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();

            var pitching = new StartupPitching
            {
                StartupId = 1,
                Type = "Video",
                Link = "http://example.com/video",
                CreateAt = DateTime.UtcNow,
                Startup = startup 
            };

            // Act
            try
            {
                await _startupRepository.AddStartupPitchingAsync(pitching);
                await _context.SaveChangesAsync(); 
            }
            catch (Exception ex)
            {
                Assert.Fail($"AddStartupPitchingAsync hoặc SaveChangesAsync ném ngoại lệ: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }

            var pitchingCount = await _context.StartupPitchings.CountAsync();

            // Assert
            var savedPitching = await _context.StartupPitchings
                .FirstOrDefaultAsync(p => p.StartupId == 1 && p.Type == "Video" && p.Link == "http://example.com/video");
            Assert.NotNull(savedPitching);
            Assert.Equal(1, savedPitching.StartupId);
            Assert.Equal("Video", savedPitching.Type);
            Assert.Equal("http://example.com/video", savedPitching.Link);
            Assert.Equal(pitching.CreateAt!.Value.Date, savedPitching.CreateAt!.Value.Date); 
            Assert.Equal(1, pitchingCount);
        }
        #endregion

        #region GetPitchingsByTypeAndStartupAsync
        [Fact]
        public async Task GetPitchingsByTypeAndStartupAsync_ValidType_ReturnsPitchings()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            _context.Startups.Add(startup);
            var pitchings = new List<StartupPitching>
            {
                new StartupPitching
                {
                    PitchingId = 1,
                    StartupId = 1,
                    Type = "Video",
                    Link = "http://example.com/video",
                    CreateAt = DateTime.UtcNow
                },
                new StartupPitching
                {
                    PitchingId = 2,
                    StartupId = 1,
                    Type = "Document",
                    Link = "http://example.com/document",
                    CreateAt = DateTime.UtcNow
                }
            };
            _context.StartupPitchings.AddRange(pitchings);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetPitchingsByTypeAndStartupAsync(1, "Video");

            // Assert
            Assert.Single(result);
            Assert.Equal("Video", result.First().Type);
            Assert.Equal("http://example.com/video", result.First().Link);
        }
        #endregion

        #region GetStartupPitchingByIdAsync
        [Fact]
        public async Task GetStartupPitchingByIdAsync_ValidId_ReturnsPitching()
        {
            // Arrange
            var pitching = new StartupPitching
            {
                PitchingId = 1,
                StartupId = 1,
                Type = "Video",
                Link = "http://example.com/video",
                CreateAt = DateTime.UtcNow
            };
            _context.StartupPitchings.Add(pitching);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetStartupPitchingByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Video", result.Type);
            Assert.Equal(1, result.PitchingId);
        }
        #endregion

        #region DeleteStartupPitching
        [Fact]
        public async Task DeleteStartupPitching_ValidPitching_DeletesSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            _context.Startups.Add(startup);
            var pitching = new StartupPitching
            {
                PitchingId = 1,
                StartupId = 1,
                Type = "Video",
                Link = "http://example.com/video",
                CreateAt = DateTime.UtcNow
            };
            _context.StartupPitchings.Add(pitching);
            await _context.SaveChangesAsync();

            // Act
            _startupRepository.DeleteStartupPitching(pitching);
            await _context.SaveChangesAsync();

            // Assert
            Assert.Empty(await _context.StartupPitchings.ToListAsync());
        }
        #endregion

        #region UpdateStartupPitching
        [Fact]
        public async Task UpdateStartupPitching_ValidPitching_UpdatesSuccessfully()
        {
            // Arrange
            var pitching = new StartupPitching
            {
                PitchingId = 1,
                StartupId = 1,
                Type = "Video",
                Link = "http://example.com/video",
                CreateAt = DateTime.UtcNow
            };
            _context.StartupPitchings.Add(pitching);
            await _context.SaveChangesAsync();
            pitching.Type = "Updated Type";

            // Act
            _startupRepository.UpdateStartupPitching(pitching);
            await _context.SaveChangesAsync();

            // Assert
            var updatedPitching = await _context.StartupPitchings.FindAsync(1);
            Assert.NotNull(updatedPitching);
            Assert.Equal("Updated Type", updatedPitching.Type);
        }
        #endregion

        #region CreatePermissionAsync
        [Fact]
        public async Task CreatePermissionAsync_ValidPermission_AddsSuccessfully()
        {
            // Arrange
            var permission = new PermissionInStartup
            {
                RoleId = 1,
                CanManagePost = true,
                CanManageCandidate = true
            };

            // Act
            var result = await _startupRepository.CreatePermissionAsync(permission);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CanManagePost);
            var savedPermission = await _context.PermissionInStartups.FirstOrDefaultAsync(p => p.RoleId == 1);
            Assert.NotNull(savedPermission);
        }
        #endregion

        #region GetByRoleIdAsync
        [Fact]
        public async Task GetByRoleIdAsync_ValidRoleId_ReturnsPermission()
        {
            // Arrange
            var permission = new PermissionInStartup { RoleId = 1, CanManagePost = true };
            _context.PermissionInStartups.Add(permission);
            await _context.SaveChangesAsync();

            // Act
            var result = await _startupRepository.GetByRoleIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CanManagePost);
        }

        [Fact]
        public async Task GetByRoleIdAsync_NonExistingRoleId_ReturnsNull()
        {
            // Act
            var result = await _startupRepository.GetByRoleIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        private IFormFile CreateMockFormFile(string fileName)
        {
            var content = "Mock file content";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.FileName).Returns(fileName);
            formFile.Setup(f => f.Length).Returns(stream.Length);
            formFile.Setup(f => f.OpenReadStream()).Returns(stream);
            formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            return formFile.Object;
        }
    }
}