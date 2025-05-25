using Infrastructure.Models;

namespace API.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification?> CreateNotificationAsync(int accountId, string mess);
    }
}
