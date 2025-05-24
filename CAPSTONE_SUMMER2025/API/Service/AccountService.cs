using API.DTO.AccountDTO;
using API.DTO.BioDTO;
using API.DTO.ProfileDTO;
using API.Repositories.Interfaces;
using API.Service.Interface;
using AutoMapper;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Service
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        public AccountService(IMapper mapper, IAccountRepository accountRepository)
        {
            _mapper = mapper;
            _accountRepository = accountRepository;
        }
        public async Task<List<ResAccountDTO>> GetAllAccountAsync()
        {
            var accounts = await _accountRepository.GetAllAccountAsync();
            return _mapper.Map<List<ResAccountDTO>>(accounts);
        }
        public async Task<ResAccountInfoDTO> GetAccountByAccountIDAsync(int accountId)
        {
            var account = await _accountRepository.GetAccountByAccountIDAsync(accountId);
            if (account == null) return null;
            return _mapper.Map<ResAccountInfoDTO>(account);
        }

        public async Task<List<ResAccountInfoDTO>> GetFollowingAsync(int accountId)
        {
            var following = await _accountRepository.GetFollowingAsync(accountId);
            if (following == null) return null;
            return _mapper.Map<List<ResAccountInfoDTO>>(following);
        }
        public async Task<List<ResAccountInfoDTO>> GetFollowersAsync(int accountId)
        {
            var followers = await _accountRepository.GetFollowersAsync(accountId);
            if (followers == null) return null;
            return _mapper.Map<List<ResAccountInfoDTO>>(followers);
        }

        public async Task<ResAccountInfoDTO> UpdateProfileAsync(int accountId, ReqUpdateProfileDTO updateProfileDTO)
        {
            var account = await _accountRepository.GetAccountWithProfileByIdAsync(accountId);
            if (account == null || account.AccountProfile == null)
                return null;

            var profile = account.AccountProfile;

            if (!string.IsNullOrWhiteSpace(updateProfileDTO.FirstName)) profile.FirstName = updateProfileDTO.FirstName;
            if (!string.IsNullOrWhiteSpace(updateProfileDTO.LastName)) profile.LastName = updateProfileDTO.LastName;
            if (!string.IsNullOrWhiteSpace(updateProfileDTO.Gender)) profile.Gender = updateProfileDTO.Gender;
            if (updateProfileDTO.Dob.HasValue) profile.Dob = updateProfileDTO.Dob.Value;
            if (!string.IsNullOrWhiteSpace(updateProfileDTO.Address)) profile.Address = updateProfileDTO.Address;
            if (!string.IsNullOrWhiteSpace(updateProfileDTO.PhoneNumber)) profile.PhoneNumber = updateProfileDTO.PhoneNumber;
            if (!string.IsNullOrWhiteSpace(updateProfileDTO.AvatarUrl)) profile.AvatarUrl = updateProfileDTO.AvatarUrl;

            await _accountRepository.SaveChangesAsync();

            return _mapper.Map<ResAccountInfoDTO>(account);
        }

        public async Task<ResAccountInfoDTO> UpdateBioAsync(int accountId, ReqUpdateBioDTO updateBioDTO)
        {
            var account = await _accountRepository.GetAccountWithBioByIdAsync(accountId);
            if (account == null || account.Bio == null)
                return null;

            var bio = account.Bio;

            if (!string.IsNullOrWhiteSpace(updateBioDTO.IntroTitle)) bio.IntroTitle = updateBioDTO.IntroTitle;
            if (!string.IsNullOrWhiteSpace(updateBioDTO.Position)) bio.Position = updateBioDTO.Position;
            if (!string.IsNullOrWhiteSpace(updateBioDTO.Workplace)) bio.Workplace = updateBioDTO.Workplace;
            if (!string.IsNullOrWhiteSpace(updateBioDTO.FacebookUrl)) bio.FacebookUrl = updateBioDTO.FacebookUrl;
            if (!string.IsNullOrWhiteSpace(updateBioDTO.LinkedinUrl)) bio.LinkedinUrl = updateBioDTO.LinkedinUrl;
            if (!string.IsNullOrWhiteSpace(updateBioDTO.GithubUrl)) bio.GithubUrl = updateBioDTO.GithubUrl;
            if (!string.IsNullOrWhiteSpace(updateBioDTO.PortfolioUrl)) bio.PortfolioUrl = updateBioDTO.PortfolioUrl;
            if (!string.IsNullOrWhiteSpace(updateBioDTO.Country)) bio.Country = updateBioDTO.Country;

            await _accountRepository.SaveChangesAsync();

            return _mapper.Map<ResAccountInfoDTO>(account);
        }
        public async Task<bool> ChangePasswordAsync(int accountId, ChangePasswordDTO changePasswordDTO)
        {
            var account = await _accountRepository.GetAccountByIdAsync(accountId);
            if (account == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDTO.OldPassword, account.Password))
                return false;

            if (changePasswordDTO.NewPassword != changePasswordDTO.ConfirmPassword)
                return false;

            account.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDTO.NewPassword);
            await _accountRepository.SaveChangesAsync();

            return true;
        }       
    }
}
