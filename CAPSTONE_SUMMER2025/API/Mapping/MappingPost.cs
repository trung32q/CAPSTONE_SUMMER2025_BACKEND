﻿using Infrastructure.Models;
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
            CreateMap<Post, resPostDTO>();
            CreateMap<resPostDTO, Post>();

            CreateMap<PostComment, PostCommentDTO>();
            CreateMap<PostCommentDTO, PostComment>();

            CreateMap<PostLike, PostLikeDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    src.Account.AccountProfile.FirstName + " " + src.Account.AccountProfile.LastName))
                        .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.Account.AccountProfile.AvatarUrl));

            CreateMap<PostLikeDTO, PostLike>();



            CreateMap<PostMedium, PostMediaDTO>();        
            CreateMap<PostMediaDTO, PostMedium>();

        }
    }
}
