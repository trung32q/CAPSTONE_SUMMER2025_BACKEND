using Infrastructure.Models;
using API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using API.DTO.AccountDTO;
using API.Utils.Constants;
using API.DTO.AuthDTO;
namespace API.Mapping
{
    public class MappingAccount : Profile
    {
        public MappingAccount()
        {
            // Ánh xạ từ Account sang ResAccountDTO
            CreateMap<Account, ResAccountDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.AccountProfile.FirstName))  // Lấy FirstName từ AccountProfiles
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.AccountProfile.LastName))    // Lấy LastName từ AccountProfiles
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                    .ForAllOtherMembers(opt => opt.Ignore());
            ;

            // Map từ ReqAccountDTO sang Account
            CreateMap<ReqAccountDTO, Account>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.Ignore()) // bỏ qua, vì sẽ hash
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => RoleConst.STARTUP))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => AccountStatusConst.DEACTIVE))
                    .ForAllOtherMembers(opt => opt.Ignore());
            ;

            // Tạo mapping từ Account Model sang ResLoginDTO
            CreateMap<Account, ResLoginDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore())
                    .ForAllOtherMembers(opt => opt.Ignore());
            ;

            CreateMap<Account, ResAccountInfoDTO>()
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.AccountProfile.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.AccountProfile.LastName))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.AccountProfile != null ? src.AccountProfile.Gender : null))
                .ForMember(dest => dest.Dob, opt => opt.MapFrom(src => src.AccountProfile != null ? src.AccountProfile.Dob : (DateTime?)null))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.AccountProfile != null ? src.AccountProfile.Address : null))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.AccountProfile != null ? src.AccountProfile.PhoneNumber : null))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AccountProfile != null ? src.AccountProfile.AvatarUrl : null))
                  .ForMember(dest => dest.BackgroundUrl, opt => opt.MapFrom(src => src.AccountProfile != null ? src.AccountProfile.BackgroundUrl : null))
                .ForMember(dest => dest.IntroTitle, opt => opt.MapFrom(src => src.AccountProfile != null ? src.Bio.IntroTitle : null))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Bio != null ? src.Bio.Position : null))
                .ForMember(dest => dest.Workplace, opt => opt.MapFrom(src => src.Bio != null ? src.Bio.Workplace : null))
                .ForMember(dest => dest.FacebookUrl, opt => opt.MapFrom(src => src.Bio != null ? src.Bio.FacebookUrl : null))
                .ForMember(dest => dest.LinkedinUrl, opt => opt.MapFrom(src => src.Bio != null ? src.Bio.LinkedinUrl : null))
                .ForMember(dest => dest.GithubUrl, opt => opt.MapFrom(src => src.Bio != null ? src.Bio.GithubUrl : null))
                .ForMember(dest => dest.PortfolioUrl, opt => opt.MapFrom(src => src.Bio != null ? src.Bio.PortfolioUrl : null))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Bio != null ? src.Bio.Country : null))
                .ForMember(dest => dest.PostCount, opt => opt.MapFrom(src => src.Posts.Count()))
                .ForMember(dest => dest.FollowingCount, opt => opt.MapFrom(src => src.FollowFollowerAccounts.Count))
                .ForMember(dest => dest.FollowerCount, opt => opt.MapFrom(src => src.FollowFollowingAccounts.Count))
                    .ForAllOtherMembers(opt => opt.Ignore());
            ;


        }
    }
}


