namespace API.DTO.PostDTO
{
    public class InternshipPostDTOcs
    {
        public int InternshipID { get; set; }
        public int StartupID { get; set; }
        public int PositionID { get; set; }
        public string? Description { get; set; }
        public string? Requirement { get; set; }
        public string? Benefits { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Status { get; set; }
        public string? Address { get; set; }
        public decimal? Salary { get; set; }

        // Thông tin startup
        public string? StartupName { get; set; }
        public string? StartupLogo { get; set; }
    }
}
