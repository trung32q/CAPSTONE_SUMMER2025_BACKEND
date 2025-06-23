namespace API.DTO.PostDTO
{
    public class CreateInternshipPostDTO
    {
        public int Startup_ID { get; set; }
        public int Position_ID { get; set; }
        public string Description { get; set; }
        public string Requirement { get; set; }
        public string Benefits { get; set; }
        public DateTime Deadline { get; set; }
        public string? Address { get; set; }
        public string? Salary { get; set; }
    }
}
