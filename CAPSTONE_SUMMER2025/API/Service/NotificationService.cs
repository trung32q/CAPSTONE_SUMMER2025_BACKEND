using API.DTO.AccountDTO;
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
            return _mapper.Map<resNotificationDTO>(notificationEntity);
        }


        public async Task<PagedResult<resNotificationDTO>> GetPagedNotificationsAsync(int accountId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
                throw new ArgumentException("pageNumber and pageSize must be greater than 0");

            // 1. Lấy dữ liệu từ repository
            var (notifications, totalCount) = await _repository.GetPagedNotificationsAsync(accountId, pageNumber, pageSize);
            // 2. Map sang DTO
            var mapped = _mapper.Map<List<resNotificationDTO>>(notifications);

           foreach (var item in mapped)
            {
                item.AccountId = accountId;
            }


            // 3. Gửi dữ liệu qua SignalR
            await _hub.Clients.Group(accountId.ToString()).SendAsync("ReceivePagedNotifications", new
            {
                notifications = mapped, // ← gửi nguyên danh sách DTO
                totalCount,
                pageNumber,
                pageSize
            });

            return new PagedResult<resNotificationDTO>(mapped, totalCount, pageNumber, pageSize);
        }

        public async Task<int> GetUnreadNotificationCountAsync(int accountId)
        {
            try
            {                
                int unreadCount = await _repository.GetUnreadNotificationCountAsync(accountId);

                // B2: Gửi số lượng qua SignalR
                await _hub.Clients.Group(accountId.ToString()).SendAsync("ReceiveUnreadNotificationCount", new
                {
                    accountId,
                    unreadCount
                });
                return unreadCount;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve unread notification count", ex);
            }
        }
        public async Task<bool> MarkNotificationAsReadAsync(int notificationId, int accountId)
        {
            try
            {
                // B1: Cập nhật trạng thái đã đọc
                bool success = await _repository.MarkNotificationAsReadAsync(notificationId, accountId);
                if (!success)
                {
                    return false; // Thông báo không tồn tại hoặc không thuộc về accountId
                }

                // B2: Lấy số lượng thông báo chưa đọc mới
                int unreadCount = await _repository.GetUnreadNotificationCountAsync(accountId);

                // B3: Gửi cập nhật qua SignalR
                await _hub.Clients.Group(accountId.ToString()).SendAsync("NotificationRead", new
                {
                    notificationId,
                    isRead = true,
                    unreadCount
                });

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to mark notification as read", ex);
            }
        }

    }
}
