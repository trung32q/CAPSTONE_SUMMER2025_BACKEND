using API.DTO.AccountDTO;

namespace API.Service.Interface
{
    public interface ICCCDService
    {
        Task<CccdVerificationResultDTO> VerifyCccdAsync(IFormFile cccdImage, IFormFile selfieImage);
       
    }
}
