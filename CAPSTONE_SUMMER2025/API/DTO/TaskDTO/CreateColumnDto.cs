namespace API.DTO.TaskDTO
{
    public class CreateColumnDto
    {
        public int MilestoneId { get; set; }
        public string ColumnName { get; set; }
        public string? Description { get; set; }
    }
}
