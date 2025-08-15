using API.Repositories;
using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using UnitTest;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTest.Repositories
{
    public class NotificationRepositoryTest
    {
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly INotificationRepository _notificationRepository;

        public NotificationRepositoryTest()
        {
            _context = TestHelper.CreateInMemoryDbContext();
            _notificationRepository = new NotificationRepository(_context);
        }

        [Fact]
        public async Task CreateNotificationAsync_ValidInput_ReturnsNotification()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            var notification = new Notification
            {
                NotificationId = 1,
                AccountId = 1,
                Content = "New post liked",
                IsRead = false,
                SendAt = DateTime.UtcNow
            };

            // Act
            var result = await _notificationRepository.CreateNotificationAsync(1, notification);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.NotificationId);
            Assert.Equal(1, result.AccountId);
            Assert.Equal("New post liked", result.Content);
            Assert.False(result.IsRead);
            var savedNotification = await _context.Notifications.FindAsync(1);
            Assert.NotNull(savedNotification);
            Assert.Equal("New post liked", savedNotification.Content);
        }

        [Fact]
        public async Task CreateNotificationAsync_NonExistingAccountId_ReturnsNull()
        {
            // Arrange
            var notification = new Notification
            {
                NotificationId = 1,
                AccountId = 999,
                Content = "New post liked",
                IsRead = false,
                SendAt = DateTime.UtcNow
            };

            // Act
            var result = await _notificationRepository.CreateNotificationAsync(999, notification);

            // Assert
            Assert.Null(result);
            Assert.Equal(0, await _context.Notifications.CountAsync());
        }

        [Fact]
        public async Task GetPagedNotificationsAsync_ValidInput_ReturnsPagedNotifications()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            _context.Accounts.Add(account);
            var notifications = new List<Notification>
            {
                new Notification { NotificationId = 1, AccountId = 1, Content = "Notification 1", IsRead = false, SendAt = DateTime.UtcNow.AddMinutes(-1) },
                new Notification { NotificationId = 2, AccountId = 1, Content = "Notification 2", IsRead = true, SendAt = DateTime.UtcNow }
            };
            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            // Act
            var (notificationsResult, totalCount) = await _notificationRepository.GetPagedNotificationsAsync(1, 1, 10);

            // Assert
            Assert.NotNull(notificationsResult);
            Assert.Equal(2, totalCount);
            Assert.Equal(2, notificationsResult.Count);
            Assert.Equal("Notification 2", notificationsResult[0].Content);
            Assert.Equal("Notification 1", notificationsResult[1].Content);
        }

        [Fact]
        public async Task GetPagedNotificationsAsync_NonExistingAccountId_ReturnsEmptyResult()
        {
            // Arrange

            // Act
            var (notificationsResult, totalCount) = await _notificationRepository.GetPagedNotificationsAsync(999, 1, 10);

            // Assert
            Assert.NotNull(notificationsResult);
            Assert.Empty(notificationsResult);
            Assert.Equal(0, totalCount);
        }

        #region GetUnreadNotificationCountAsync
        [Fact]
        public async Task GetUnreadNotificationCountAsync_ValidAccountId_ReturnsUnreadCount()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            var notifications = new List<Notification>
            {
                new Notification { NotificationId = 1, AccountId = 1, Content = "Notification 1", IsRead = false, SendAt = DateTime.UtcNow },
                new Notification { NotificationId = 2, AccountId = 1, Content = "Notification 2", IsRead = true, SendAt = DateTime.UtcNow },
                new Notification { NotificationId = 3, AccountId = 1, Content = "Notification 3", IsRead = false, SendAt = DateTime.UtcNow }
            };
            _context.Accounts.Add(account);
            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            // Act
            var result = await _notificationRepository.GetUnreadNotificationCountAsync(1);

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetUnreadNotificationCountAsync_NoUnreadNotifications_ReturnsZero()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            var notification = new Notification
            {
                NotificationId = 1,
                AccountId = 1,
                Content = "Notification 1",
                IsRead = true,
                SendAt = DateTime.UtcNow
            };
            _context.Accounts.Add(account);
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Act
            var result = await _notificationRepository.GetUnreadNotificationCountAsync(1);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetUnreadNotificationCountAsync_NonExistingAccountId_ReturnsZero()
        {
            // Arrange
            // Act
            var result = await _notificationRepository.GetUnreadNotificationCountAsync(999);
            // Assert
            Assert.Equal(0, result);
        }
        #endregion

        [Fact]
        public async Task MarkNotificationAsReadAsync_ValidInput_ReturnsTrue()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            var notification = new Notification
            {
                NotificationId = 1,
                AccountId = 1,
                Content = "Notification 1",
                IsRead = false,
                SendAt = DateTime.UtcNow
            };
            _context.Accounts.Add(account);
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Act
            var result = await _notificationRepository.MarkNotificationAsReadAsync(1, 1);

            // Assert
            Assert.True(result);
            var updatedNotification = await _context.Notifications.FindAsync(1);
            Assert.True(updatedNotification.IsRead);
        }

        [Fact]
        public async Task MarkNotificationAsReadAsync_NonExistingNotificationId_ReturnsFalse()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _notificationRepository.MarkNotificationAsReadAsync(999, 1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task MarkNotificationAsReadAsync_NonExistentAccountId_ReturnsFalse()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            var notification = new Notification
            {
                NotificationId = 1,
                AccountId = 1,
                Content = "Notification 1",
                IsRead = false,
                SendAt = DateTime.UtcNow
            };
            _context.Accounts.Add(account);
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Act
            var result = await _notificationRepository.MarkNotificationAsReadAsync(1, 999);

            // Assert
            Assert.False(result);
            var notificationCheck = await _context.Notifications.FindAsync(1);
            Assert.False(notificationCheck.IsRead);
        }

        [Fact]
        public async Task MarkNotificationAsReadAsync_AlreadyReadNotification_ReturnsTrue()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            var notification = new Notification
            {
                NotificationId = 1,
                AccountId = 1,
                Content = "Notification 1",
                IsRead = true,
                SendAt = DateTime.UtcNow
            };
            _context.Accounts.Add(account);
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Act
            var result = await _notificationRepository.MarkNotificationAsReadAsync(1, 1);

            // Assert
            Assert.True(result);
            var updatedNotification = await _context.Notifications.FindAsync(1);
            Assert.True(updatedNotification.IsRead);
        }
    }
}