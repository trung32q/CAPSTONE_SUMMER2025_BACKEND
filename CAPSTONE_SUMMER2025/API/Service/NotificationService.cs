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
        private readonly IAccountRepository _accrepository;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository repository, IMapper mapper, IHubContext<NotificationHub> hub,IAccountRepository accountRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _hub = hub;
            _accrepository = accountRepository;
        }

        public async Task<resNotificationDTO> CreateAndSendAsync(reqNotificationDTO dto)
        {
            // B1: Lưu vào DB
            var noti = new Notification
            {
                AccountId = dto.UserId,
                Content = dto.Message,
                IsRead = false,
                SendAt = DateTime.Now,
                SenderId=dto.senderid,
                TargetUrl=dto.TargetURL,
                NotificationType = dto.NotificationType
                
            };
            var account =await _accrepository.GetAccountByAccountIDAsync(dto.senderid);
            var notificationDto = new resNotificationDTO
            {
                NotificationId = noti.NotificationId,
                AccountId = (int)noti.AccountId,
                Content = noti.Content,
                CreatedAt = (DateTime)noti.SendAt,
                IsRead = (bool)noti.IsRead,
                SenderID = (int)noti.SenderId,
                Type = noti.NotificationType,
                TargetURL = noti.TargetUrl,
                AvartarURL=account.AccountProfile.AvatarUrl,
            };
            var notificationEntity = await _repository.CreateNotificationAsync(dto.UserId,noti);
            // Gửi qua Hub (SignalR)
            await _hub.Clients.Group(dto.UserId.ToString())
                .SendAsync("ReceiveNotification", notificationDto);
            return _mapper.Map<resNotificationDTO>(notificationEntity);
        }


        public async Task<PagedResult<resNotificationDTO>> GetPagedNotificationsAsync(int accountId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
                throw new ArgumentException("pageNumber and pageSize must be greater than 0");

            var (notifications, totalCount) = await _repository.GetPagedNotificationsAsync(accountId, pageNumber, pageSize);

            var mapped = new List<resNotificationDTO>();

            foreach (var n in notifications)
            {
                string avatarUrl = null;
                // Nếu có senderId thì lấy account profile
                if (n.SenderId > 0)
                {
                    var senderAccount = await _accrepository.GetAccountByAccountIDAsync((int)n.SenderId);
                    avatarUrl = senderAccount?.AccountProfile?.AvatarUrl;
                }

                mapped.Add(new resNotificationDTO
                {
                    NotificationId = n.NotificationId,
                    AccountId = n.AccountId ?? 0,
                    Content = n.Content,
                    CreatedAt = n.SendAt ?? DateTime.MinValue,
                    IsRead = n.IsRead ?? false,
                    SenderID = n.SenderId ?? 0,
                    Type = n.NotificationType,
                    TargetURL = n.TargetUrl,
                    AvartarURL = avatarUrl
                });
            }

            await _hub.Clients.Group(accountId.ToString()).SendAsync("ReceivePagedNotifications", new
            {
                notifications = mapped,
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
