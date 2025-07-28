using API.DTO.Admin;
using API.Repositories.Interfaces;
using API.Service.Interface;

namespace API.Service
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;

        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<AdminDashBoardResult> GetAdminDashboardResult()
        {
            return new AdminDashBoardResult
            {
                NumAccount = await _adminRepository.GetCountAccountAsync(),
                NumAccountActive = await _adminRepository.GetCountAccountActiveAsync(),
                AccountsCreatedLast7Days = await _adminRepository.GetAccountsCreatedLast7DaysAsync(),
                NumStartup = await _adminRepository.GetStartupCountAsync(),
                StartupsCreatedLast7Days = await _adminRepository.GetStartupsCreatedLast7DaysAsync(),
                GrowthAccountStatsDtocs = await _adminRepository.GetUserGrowthRateAsync()
            };
        }
    }
}
