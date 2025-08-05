namespace API.DTO.StartupDTO
{
    public class StartupDetailDTO
    {
        public int StartupId { get; set; }
        public string? StartupName { get; set; }
        public string? AbbreviationName { get; set; }
        public string? Description { get; set; }
        public string? Vision { get; set; }
        public string? Mission { get; set; }
        public string? Logo { get; set; }
        public string? BackgroundURL { get; set; }
        public string? WebsiteURL { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public int? StageId { get; set; }
        public string? StageName {  get; set; }
        public DateTime CreateAt { get; set; }
    }
}
