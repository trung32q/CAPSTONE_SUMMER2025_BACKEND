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
        private readonly FilebaseHandler filebaseHandler;
        public MappingPost() {
            CreateMap<Post, resPostDTO>();
            CreateMap<resPostDTO, Post>();

            CreateMap<PostComment, PostCommentDTO>();
            CreateMap<PostCommentDTO, PostComment>();

            CreateMap<PostLike, PostLikeDTO>();
            CreateMap<PostLikeDTO, PostLike>();
            CreateMap<PostMedium, PostMediaDTO>();
                     
            CreateMap<PostMediaDTO, PostMedium>().ForMember(dest => dest.MediaUrl,
                                  opt => opt.MapFrom(src => filebaseHandler.GeneratePreSignedUrl(src.MediaUrl))); 
        }
    }
}
