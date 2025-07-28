namespace API.DTO.StartupDTO
{
    public class UpdateStartupDto
    {
        public string? StartupName { get; set; }
        public string? AbbreviationName { get; set; }
        public string? Description { get; set; }
        public string? Vision { get; set; }
        public string? Mission { get; set; }
        public IFormFile? Logo { get; set; }
        public IFormFile? Background { get; set; }
        public string? WebsiteURL { get; set; }
        public string? Email { get; set; }
        public int? StageId { get; set; }
    }
}
