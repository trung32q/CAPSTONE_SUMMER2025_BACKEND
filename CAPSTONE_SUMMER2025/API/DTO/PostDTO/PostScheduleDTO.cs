namespace API.DTO.PostDTO
{
    public class PostScheduleDTO
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? Schedule { get; set; }
    }
}
