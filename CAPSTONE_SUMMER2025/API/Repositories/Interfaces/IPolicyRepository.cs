using Infrastructure.Models;

namespace API.Repositories.Interfaces
{
    public interface IPolicyRepository
    {
        Task<List<PolicyType>> GetAllPolicyTypeAsync();
        Task<PolicyType> GetPolicyTypeByIdAsync(int id);
        Task AddPolicyTypeAsync(PolicyType policyType);
        Task UpdatePolicyTypeAsync(PolicyType policyType);
        Task DeletePolicyTypeAsync(int id);
        Task<List<Policy>> GetAllPolicyAsync();
        Task<Policy> GetPolicyByIdAsync(int id);
        Task AddPolicyAsync(Policy policy);
        Task UpdatePolicyAsync(Policy policy);
        Task DeletePolicyAsync(int id);
        Task<List<Policy>> GetAllPoliciesByPolicyTypeAsync(int policyTypeId);
        Task<List<Policy>> GetAllActivePolicyAsync();
    }
}
