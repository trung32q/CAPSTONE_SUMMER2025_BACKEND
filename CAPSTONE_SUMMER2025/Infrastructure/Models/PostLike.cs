using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class PostLike
    {
        public int PostLikeId { get; set; }
        public int? PostId { get; set; }
        public int? AccountId { get; set; }
        public DateTime? LikedAt { get; set; }

        public virtual Account? Account { get; set; }
        public virtual Post? Post { get; set; }
    }
}
