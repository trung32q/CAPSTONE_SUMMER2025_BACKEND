namespace API.DTO.AccountDTO
{
    public class CccdVerificationRequestDTO
    {
        public IFormFile CccdFront { get; set; }
        public IFormFile CccdBack { get; set; }
        public IFormFile Selfie { get; set; }
    }
}
