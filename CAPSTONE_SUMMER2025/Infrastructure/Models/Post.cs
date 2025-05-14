using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Post
    {
        public Post()
        {
            PostComments = new HashSet<PostComment>();
            PostLikes = new HashSet<PostLike>();
            PostMedia = new HashSet<PostMedium>();
            PostReports = new HashSet<PostReport>();
        }

        public int PostId { get; set; }
        public int? AccountId { get; set; }
        public string? Content { get; set; }
        public string? Title { get; set; }
        public DateTime? CreateAt { get; set; }

        public virtual Account? Account { get; set; }
        public virtual ICollection<PostComment> PostComments { get; set; }
        public virtual ICollection<PostLike> PostLikes { get; set; }
        public virtual ICollection<PostMedium> PostMedia { get; set; }
        public virtual ICollection<PostReport> PostReports { get; set; }
    }
}
