namespace API.DTO.AccountDTO
{
    public class ResAccountInfoDTO
    {
        public int AccountId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string Gender { get; set; }
        public DateTime? Dob { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string AvatarUrl { get; set; }
        public string BackgroundUrl { get; set; }
        public string IntroTitle { get; set; }
        public string Position { get; set; }
        public string Workplace { get; set; }
        public string FacebookUrl { get; set; }
        public string LinkedinUrl { get; set; }
        public string GithubUrl { get; set; }
        public string PortfolioUrl { get; set; }
        public string Country { get; set; }
        public int PostCount { get; set; }
        public int FollowingCount { get; set; }
        public int FollowerCount { get; set; }
    }
}
