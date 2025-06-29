namespace API.DTO.TaskDTO
{
    public class ResMilestoneDto
    {
        public int MilestoneId { get; set; }
        public int StartupId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public List<MemberInMilestoneDto> Members { get; set; } = new();
    }
    public class MemberInMilestoneDto
    {
        public int MemberId { get; set; }
        public int AccountId { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
