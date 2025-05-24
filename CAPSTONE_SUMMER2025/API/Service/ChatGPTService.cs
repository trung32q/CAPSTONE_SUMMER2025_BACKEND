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

            // Có media — kiểm tra loại media
            var file = reqPostDTO.MediaFiles.First();
            var contentType = file.ContentType.ToLower();

            if (contentType.StartsWith("image/"))
            {
                return await _chatGPTRepository.CheckPostPolicyWithUploadedImageAsync(reqPostDTO.Content, file, policies);
            }
            else if (contentType.StartsWith("video/"))
            {
                return await _chatGPTRepository.CheckPostPolicyWithVideoAsync(reqPostDTO.Content, file, policies);
            }
            else
            {
                return "Vi phạm: Định dạng media không được hỗ trợ";
            }
        }
    }
}
