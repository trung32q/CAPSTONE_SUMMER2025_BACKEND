using API.DTO.NotificationDTO;
using API.Hubs;
using API.Repositories.Interfaces;
using API.Service.Interface;
using API.Utils.Constants;
using AutoMapper;
using Infrastructure.Models;
using Microsoft.AspNetCore.SignalR;
using System;

namespace API.Service
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository repository, IMapper mapper, IHubContext<NotificationHub> hub)
        {
            _repository = repository;
            _mapper = mapper;
            _hub = hub;
        }

        public async Task<resNotificationDTO> CreateAndSendAsync(reqNotificationDTO dto)
        {
            // B1: Lưu vào DB
            var noti = new Notification
            {
                AccountId = dto.UserId,
                Content = dto.Message,
                IsRead = false,
                SendAt = DateTime.UtcNow
            };

            var notificationEntity = await _repository.CreateNotificationAsync(dto.UserId, dto.Message);

            // B2: Gửi realtime qua SignalR
            await _hub.Clients.Group(dto.UserId.ToString()).SendAsync("ReceiveNotification", new
            {
                id = noti.NotificationId,
                message = noti.Content,
                IsRead = noti.IsRead,
                SendAt = noti.IsRead
            });
            return _mapper.Map<resNotificationDTO>(notificationEntity);
        }
       
    }
}
