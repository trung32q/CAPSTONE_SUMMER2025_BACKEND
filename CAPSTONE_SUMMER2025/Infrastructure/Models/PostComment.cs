using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class PostComment
    {
        public PostComment()
        {
            InverseParentComment = new HashSet<PostComment>();
        }

        public int PostcommentId { get; set; }
        public int? PostId { get; set; }
        public int? AccountId { get; set; }
        public int? ParentCommentId { get; set; }
        public string? Content { get; set; }
        public DateTime? CommentAt { get; set; }

        public virtual Account? Account { get; set; }
        public virtual PostComment? ParentComment { get; set; }
        public virtual Post? Post { get; set; }
        public virtual ICollection<PostComment> InverseParentComment { get; set; }
    }
}
