namespace API.DTO.PostDTO
{
    public class resPostDTO
    {
        public int PostId { get; set; }
        public int? AccountId { get; set; }
        public string? Content { get; set; }
        public List<PostMediaDTO> PostMedia { get; set; }
        public string? Title { get; set; }
        public DateTime? CreateAt { get; set; }
    }
}
