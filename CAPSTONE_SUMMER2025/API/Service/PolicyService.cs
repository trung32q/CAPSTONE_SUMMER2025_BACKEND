using API.DTO.PolicyDTO;
using API.Repositories.Interfaces;
using API.Service.Interface;
using AutoMapper;
using Infrastructure.Models;

namespace API.Service
{
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyRepository _repository;
        private readonly IMapper _mapper;

        public PolicyService(IPolicyRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // === PolicyType ===
        public async Task<List<resPolicyTypeDTO>> GetAllPolicyTypesAsync()
        {
            var entities = await _repository.GetAllPolicyTypeAsync();
            return _mapper.Map<List<resPolicyTypeDTO>>(entities);
        }

        public async Task<resPolicyTypeDTO?> GetPolicyTypeByIdAsync(int id)
        {
            var entity = await _repository.GetPolicyTypeByIdAsync(id);
            return entity == null ? null : _mapper.Map<resPolicyTypeDTO>(entity);
        }

        public async Task<(bool Success, string Message)> AddPolicyTypeAsync(reqPolicyTypeDTO dto)
        {
            var entity = _mapper.Map<PolicyType>(dto);
            await _repository.AddPolicyTypeAsync(entity);
            return (true, "Policy type created successfully");
        }

        public async Task<(bool Success, string Message)> UpdatePolicyTypeAsync(int id, reqPolicyTypeDTO dto)
        {
            var existing = await _repository.GetPolicyTypeByIdAsync(id);
            if (existing == null) return (false, "Policy type not found");

            _mapper.Map(dto, existing);
            await _repository.UpdatePolicyTypeAsync(existing);
            return (true, "Policy type updated successfully");
        }

        public async Task<(bool Success, string Message)> DeletePolicyTypeAsync(int id)
        {
            var existing = await _repository.GetPolicyTypeByIdAsync(id);
            if (existing == null) return (false, "Policy type not found");

            await _repository.DeletePolicyTypeAsync(id);
            return (true, "Policy type deleted successfully");
        }

        // === Policy ===
        public async Task<List<resPolicyDTO>> GetAllPoliciesAsync()
        {
            var entities = await _repository.GetAllPolicyAsync();
            return _mapper.Map<List<resPolicyDTO>>(entities);
        }

        public async Task<List<resPolicyDTO>> GetAllActivePoliciesAsync()
        {
            var entities = await _repository.GetAllActivePolicyAsync();
            return _mapper.Map<List<resPolicyDTO>>(entities);
        }

        public async Task<resPolicyDTO?> GetPolicyByIdAsync(int id)
        {
            var entity = await _repository.GetPolicyByIdAsync(id);
            return entity == null ? null : _mapper.Map<resPolicyDTO>(entity);
        }

        public async Task<(bool Success, string Message)> AddPolicyAsync(reqPolicyDTO dto)
        {
            var entity = _mapper.Map<Policy>(dto);
            await _repository.AddPolicyAsync(entity);
            return (true, "Policy created successfully");
        }

        public async Task<(bool Success, string Message)> UpdatePolicyAsync(int id, reqPolicyDTO dto)
        {
            var existing = await _repository.GetPolicyByIdAsync(id);
            if (existing == null) return (false, "Policy not found");

            _mapper.Map(dto, existing);
            await _repository.UpdatePolicyAsync(existing);
            return (true, "Policy updated successfully");
        }

        public async Task<(bool Success, string Message)> DeletePolicyAsync(int id)
        {
            var existing = await _repository.GetPolicyByIdAsync(id);
            if (existing == null) return (false, "Policy not found");

            await _repository.DeletePolicyAsync(id);
            return (true, "Policy deleted successfully");
        }

        public async Task<List<resPolicyDTO>> GetPoliciesByPolicyTypeAsync(int policyTypeId)
        {
            var entities = await _repository.GetAllPoliciesByPolicyTypeAsync(policyTypeId);
            return _mapper.Map<List<resPolicyDTO>>(entities);
        }
        public async Task<(bool Success, string Message)> UpdatePolicyStatus(int id, bool isActive)
        {
            var existing = await _repository.GetPolicyByIdAsync(id);
            if (existing == null) return (false, "Policy not found");

            existing.IsActive = isActive;
            await _repository.UpdatePolicyAsync(existing);
            return (true, "Policy updated successfully");
        }
    }
}
