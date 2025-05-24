using API.DTO.PolicyDTO;
using API.DTO.PostDTO;

namespace API.Service.Interface
{
    public interface IChatGPTService
    {
        Task<string> ModeratePostContentAsync(ReqPostDTO reqPostDTO, List<resPolicyDTO> policies);
    }
}
