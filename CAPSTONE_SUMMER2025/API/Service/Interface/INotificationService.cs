using API.DTO.AccountDTO;
using API.DTO.NotificationDTO;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface INotificationService
    {
        Task<resNotificationDTO> CreateAndSendAsync(reqNotificationDTO dto);
        Task<PagedResult<resNotificationDTO>> GetPagedNotificationsAsync(int accountId, int pageNumber, int pageSize);
        Task<int> GetUnreadNotificationCountAsync(int accountId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId, int accountId);
    }
}
