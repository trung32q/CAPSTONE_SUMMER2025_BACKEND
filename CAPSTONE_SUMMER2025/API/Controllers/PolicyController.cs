using API.DTO.PolicyDTO;
using API.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api")]
    [ApiController]
    public class PolicyController : ControllerBase
    {
        private readonly IPolicyService _service;

        public PolicyController(IPolicyService service)
        {
            _service = service;
        }

        // ===== POLICY TYPES =====

        [HttpGet("policy-types")]
        public async Task<IActionResult> GetAllPolicyTypes()
        {
            var result = await _service.GetAllPolicyTypesAsync();
            return Ok(result);
        }

        [HttpGet("policy-types/{id}")]
        public async Task<IActionResult> GetPolicyTypeById(int id)
        {
            var result = await _service.GetPolicyTypeByIdAsync(id);
            if (result == null)
                return NotFound(new { error = $"Policy type with ID {id} not found" });

            return Ok(result);
        }

        [HttpPost("policy-types")]
        public async Task<IActionResult> CreatePolicyType([FromBody] reqPolicyTypeDTO dto)
        {
            var result = await _service.AddPolicyTypeAsync(dto);
            return Ok(new { message = result.Message });
        }

        [HttpPut("policy-types/{id}")]
        public async Task<IActionResult> UpdatePolicyType(int id, [FromBody] reqPolicyTypeDTO dto)
        {
            var result = await _service.UpdatePolicyTypeAsync(id, dto);
            if (!result.Success)
                return NotFound(new { error = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpDelete("policy-types/{id}")]
        public async Task<IActionResult> DeletePolicyType(int id)
        {
            var result = await _service.DeletePolicyTypeAsync(id);
            if (!result.Success)
                return NotFound(new { error = result.Message });

            return Ok(new { message = result.Message });
        }

        // ===== POLICIES =====

        [HttpGet("policies")]
        public async Task<IActionResult> GetAllPolicies()
        {
            var result = await _service.GetAllPoliciesAsync();
            return Ok(result);
        }

        [HttpGet("policies/{id}")]
        public async Task<IActionResult> GetPolicyById(int id)
        {
            var result = await _service.GetPolicyByIdAsync(id);
            if (result == null)
                return NotFound(new { error = $"Policy with ID {id} not found" });

            return Ok(result);
        }

        [HttpPost("policies")]
        public async Task<IActionResult> CreatePolicy([FromBody] reqPolicyDTO dto)
        {
            var result = await _service.AddPolicyAsync(dto);
            return Ok(new { message = result.Message });
        }

        [HttpPut("policies/{id}")]
        public async Task<IActionResult> UpdatePolicy(int id, [FromBody] reqPolicyDTO dto)
        {
            var result = await _service.UpdatePolicyAsync(id, dto);
            if (!result.Success)
                return NotFound(new { error = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpDelete("policies/{id}")]
        public async Task<IActionResult> DeletePolicy(int id)
        {
            var result = await _service.DeletePolicyAsync(id);
            if (!result.Success)
                return NotFound(new { error = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpGet("policy-types/{policyTypeId}/policies")]
        public async Task<IActionResult> GetPoliciesByPolicyType(int policyTypeId)
        {
            var result = await _service.GetPoliciesByPolicyTypeAsync(policyTypeId);
            return Ok(result);
        }

        [HttpPut("update-policy-status/{id}")]
        public async Task<IActionResult> UpdatePolicyStatus(int id, bool isActive)
        {
            var result = await _service.UpdatePolicyStatus(id, isActive);
            if (!result.Success)
                return NotFound(new { error = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
