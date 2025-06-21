using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Post
    {
        public Post()
        {
            InversePostShare = new HashSet<Post>();
            PostComments = new HashSet<PostComment>();
            PostHides = new HashSet<PostHide>();
            PostLikes = new HashSet<PostLike>();
            PostMedia = new HashSet<PostMedium>();
            PostReports = new HashSet<PostReport>();
        }

        public int PostId { get; set; }
        public int? AccountId { get; set; }
        public int? StartupId { get; set; }
        public string? Content { get; set; }
        public string? Title { get; set; }
        public DateTime? CreateAt { get; set; }
        public int? PostShareId { get; set; }
        public string? ContentShare { get; set; }
        public DateTime? Schedule { get; set; }

        public virtual Account? Account { get; set; }
        public virtual Post? PostShare { get; set; }
        public virtual Startup? Startup { get; set; }
        public virtual ICollection<Post> InversePostShare { get; set; }
        public virtual ICollection<PostComment> PostComments { get; set; }
        public virtual ICollection<PostHide> PostHides { get; set; }
        public virtual ICollection<PostLike> PostLikes { get; set; }
        public virtual ICollection<PostMedium> PostMedia { get; set; }
        public virtual ICollection<PostReport> PostReports { get; set; }
    }
}
