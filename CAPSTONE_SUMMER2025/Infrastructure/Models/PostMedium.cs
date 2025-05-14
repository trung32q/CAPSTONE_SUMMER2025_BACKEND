using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class PostMedium
    {
        public int PostMediaId { get; set; }
        public int? PostId { get; set; }
        public string? MediaUrl { get; set; }
        public int? DisplayOrder { get; set; }
        public DateTime? CreateAt { get; set; }

        public virtual Post? Post { get; set; }
    }
}
