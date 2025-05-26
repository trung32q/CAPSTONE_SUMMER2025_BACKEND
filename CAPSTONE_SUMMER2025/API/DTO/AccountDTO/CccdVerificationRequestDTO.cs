namespace API.DTO.AccountDTO
{
    public class CccdVerificationRequestDTO
    {
        public IFormFile Cccd { get; set; }    // Ảnh CCCD mặt trước
        public IFormFile Selfie { get; set; }  // Ảnh selfie người dùng
    }
}
