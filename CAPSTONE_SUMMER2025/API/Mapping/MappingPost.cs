using AutoMapper;
using Infrastructure.Models;
using API.DTO.PostDTO;

namespace API.Mapping
{
    public class MappingPost : Profile
    {
        public MappingPost()
        {
            CreateMap<Post, ResPostDTO>()
                .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.PostId))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreateAt))
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Account.AccountProfile.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Account.AccountProfile.LastName))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.Account.AccountProfile.AvatarUrl))
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.PostLikes.Count))
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.PostComments.Count))
                .ForMember(dest => dest.IsLiked, opt => opt.Ignore());

            CreateMap<ReqPostDTO, Post>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
} 