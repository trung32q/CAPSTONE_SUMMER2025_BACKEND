namespace API.DTO.PostDTO
{
    public class UpdateInternshipPostDTO
    {
        public string Description { get; set; }
        public string Requirement { get; set; }
        public string Benefits { get; set; }
        public string Address { get; set; }
        public string Salary { get; set; } 
        public DateTime Deadline { get; set; }
    }
}
