using API.Repositories.Interfaces;
using Google.Cloud.AIPlatform.V1;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace API.Repositories
{
    class NotificationRepository : INotificationRepository
    {
        private readonly CAPSTONE_SUMMER2025Context _context;

        public NotificationRepository(CAPSTONE_SUMMER2025Context context)
        {
            _context = context;
        }

        public async Task<Notification?> CreateNotificationAsync(int accountId, string content)
        {
            var accountExists = await _context.Accounts.AnyAsync(acc => acc.AccountId == accountId);
            if (!accountExists) return null;

            var notification = new Notification
            {
                AccountId = accountId,
                Content = content,
                IsRead = false,
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            return notification;
        }
    }
}