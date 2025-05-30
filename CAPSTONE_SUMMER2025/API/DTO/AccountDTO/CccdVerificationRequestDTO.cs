namespace API.DTO.AccountDTO
{
    public class CccdVerificationRequestDTO
    {
        public IFormFile CccdFront { get; set; }
        public IFormFile CccdBack { get; set; }
        public IFormFile Selfie { get; set; }

        // Thông tin người dùng nhập
        public string FullName { get; set; }
        public string CccdNumber { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }

        public string IssueDate { get; set; } // Ngày cấp (yyyy-MM-dd)
        public string IssuePlace { get; set; } // Nơi cấp
    }
}
