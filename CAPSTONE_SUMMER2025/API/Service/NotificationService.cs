using API.DTO.NotificationDTO;
using API.Repositories.Interfaces;
using API.Service.Interface;
using AutoMapper;

namespace API.Service
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<resNotificationDTO> CreateNotificationAsync(reqNotificationDTO dto)
        {
            var notificationEntity = await _repository.CreateNotificationAsync(dto.AccountId, dto.Content);
            if (notificationEntity == null) return null;
            return _mapper.Map<resNotificationDTO>(notificationEntity);
        }
    }
}
