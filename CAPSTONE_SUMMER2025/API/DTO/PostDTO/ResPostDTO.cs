using System;

namespace API.DTO.PostDTO
{
    public class ResPostDTO
    {
        public int PostId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AvatarUrl { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsLiked { get; set; }
    }
} 