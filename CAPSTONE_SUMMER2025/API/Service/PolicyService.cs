using API.DTO.PolicyDTO;
using API.Repositories.Interfaces;
using API.Service.Interface;
using AutoMapper;
using Infrastructure.Models;

namespace API.Service
{
    public class PolicyService:IPolicyService
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

        public async Task<resPolicyTypeDTO> GetPolicyTypeByIdAsync(int id)
        {
            var entity = await _repository.GetPolicyTypeByIdAsync(id);
            return _mapper.Map<resPolicyTypeDTO>(entity);
        }

        public async Task AddPolicyTypeAsync(reqPolicyTypeDTO dto)
        {
            var entity = _mapper.Map<PolicyType>(dto);
            await _repository.AddPolicyTypeAsync(entity);
        }

        public async Task UpdatePolicyTypeAsync(int id, reqPolicyTypeDTO dto)
        {
            var existing = await _repository.GetPolicyTypeByIdAsync(id);
            if (existing == null) return;
            _mapper.Map(dto, existing);
            await _repository.UpdatePolicyTypeAsync(existing);
        }

        public async Task DeletePolicyTypeAsync(int id)
        {
            await _repository.DeletePolicyTypeAsync(id);
        }

        // === Policy ===
        public async Task<List<resPolicyDTO>> GetAllPoliciesAsync()
        {
            var entities = await _repository.GetAllPolicyAsync();
            return _mapper.Map<List<resPolicyDTO>>(entities);
        }

        public async Task<resPolicyDTO> GetPolicyByIdAsync(int id)
        {
            var entity = await _repository.GetPolicyByIdAsync(id);
            return _mapper.Map<resPolicyDTO>(entity);
        }

        public async Task AddPolicyAsync(reqPolicyDTO dto)
        {
            var entity = _mapper.Map<Policy>(dto);
            entity.CreateAt = DateTime.UtcNow;
            await _repository.AddPolicyAsync(entity);
        }

        public async Task UpdatePolicyAsync(int id, reqPolicyDTO dto)
        {
            var existing = await _repository.GetPolicyByIdAsync(id);
            if (existing == null) return;
            _mapper.Map(dto, existing);
            await _repository.UpdatePolicyAsync(existing);
        }

        public async Task DeletePolicyAsync(int id)
        {
            await _repository.DeletePolicyAsync(id);
        }

        public async Task<List<resPolicyDTO>> GetPoliciesByPolicyTypeAsync(int policyTypeId)
        {
            var entities = await _repository.GetAllPoliciesByPolicyTypeAsync(policyTypeId);
            return _mapper.Map<List<resPolicyDTO>>(entities);
        }
    }
}
