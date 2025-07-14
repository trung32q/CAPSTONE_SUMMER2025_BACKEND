namespace API.DTO.TaskDTO
{
    public class ColumnWithTasksDto
    {
        public int ColumnStatusId { get; set; }
        public string ColumnName { get; set; }
        public int SortOrder { get; set; }
        public List<TaskDto> Tasks { get; set; }
    }

    public class TaskDto
    {
        public int TaskId { get; set; }
        public string? label { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public List<AssignToDTO> assignto{ get; set; }
    }
    public class AssignToDTO
    {
        public int Id { get; set; }
        public string? Fullname { get; set; }
        public string?  AvatarURL { get; set; }
    }
    
}
