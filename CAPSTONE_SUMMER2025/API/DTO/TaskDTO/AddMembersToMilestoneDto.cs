namespace API.DTO.TaskDTO
{
    public class AddMembersToMilestoneDto
    {
        public int MilestoneId { get; set; }
        public List<int> MemberIds { get; set; }
    }
}
