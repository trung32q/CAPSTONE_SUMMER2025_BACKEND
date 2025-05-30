using API.DTO.AccountDTO;

namespace API.Service.Interface
{
    public interface ICCCDService
    {
        Task<CccdVerificationResultDTO> VerifyCccdAsync(IFormFile cccdImage, IFormFile selfieImage);
        Task<ExtractedCccdInfoDTO> ExtractCccdInfoAsync(IFormFile front, IFormFile back);
        List<string> CompareWithUserInput(ExtractedCccdInfoDTO extracted, CccdVerificationRequestDTO input);
    }
}
