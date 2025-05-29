using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class PolicyRepository: IPolicyRepository
    {
        private readonly CAPSTONE_SUMMER2025Context _context;
        public PolicyRepository(CAPSTONE_SUMMER2025Context context)
        {
            _context = context;
        }

        public async Task<List<PolicyType>> GetAllPolicyTypeAsync()
        {
            return await _context.PolicyTypes.ToListAsync();
        }

        public async Task<PolicyType> GetPolicyTypeByIdAsync(int id)
        {
            return await _context.PolicyTypes.FirstOrDefaultAsync(pt => pt.PolicyTypeId == id);
        }

        public async Task AddPolicyTypeAsync(PolicyType policyType)
        {
            _context.PolicyTypes.Add(policyType);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePolicyTypeAsync(PolicyType policyType)
        {
            _context.PolicyTypes.Update(policyType);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePolicyTypeAsync(int id)
        {
            var policyType = await _context.PolicyTypes
                                           .Include(pt => pt.Policies)
                                           .FirstOrDefaultAsync(pt => pt.PolicyTypeId == id);

            if (policyType != null)
            {
                _context.Policies.RemoveRange(policyType.Policies); 
                _context.PolicyTypes.Remove(policyType);            
                await _context.SaveChangesAsync();
            }
        }


        public async Task<List<Policy>> GetAllPolicyAsync()
        {
            return await _context.Policies.ToListAsync();
        }

        public async Task<List<Policy>> GetAllActivePolicyAsync()
        {
            return await _context.Policies.Where(p => p.IsActive == true).ToListAsync();
        }

        public async Task<Policy> GetPolicyByIdAsync(int id)
        {
            return await _context.Policies.FirstOrDefaultAsync(p => p.PolicyId == id);
        }

        public async Task AddPolicyAsync(Policy policy)
        {
            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePolicyAsync(Policy policy)
        {
            _context.Policies.Update(policy);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePolicyAsync(int id)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy != null)
            {
                _context.Policies.Remove(policy);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Policy>> GetAllPoliciesByPolicyTypeAsync(int policyTypeId)
        {
            return await _context.Policies.Where(p => p.PolicyTypeId == policyTypeId).ToListAsync();
        }
    }
}
