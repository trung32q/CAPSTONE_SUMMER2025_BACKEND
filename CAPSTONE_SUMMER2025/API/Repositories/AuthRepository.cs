using API.DTO.AccountDTO;
using API.DTO.AuthDTO;
using API.Repositories.Interfaces;
using AutoMapper;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using API.Utils.Constants;
using API.Service;
using Org.BouncyCastle.Ocsp;
using API.Service.Interface;
namespace API.Repositories
{
    public class AuthRepository : IAuthRepository
    {

        private readonly IMapper _mapper;
        private readonly CAPSTONE_SUMMER2025Context _context;
        private IConfiguration _configuration;
      
        public AuthRepository(IMapper mapper, CAPSTONE_SUMMER2025Context context, IConfiguration configuration)
        {
            _mapper = mapper;
            _context = context;
            _configuration = configuration;
          
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _context.Accounts.AnyAsync(a => a.Email == email);
        }

        public async Task AddAccountAsync(Account account)
        {
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
        }

        public async Task SaveOtpAsync(UserOtp otp)
        {
            _context.UserOtps.Add(otp);
            await _context.SaveChangesAsync();
        }

        public async Task<Account> GetAccountByEmailAsync(string email)
        {
            return await _context.Accounts
                .Include(a => a.AccountProfile)
                .FirstOrDefaultAsync(a => a.Email == email);
        }
        public async Task<Account> GetAccountByIdAsync(int accountId)
        {
            return await _context.Accounts
                       .FirstOrDefaultAsync(x => x.AccountId == accountId);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<UserOtp> GetActiveUserOtpAsync(int accountId)
        {
            return await _context.UserOtps
                                 .Where(o => o.AccountId == accountId &&
                                             o.ExpiresAt > DateTime.UtcNow)
                                 .OrderByDescending(o => o.UserOtpId)
                                 .FirstOrDefaultAsync();
        }    
    }
}
