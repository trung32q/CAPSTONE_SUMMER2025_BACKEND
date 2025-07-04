namespace API.DTO.TaskDTO
{
    public class TaskAssignmentDto
    {
        public int TaskId { get; set; }
        public int AssignedByAccountId { get; set; }
        public int AssignToAccountId { get; set; }
    }
}
