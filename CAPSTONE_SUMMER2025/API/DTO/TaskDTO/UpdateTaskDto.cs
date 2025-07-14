namespace API.DTO.TaskDTO
{
    public class UpdateTaskDto
    {
        public int TaskId { get; set; }
        public string? Title { get; set; }
        public string? Priority { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int? Progress { get; set; }
        public int? ColumnnStatusId { get; set; }
        public string? Note { get; set; }
        public int AccountId  { get; set; }
        public int? labelcolorID { get; set; }
    }
}
