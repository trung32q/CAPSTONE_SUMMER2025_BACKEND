using API.DTO.PolicyDTO;
using API.DTO.PostDTO;
using API.Repositories.Interfaces;
using API.Service.Interface;

namespace API.Service
{
    public class ChatGPTService : IChatGPTService
    {
        private readonly IChatGPTRepository _chatGPTRepository;

        public ChatGPTService(IChatGPTRepository chatGPTRepository)
        {
            _chatGPTRepository = chatGPTRepository;
        }
        public async Task<string> ModeratePostContentAsync(ReqPostDTO reqPostDTO, List<resPolicyDTO> policies)
        {
            // Không có media
            if (reqPostDTO.MediaFiles == null || !reqPostDTO.MediaFiles.Any())
            {
                return await _chatGPTRepository.CheckPostPolicyAsync(reqPostDTO.Content, policies);
            }

            // Có media — kiểm tra từng file
            foreach (var file in reqPostDTO.MediaFiles)
            {
                var contentType = file.ContentType.ToLower();

                if (contentType.StartsWith("image/"))
                {
                    var result = await _chatGPTRepository.CheckPostPolicyWithUploadedImageAsync(reqPostDTO.Content, file, policies);
                    if (result.Contains("Violation"))
                        return result;
                }
                else if (contentType.StartsWith("video/"))
                {
                    var result = await _chatGPTRepository.CheckPostPolicyWithVideoAsync(reqPostDTO.Content, file, policies);
                    if (result.Contains("Violation"))
                        return result;
                }
                else
                {
                    return "Violation: file not supported";
                }
            }

            // Nếu không có vi phạm nào
            return await _chatGPTRepository.CheckPostPolicyAsync(reqPostDTO.Content, policies);
        }

    }
}