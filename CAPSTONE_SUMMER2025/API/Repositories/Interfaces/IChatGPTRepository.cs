using API.DTO.PolicyDTO;
using API.DTO.PostDTO;
using API.DTO.StartupDTO;
using Infrastructure.Models;

namespace API.Repositories.Interfaces
{
    public interface IChatGPTRepository
    {
        Task<string> CheckPostPolicyAsync(string postContent, List<resPolicyDTO> policies);
        Task<string> CheckPostPolicyWithUploadedImageAsync(string contentToCheck, IFormFile imageFile, List<resPolicyDTO> policies);
        Task<string> CheckPostPolicyWithVideoAsync(string postContent, IFormFile videoFile, List<resPolicyDTO> policies);
        Task<CVRequirementEvaluationResultDto> EvaluateCVAgainstPositionAsync(string cvText, string positionDescription, string positionRequirement);
    }
}
