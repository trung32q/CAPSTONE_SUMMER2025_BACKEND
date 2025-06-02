using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class PostHide
    {
        public int PostHideId { get; set; }
        public int? AccountId { get; set; }
        public int? PostId { get; set; }
        public DateTime? HideAt { get; set; }

        public virtual Account? Account { get; set; }
        public virtual Post? Post { get; set; }
    }
}
