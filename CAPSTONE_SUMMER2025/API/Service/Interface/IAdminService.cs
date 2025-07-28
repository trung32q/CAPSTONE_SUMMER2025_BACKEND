using API.DTO.Admin;

namespace API.Service.Interface
{
    public interface IAdminService
    {
        Task<AdminDashBoardResult> GetAdminDashboardResult();
    }
}
