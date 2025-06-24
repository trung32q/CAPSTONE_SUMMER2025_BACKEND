using Infrastructure.Models;
using System.Collections.Generic;

namespace API.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification?> CreateNotificationAsync(int accountId, Notification notification);
        Task<(List<Notification> Notifications, int TotalCount)> GetPagedNotificationsAsync(int accountId, int pageNumber, int pageSize);
        Task<int> GetUnreadNotificationCountAsync(int accountId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId, int accountId);
    }
}
