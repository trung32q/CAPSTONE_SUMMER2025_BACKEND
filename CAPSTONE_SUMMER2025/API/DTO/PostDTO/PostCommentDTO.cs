using Amazon;

namespace API.DTO.PostDTO
{
    public class PostCommentDTO 
    {
        public int PostcommentId { get; set; }
        public int? PostId { get; set; }
        public int? AccountId { get; set; }
        public string? Content { get; set; }
        public DateTime? CommentAt { get; set; }
        public int? ParentCommentId { get; set; }
        public int numChildComment {  get; set; }

    }
}
