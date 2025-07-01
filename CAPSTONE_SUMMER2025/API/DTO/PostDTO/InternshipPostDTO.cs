namespace API.DTO.PostDTO
{
    public class InternshipPostDTO
    {
        public int InternshipId { get; set; }
        public int StartupId { get; set; }
        public string Position { get; set; }
        public int PositionId {  get; set; }
        public string Description { get; set; }
        public string Requirement { get; set; }
        public string Benefits { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? Deadline { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string Salary { get; set; }
        public string? StartupName { get; set; }
        public string? StartupLogo { get; set; }
    }
}
