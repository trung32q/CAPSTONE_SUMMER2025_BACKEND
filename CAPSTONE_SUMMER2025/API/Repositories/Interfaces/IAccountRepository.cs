using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.DTO;
using API.DTO.AccountDTO;
using API.DTO.ProfileDTO;
using API.DTO.BioDTO;
namespace API.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<List<ResAccountDTO>> GetAllAccountAsync();
        Task<Account> GetAccountByEmailAsync(string email);
        Task<ResAccountInfoDTO> GetAccountByAccountIDAsync(int accountId);
        Task<List<ResAccountInfoDTO>> GetFollowingAsync(int accountId);
        Task<List<ResAccountInfoDTO>> GetFollowersAsync(int accountId);
        Task<ResAccountInfoDTO> UpdateProfileAsync(int accountId, ReqUpdateProfileDTO updateProfileDTO);
        Task<ResAccountInfoDTO> UpdateBioAsync(int accountId, ReqUpdateBioDTO updateBioDTO);
        Task<bool> ChangePasswordAsync(int accountId, ChangePasswordDTO changePasswordDTO);
    }
}
