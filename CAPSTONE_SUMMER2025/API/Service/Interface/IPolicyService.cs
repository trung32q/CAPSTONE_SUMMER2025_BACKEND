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
        Task AddPolicyTypeAsync(reqPolicyTypeDTO dto);
        Task UpdatePolicyTypeAsync(int id, reqPolicyTypeDTO dto);
        Task DeletePolicyTypeAsync(int id);

        // === Policy ===
        Task<List<resPolicyDTO>> GetAllPoliciesAsync();
        Task<resPolicyDTO> GetPolicyByIdAsync(int id);
        Task AddPolicyAsync(reqPolicyDTO dto);
        Task UpdatePolicyAsync(int id, reqPolicyDTO dto);
        Task DeletePolicyAsync(int id);
        Task<List<resPolicyDTO>> GetPoliciesByPolicyTypeAsync(int policyTypeId);
    }
}
