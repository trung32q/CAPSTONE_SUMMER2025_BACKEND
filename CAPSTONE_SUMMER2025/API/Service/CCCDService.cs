using API.DTO.AccountDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Service.Interface;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace API.Service
{
    public class CCCDService : ICCCDService
    {
        private readonly IFptAIRepository _fptAIRepository;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CCCDService(HttpClient httpClient, IConfiguration configuration, IFptAIRepository fptAIRepository)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _fptAIRepository = fptAIRepository;
        }

      

        private async Task<string> ConvertToBase64(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return Convert.ToBase64String(ms.ToArray());
        }

        public async Task<CccdVerificationResultDTO> VerifyCccdAsync(IFormFile cccdImage, IFormFile selfieImage)
        {
            var faceMatch = await _fptAIRepository.VerifyFaceAsync(cccdImage, selfieImage);

            return new CccdVerificationResultDTO
            {
                IsMatch = faceMatch,
                ExtractedInfo = faceMatch ? "Khuôn mặt trùng khớp" : "Khuôn mặt không trùng khớp"
            };
        }

      
    }
}
