namespace API.DTO.TaskDTO
{
    public class CreateCommentTaskDto
    {
        public int TaskId { get; set; }
        public int AccountId { get; set; }
        public string Comment { get; set; }
    }
}
