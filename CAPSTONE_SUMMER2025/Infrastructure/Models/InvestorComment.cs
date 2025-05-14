using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class InvestorComment
    {
        public int InvestorCommentId { get; set; }
        public int? AccountId { get; set; }
        public int? StartupId { get; set; }
        public string? Content { get; set; }
        public DateTime? CommentAt { get; set; }

        public virtual Account? Account { get; set; }
        public virtual Startup? Startup { get; set; }
    }
}
