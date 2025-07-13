namespace API.DTO.TaskDTO
{
    public class UpdateTaskColumnDto
    {
        public int TaskId { get; set; }
        public int NewColumnStatusId { get; set; }
        public int AccountId { get; set; }
    }
}
