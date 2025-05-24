using API.DTO.PolicyDTO;
using API.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolicyController : ControllerBase
    {
        private readonly IPolicyService _service;

        public PolicyController(IPolicyService service)
        {
            _service = service;
        }

        // === POLICY TYPE ===

        [HttpGet("types")]
        public async Task<ActionResult<List<resPolicyTypeDTO>>> GetAllPolicyTypes()
        {
            var result = await _service.GetAllPolicyTypesAsync();
            return Ok(result);
        }

        [HttpGet("types/{id}")]
        public async Task<ActionResult<resPolicyTypeDTO>> GetPolicyTypeById(int id)
        {
            var result = await _service.GetPolicyTypeByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("types")]
        public async Task<IActionResult> CreatePolicyType([FromBody] reqPolicyTypeDTO dto)
        {
            await _service.AddPolicyTypeAsync(dto);
            return Ok();
        }

        [HttpPut("types/{id}")]
        public async Task<IActionResult> UpdatePolicyType(int id, [FromBody] reqPolicyTypeDTO dto)
        {
            await _service.UpdatePolicyTypeAsync(id, dto);
            return Ok();
        }

        [HttpDelete("types/{id}")]
        public async Task<IActionResult> DeletePolicyType(int id)
        {
            await _service.DeletePolicyTypeAsync(id);
            return Ok();
        }

        // === POLICY ===

        [HttpGet]
        public async Task<ActionResult<List<resPolicyDTO>>> GetAllPolicies()
        {
            var result = await _service.GetAllPoliciesAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<resPolicyDTO>> GetPolicyById(int id)
        {
            var result = await _service.GetPolicyByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePolicy([FromBody] reqPolicyDTO dto)
        {
            await _service.AddPolicyAsync(dto);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePolicy(int id, [FromBody] reqPolicyDTO dto)
        {
            await _service.UpdatePolicyAsync(id, dto);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePolicy(int id)
        {
            await _service.DeletePolicyAsync(id);
            return Ok();
        }

        [HttpGet("by-type/{policyTypeId}")]
        public async Task<ActionResult<List<resPolicyDTO>>> GetPoliciesByPolicyType(int policyTypeId)
        {
            var result = await _service.GetPoliciesByPolicyTypeAsync(policyTypeId);
            return Ok(result);
        }
    }
}
