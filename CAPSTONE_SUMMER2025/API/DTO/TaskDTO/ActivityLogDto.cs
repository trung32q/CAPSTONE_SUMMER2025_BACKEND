namespace API.DTO.TaskDTO
{
    public class ActivityLogDto
    {
        public int ActivityId { get; set; }
        public int TaskId { get; set; }
        public string ActionType { get; set; }
        public DateTime AtTime { get; set; }
        public string? Content { get; set; }
        public int ByAccountId { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
