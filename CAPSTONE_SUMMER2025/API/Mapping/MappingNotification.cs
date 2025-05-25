using API.DTO.NotificationDTO;
using API.DTO.PolicyDTO;
using AutoMapper;
using Infrastructure.Models;

namespace API.Mapping
{
    public class MappingNotification: Profile
    {
        public MappingNotification()
        {
            CreateMap<Notification, reqNotificationDTO>();
            CreateMap<reqNotificationDTO, Notification>();
        }
    }
}
