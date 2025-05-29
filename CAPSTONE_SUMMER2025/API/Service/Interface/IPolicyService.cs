using API.DTO.PolicyDTO;
using API.Repositories.Interfaces;
using AutoMapper;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface IPolicyService
    {
        Task<List<resPolicyTypeDTO>> GetAllPolicyTypesAsync();
        Task<resPolicyTypeDTO> GetPolicyTypeByIdAsync(int id);
        Task<(bool Success, string Message)> AddPolicyTypeAsync(reqPolicyTypeDTO dto);
        Task<(bool Success, string Message)> UpdatePolicyTypeAsync(int id, reqPolicyTypeDTO dto);
        Task<(bool Success, string Message)> DeletePolicyTypeAsync(int id);

        // === Policy ===
        Task<List<resPolicyDTO>> GetAllPoliciesAsync();
        Task<resPolicyDTO> GetPolicyByIdAsync(int id);
        Task<(bool Success, string Message)> AddPolicyAsync(reqPolicyDTO dto);
        Task<(bool Success, string Message)> UpdatePolicyAsync(int id, reqPolicyDTO dto);
        Task<(bool Success, string Message)> DeletePolicyAsync(int id);
        Task<List<resPolicyDTO>> GetPoliciesByPolicyTypeAsync(int policyTypeId);

        Task<(bool Success, string Message)> UpdatePolicyStatus(int id, bool isActive);
        Task<List<resPolicyDTO>> GetAllActivePoliciesAsync();


    }
}
