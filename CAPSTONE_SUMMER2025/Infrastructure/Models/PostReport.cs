using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class PostReport
    {
        public int ReportId { get; set; }
        public int? AccountId { get; set; }
        public int? PostId { get; set; }
        public int? ReasonId { get; set; }
        public string? Status { get; set; }
        public DateTime? CreateAt { get; set; }

        public virtual Account? Account { get; set; }
        public virtual Post? Post { get; set; }
        public virtual ReportReason? Reason { get; set; }
    }
}
