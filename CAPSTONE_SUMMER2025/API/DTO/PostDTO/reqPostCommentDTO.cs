namespace API.DTO.PostDTO
{
    public class reqPostCommentDTO
    {
        public int AccountId { get; set; }
        public int PostId { get; set; }
        public string Content { get; set; }
        public int? ParentCommentId { get; set; }
    }
}
