using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class ReportReason
    {
        public ReportReason()
        {
            PostReports = new HashSet<PostReport>();
        }

        public int ReasonId { get; set; }
        public string? Reason { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<PostReport> PostReports { get; set; }
    }
}
