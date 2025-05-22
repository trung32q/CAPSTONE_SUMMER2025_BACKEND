using API.DTOs;
using AutoMapper;
using Infrastructure.Models;

namespace API.Mappings
{
    public class AccountBlockMappingProfile : Profile
    {
        public AccountBlockMappingProfile()
        {
            CreateMap<AccountBlock, AccountBlockResponseDTO>();
            CreateMap<BlockAccountDTO, AccountBlock>();
        }
    }
} 