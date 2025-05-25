using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using API.DTO.PostDTO;

namespace API.Mapping
{
    public class MappingPost : Profile
    {
        public MappingPost() {
            CreateMap<Post, resPostDTO>();
            CreateMap<resPostDTO, Post>();

            CreateMap<PostComment, PostCommentDTO>();
            CreateMap<PostCommentDTO, PostComment>();

            CreateMap<PostLike, PostLikeDTO>();
            CreateMap<PostLikeDTO, PostLike>();

            CreateMap<PostMedia, PostMediaDTO>();
            CreateMap<PostMediaDTO, PostMedia>();
        }
    }
}
