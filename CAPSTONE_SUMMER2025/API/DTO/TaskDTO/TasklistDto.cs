namespace API.DTO.TaskDTO
{
    public class TasklistDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string? Priority { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int? Progress { get; set; }
        public string ColumnStatus { get; set; }
        public string? Note { get; set; }
        public string CreatedBy { get; set; }
       public List<AssignToDTO> AsignTo { get; set; }
        
    }
}
