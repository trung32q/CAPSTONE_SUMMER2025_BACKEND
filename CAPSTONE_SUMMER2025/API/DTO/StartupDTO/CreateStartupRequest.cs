namespace API.DTO.StartupDTO
{
    public class CreateStartupRequest
    {
        public string StartupName { get; set; }      
        public string AbbreviationName { get; set; }
        public string Description { get; set; }
        public string Vision { get; set; }
        public string Mission { get; set; }
        public IFormFile? Logo { get; set; }
        public IFormFile? BackgroundUrl { get; set; }
        public string WebsiteUrl { get; set; }
        public string Email { get; set; }
        public int StageId { get; set; }
        public int CreatorAccountId { get; set; }
        public List<int>? InviteAccountIds { get; set; }
        public List<int> CategoryIds { get; set; }
    }
}
