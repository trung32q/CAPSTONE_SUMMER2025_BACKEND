using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTO.Mesage;
using API.Repositories;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTest.Repositories
{
    public class UserChatRepositoryTest
    {
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly UserChatRepository _userChatRepository;

        public UserChatRepositoryTest()
        {
            _context = TestHelper.CreateInMemoryDbContext();
            _userChatRepository = new UserChatRepository(_context);
        }

        #region GetChatRoomAsync
        [Fact]
        public async Task GetChatRoomAsync_UserToUser_ValidIds_ReturnsChatRoom()
        {
            // Arrange
            int accountId = 1;
            int? targetAccountId = 2;
            int? targetStartupId = null;
            var account1 = TestHelper.CreateTestAccount(1, "user1@example.com");
            var account2 = TestHelper.CreateTestAccount(2, "user2@example.com");
            var chatRoom = new UserChatRoom
            {
                ChatRoomId = 1,
                Type = "UserToUser",
                CreatedAt = DateTime.UtcNow,
                UserChatRoomMembers = new List<UserChatRoomMember>
                {
                    new UserChatRoomMember { ChatRoomMemberId = 1, AccountId = accountId },
                    new UserChatRoomMember { ChatRoomMemberId = 2, AccountId = targetAccountId }
                }
            };
            _context.Accounts.AddRange(account1, account2);
            _context.UserChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userChatRepository.GetChatRoomAsync(accountId, targetAccountId, targetStartupId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ChatRoomId);
            Assert.Equal("UserToUser", result.Type);
            Assert.Equal(2, result.UserChatRoomMembers.Count);
        }

        [Fact]
        public async Task GetChatRoomAsync_UserToStartup_ValidIds_ReturnsChatRoom()
        {
            // Arrange
            int accountId = 1;
            int? targetAccountId = null;
            int? targetStartupId = 1;
            var account = TestHelper.CreateTestAccount(1, "user@example.com");
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup", Logo = "logo.jpg" };
            var chatRoom = new UserChatRoom
            {
                ChatRoomId = 1,
                Type = "UserToStartup",
                CreatedAt = DateTime.UtcNow,
                UserChatRoomMembers = new List<UserChatRoomMember>
                {
                    new UserChatRoomMember { ChatRoomMemberId = 1, AccountId = accountId },
                    new UserChatRoomMember { ChatRoomMemberId = 2, StartupId = targetStartupId }
                }
            };
            _context.Accounts.Add(account);
            _context.Startups.Add(startup);
            _context.UserChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userChatRepository.GetChatRoomAsync(accountId, targetAccountId, targetStartupId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ChatRoomId);
            Assert.Equal("UserToStartup", result.Type);
            Assert.Equal(2, result.UserChatRoomMembers.Count);
        }

        [Fact]
        public async Task GetChatRoomAsync_NonExistingRoom_ReturnsNull()
        {
            // Arrange
            int accountId = 1;
            int? targetAccountId = 2;
            int? targetStartupId = null;
            var account1 = TestHelper.CreateTestAccount(1, "user1@example.com");
            var account2 = TestHelper.CreateTestAccount(2, "user2@example.com");
            _context.Accounts.AddRange(account1, account2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userChatRepository.GetChatRoomAsync(accountId, targetAccountId, targetStartupId);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region CreateChatRoomAsync
        [Fact]
        public async Task CreateChatRoomAsync_UserToUser_ValidIds_CreatesRoom()
        {
            // Arrange
            int accountId = 1;
            int? targetAccountId = 2;
            int? targetStartupId = null;
            var account1 = TestHelper.CreateTestAccount(1, "user1@example.com");
            var account2 = TestHelper.CreateTestAccount(2, "user2@example.com");
            _context.Accounts.AddRange(account1, account2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userChatRepository.CreateChatRoomAsync(accountId, targetAccountId, targetStartupId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("UserToUser", result.Type);
            Assert.Equal(2, result.UserChatRoomMembers.Count);
            Assert.Contains(result.UserChatRoomMembers, m => m.AccountId == accountId);
            Assert.Contains(result.UserChatRoomMembers, m => m.AccountId == targetAccountId);
        }

        [Fact]
        public async Task CreateChatRoomAsync_UserToStartup_ValidIds_CreatesRoom()
        {
            // Arrange
            int accountId = 1;
            int? targetAccountId = null;
            int? targetStartupId = 1;
            var account = TestHelper.CreateTestAccount(1, "user@example.com");
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup", Logo = "logo.jpg" };
            _context.Accounts.Add(account);
            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userChatRepository.CreateChatRoomAsync(accountId, targetAccountId, targetStartupId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("UserToStartup", result.Type);
            Assert.Equal(2, result.UserChatRoomMembers.Count);
            Assert.Contains(result.UserChatRoomMembers, m => m.AccountId == accountId);
            Assert.Contains(result.UserChatRoomMembers, m => m.StartupId == targetStartupId);
        }
        #endregion

        #region GetMessagesAsync
        [Fact]
        public async Task GetMessagesAsync_ValidChatRoomId_ReturnsPagedMessages()
        {
            // Arrange
            int chatRoomId = 1;
            int pageNumber = 1;
            int pageSize = 2;
            var account = TestHelper.CreateTestAccount(1, "user@example.com");
            var chatRoom = new UserChatRoom
            {
                ChatRoomId = chatRoomId,
                Type = "UserToUser",
                CreatedAt = DateTime.UtcNow,
                UserChatRoomMembers = new List<UserChatRoomMember>
                {
                    new UserChatRoomMember { ChatRoomMemberId = 1, AccountId = 1 }
                }
            };
            var messages = new List<UserMessage>
            {
                new UserMessage { MessageId = 1, ChatRoomId = chatRoomId, SenderAccountId = 1, Content = "Message 1", SentAt = DateTime.UtcNow.AddMinutes(-2) },
                new UserMessage { MessageId = 2, ChatRoomId = chatRoomId, SenderAccountId = 1, Content = "Message 2", SentAt = DateTime.UtcNow.AddMinutes(-1) },
                new UserMessage { MessageId = 3, ChatRoomId = chatRoomId, SenderAccountId = 1, Content = "Message 3", SentAt = DateTime.UtcNow }
            };
            _context.Accounts.Add(account);
            _context.UserChatRooms.Add(chatRoom);
            _context.UserMessages.AddRange(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userChatRepository.GetMessagesAsync(chatRoomId, pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Message 3", result[0].Content);
            Assert.Equal("Message 2", result[1].Content);
        }

        [Fact]
        public async Task GetMessagesAsync_NonExistingChatRoomId_ReturnsEmptyList()
        {
            // Arrange
            int chatRoomId = 999;
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _userChatRepository.GetMessagesAsync(chatRoomId, pageNumber, pageSize);

            // Assert
            Assert.Empty(result);
        }
        #endregion

        #region GetTotalMessagesAsync
        [Fact]
        public async Task GetTotalMessagesAsync_ValidChatRoomId_ReturnsCount()
        {
            // Arrange
            int chatRoomId = 1;
            var account = TestHelper.CreateTestAccount(1, "user@example.com");
            var chatRoom = new UserChatRoom
            {
                ChatRoomId = chatRoomId,
                Type = "UserToUser",
                CreatedAt = DateTime.UtcNow,
                UserChatRoomMembers = new List<UserChatRoomMember>
                {
                    new UserChatRoomMember { ChatRoomMemberId = 1, AccountId = 1 }
                }
            };
            var messages = new List<UserMessage>
            {
                new UserMessage { MessageId = 1, ChatRoomId = chatRoomId, SenderAccountId = 1, Content = "Message 1" },
                new UserMessage { MessageId = 2, ChatRoomId = chatRoomId, SenderAccountId = 1, Content = "Message 2" }
            };
            _context.Accounts.Add(account);
            _context.UserChatRooms.Add(chatRoom);
            _context.UserMessages.AddRange(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userChatRepository.GetTotalMessagesAsync(chatRoomId);

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetTotalMessagesAsync_NonExistingChatRoomId_ReturnsZero()
        {
            // Arrange
            int chatRoomId = 999;

            // Act
            var result = await _userChatRepository.GetTotalMessagesAsync(chatRoomId);

            // Assert
            Assert.Equal(0, result);
        }
        #endregion

        #region SendMessageAsync
        [Fact]
        public async Task SendMessageAsync_ValidMessage_SavesSuccessfully()
        {
            // Arrange
            int chatRoomId = 1;
            var account = TestHelper.CreateTestAccount(1, "user@example.com");
            var chatRoom = new UserChatRoom
            {
                ChatRoomId = chatRoomId,
                Type = "UserToUser",
                CreatedAt = DateTime.UtcNow,
                UserChatRoomMembers = new List<UserChatRoomMember>
                {
                    new UserChatRoomMember { ChatRoomMemberId = 1, AccountId = 1 }
                }
            };
            var message = new UserMessage
            {
                ChatRoomId = chatRoomId,
                SenderAccountId = 1,
                Content = "Test Message",
                SentAt = DateTime.UtcNow
            };
            _context.Accounts.Add(account);
            _context.UserChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();

            // Act
            await _userChatRepository.SendMessageAsync(message);

            // Assert
            var savedMessage = await _context.UserMessages.FirstOrDefaultAsync(m => m.Content == "Test Message");
            Assert.NotNull(savedMessage);
            Assert.Equal(chatRoomId, savedMessage.ChatRoomId);
            Assert.Equal(1, savedMessage.SenderAccountId);
            Assert.Equal("Test Message", savedMessage.Content);
        }
        #endregion

        #region GetChatRoomsByAccountIdAsync
        [Fact]
        public async Task GetChatRoomsByAccountIdAsync_ValidAccountId_ReturnsChatRooms()
        {
            // Arrange
            int accountId = 1;
            var account1 = TestHelper.CreateTestAccount(1, "user1@example.com");
            var account2 = TestHelper.CreateTestAccount(2, "user2@example.com");
            account2.AccountProfile = new AccountProfile { AccountId = 2, FirstName = "Jane", LastName = "Smith", AvatarUrl = "avatar2.jpg" };
            var chatRoom = new UserChatRoom
            {
                ChatRoomId = 1,
                Type = "UserToUser",
                CreatedAt = DateTime.UtcNow,
                UserChatRoomMembers = new List<UserChatRoomMember>
                {
                    new UserChatRoomMember { ChatRoomMemberId = 1, AccountId = accountId, Account = account1 },
                    new UserChatRoomMember { ChatRoomMemberId = 2, AccountId = 2, Account = account2 }
                },
                UserMessages = new List<UserMessage>
                {
                    new UserMessage { MessageId = 1, ChatRoomId = 1, SenderAccountId = 1, Content = "Hello", SentAt = DateTime.UtcNow }
                }
            };
            _context.Accounts.AddRange(account1, account2);
            _context.UserChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userChatRepository.GetChatRoomsByAccountIdAsync(accountId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var chatRoomDto = result.First();
            Assert.Equal(1, chatRoomDto.ChatRoomId);
            Assert.Equal("UserToUser", chatRoomDto.Type);
            Assert.Equal("Jane Smith", chatRoomDto.TargetName);
            Assert.Equal("avatar2.jpg", chatRoomDto.TargetAvatar);
            Assert.Equal("Hello", chatRoomDto.LatestMessageContent);
        }

        [Fact]
        public async Task GetChatRoomsByAccountIdAsync_NonExistingAccountId_ReturnsEmptyList()
        {
            // Arrange
            int accountId = 999;

            // Act
            var result = await _userChatRepository.GetChatRoomsByAccountIdAsync(accountId);

            // Assert
            Assert.Empty(result);
        }
        #endregion

        #region GetChatRoomsByStartupIdAsync
        [Fact]
        public async Task GetChatRoomsByStartupIdAsync_ValidStartupId_ReturnsChatRooms()
        {
            // Arrange
            int startupId = 1;
            var account = TestHelper.CreateTestAccount(1, "user@example.com");
            var startup = new Startup { StartupId = startupId, StartupName = "Test Startup", Logo = "logo.jpg" };
            var chatRoom = new UserChatRoom
            {
                ChatRoomId = 1,
                Type = "UserToStartup",
                CreatedAt = DateTime.UtcNow,
                UserChatRoomMembers = new List<UserChatRoomMember>
                {
                    new UserChatRoomMember { ChatRoomMemberId = 1, AccountId = 1, Account = account },
                    new UserChatRoomMember { ChatRoomMemberId = 2, StartupId = startupId, Startup = startup }
                },
                UserMessages = new List<UserMessage>
                {
                    new UserMessage { MessageId = 1, ChatRoomId = 1, SenderAccountId = 1, Content = "Hello Startup", SentAt = DateTime.UtcNow }
                }
            };
            _context.Accounts.Add(account);
            _context.Startups.Add(startup);
            _context.UserChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userChatRepository.GetChatRoomsByStartupIdAsync(startupId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var chatRoomDto = result.First();
            Assert.Equal(1, chatRoomDto.ChatRoomId);
            Assert.Equal("UserToStartup", chatRoomDto.Type);
            Assert.Equal("Test User", chatRoomDto.TargetName);
            Assert.Equal(account.AccountProfile.AvatarUrl, chatRoomDto.TargetAvatar);
            Assert.Equal("Hello Startup", chatRoomDto.LatestMessageContent);
        }

        [Fact]
        public async Task GetChatRoomsByStartupIdAsync_NonExistingStartupId_ReturnsEmptyList()
        {
            // Arrange
            int startupId = 999;

            // Act
            var result = await _userChatRepository.GetChatRoomsByStartupIdAsync(startupId);

            // Assert
            Assert.Empty(result);
        }
        #endregion
    }
}