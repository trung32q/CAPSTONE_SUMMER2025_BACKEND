using Amazon;

namespace API.DTO.PostDTO
{
    public class PostCommentDTO 
    {
        public int? PostId { get; set; }
        public int? AccountId { get; set; }
        public string? Content { get; set; }
        public DateTime? CommentAt { get; set; }
    }
}
