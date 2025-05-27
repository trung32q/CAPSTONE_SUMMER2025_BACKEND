using API.DTO.NotificationDTO;

namespace API.Service.Interface
{
    public interface INotificationService
    {
        Task<resNotificationDTO> CreateAndSendAsync(reqNotificationDTO dto);
    }
}
