using API.DTO.StartupDTO;
using API.Utils.Constants;
using AutoMapper;
using Infrastructure.Models;

namespace API.Mapping
{
    public class MappingStartup : Profile
    {
        public MappingStartup() {
            CreateMap<CreateStartupRequest, Startup>()
            .ForMember(dest => dest.StartupName, opt => opt.MapFrom(src => src.StartupName))
            .ForMember(dest => dest.AbbreviationName, opt => opt.MapFrom(src => src.AbbreviationName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Vision, opt => opt.MapFrom(src => src.Vision))
            .ForMember(dest => dest.Mission, opt => opt.MapFrom(src => src.Mission))
            .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => src.Logo))
            .ForMember(dest => dest.BackgroundUrl, opt => opt.MapFrom(src => src.BackgroundUrl))
            .ForMember(dest => dest.WebsiteUrl, opt => opt.MapFrom(src => src.WebsiteUrl))
             .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.StageId, opt => opt.MapFrom(src => src.StageId))
            .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => StartupStatus.UNVERIFIED))
            .ForAllOtherMembers(opt => opt.Ignore());


        }
    }
}
