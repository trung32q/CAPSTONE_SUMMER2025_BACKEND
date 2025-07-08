namespace API.DTO.TaskDTO
{
    public class CommentTaskDto
    {
        public int CommentTaskId { get; set; }
        public int TaskId { get; set; }
        public int AccountId { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreateAt { get; set; }
        public string? FullName { get; set; } 
        public string? AvatarUrl { get; set; }
    }
}
