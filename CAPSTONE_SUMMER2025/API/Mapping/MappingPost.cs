using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using API.DTO.PostDTO;
using API.Repositories;

namespace API.Mapping
{
    public class MappingPost : Profile
    {
        public MappingPost()
        {
            CreateMap<Post, resPostDTO>().ForAllOtherMembers(opt => opt.Ignore());
            //CreateMap<resPostDTO, Post>();

            CreateMap<PostComment, PostCommentDTO>().ForAllOtherMembers(opt => opt.Ignore());
            CreateMap<PostCommentDTO, PostComment>().ForAllOtherMembers(opt => opt.Ignore()); 

            CreateMap<PostLike, PostLikeDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    src.Account.AccountProfile.FirstName + " " + src.Account.AccountProfile.LastName))
                        .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.Account.AccountProfile.AvatarUrl)).ForAllOtherMembers(opt => opt.Ignore()); 

            CreateMap<PostLikeDTO, PostLike>().ForAllOtherMembers(opt => opt.Ignore());



            CreateMap<PostMedium, PostMediaDTO>();
            CreateMap<PostMediaDTO, PostMedium>().ForAllOtherMembers(opt => opt.Ignore());

        }
    }
}
