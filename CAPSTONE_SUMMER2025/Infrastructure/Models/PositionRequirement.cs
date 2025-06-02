using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class PositionRequirement
    {
        public PositionRequirement()
        {
            InternshipPosts = new HashSet<InternshipPost>();
        }

        public int PositionId { get; set; }
        public int? StartupId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Requirement { get; set; }

        public virtual ICollection<InternshipPost> InternshipPosts { get; set; }
    }
}
