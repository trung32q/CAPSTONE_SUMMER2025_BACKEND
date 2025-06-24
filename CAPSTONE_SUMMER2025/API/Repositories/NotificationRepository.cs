using API.Repositories.Interfaces;
using Google.Cloud.AIPlatform.V1;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        public async Task<Notification?> CreateNotificationAsync(int accountId, Notification notification)
        {
            var accountExists = await _context.Accounts.AnyAsync(acc => acc.AccountId == accountId);
            if (!accountExists) return null;
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            return notification;
        }
   
        public async Task<(List<Notification> Notifications, int TotalCount)> GetPagedNotificationsAsync(int accountId, int pageNumber, int pageSize)
        {
            // Truy vấn thông báo cho accountId
            var query = _context.Notifications
                .Where(x => x.AccountId == accountId)
                .OrderByDescending(n => n.SendAt);

            // Lấy tổng số thông báo
            int totalCount = await query.CountAsync();

            // Lấy danh sách phân trang
            var notifications = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (notifications, totalCount);
        }
        public async Task<int> GetUnreadNotificationCountAsync(int accountId)
        {
            return await _context.Notifications
                .Where(x => x.AccountId == accountId && x.IsRead==false)
                .CountAsync();
        }
        public async Task<bool> MarkNotificationAsReadAsync(int notificationId, int accountId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.AccountId == accountId);

            if (notification == null)
            {
                return false; // Thông báo không tồn tại hoặc không thuộc về accountId
            }

            if (notification.IsRead==true)
            {
                return true; 
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}