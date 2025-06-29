namespace API.DTO.TaskDTO
{
    public class ColumnStatusDto
    {
        public int ColumnStatusId { get; set; }
        public string ColumnName { get; set; }
        public int SortOrder { get; set; }
        public string? Description { get; set; }
    }
}
