namespace API.DTO.TaskDTO
{
    public class CreateMilestoneDto
    {
        public int StartupId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int  AccountID { get; set; } 
        public List<int>? MemberIds { get; set; } = new();
    }
}
