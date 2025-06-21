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
           
            CreateMap<reqNotificationDTO, Notification>().ForAllOtherMembers(opt => opt.Ignore());
            CreateMap<Notification, resNotificationDTO>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.SendAt)).ForAllOtherMembers(opt => opt.Ignore());
        }
    }
}
