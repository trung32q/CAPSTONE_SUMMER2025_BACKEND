namespace API.DTO.ProfileDTO
{
    public class ReqUpdateProfileDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Dob { get; set; }
        public IFormFile? AvatarUrl { get; set; }
        public IFormFile? BackgroundURL { get; set; }
    }
}
