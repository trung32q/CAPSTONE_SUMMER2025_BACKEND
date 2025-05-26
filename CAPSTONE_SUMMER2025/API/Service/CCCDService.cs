using API.DTO.AccountDTO;
using API.Repositories.Interfaces;
using API.Service.Interface;

namespace API.Service
{
    public class CCCDService : ICCCDService
    {
        private readonly IFptAIRepository _fptAIRepository;

        public CCCDService(IFptAIRepository fptAIRepository)
        {
            _fptAIRepository = fptAIRepository;
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
