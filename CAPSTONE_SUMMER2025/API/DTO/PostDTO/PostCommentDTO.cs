using Amazon;
using API.DTO.AccountDTO;

namespace API.DTO.PostDTO
{
    public class PostCommentDTO 
    {
        public int PostcommentId { get; set; }
        public int? PostId { get; set; }
        public string? Content { get; set; }
        public DateTime? CommentAt { get; set; }
        public int? ParentCommentId { get; set; }
        public int numChildComment {  get; set; }
        public AccountInforDTOcs AccountInfor { get; set; }

    }
}
