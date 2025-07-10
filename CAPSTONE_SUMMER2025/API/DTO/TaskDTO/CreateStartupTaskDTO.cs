namespace API.DTO.TaskDTO
{
    public class CreateStartupTaskDTO
    {
        public int Milestoneid { get; set; }
        public string Title { get; set; }
        public string? Priority { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int? ColumnnStatusId { get; set; }
        public string? Note { get; set; }
        public int? AssignedByAccountId { get; set; }
        public List<int>? AssignToAccountIds { get; set; }
    }
}
