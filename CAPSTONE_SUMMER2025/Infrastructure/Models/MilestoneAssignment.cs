using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class MilestoneAssignment
    {
        public int MilestoneAssignmentId { get; set; }
        public int? MilestoneId { get; set; }
        public int? MemberId { get; set; }

        public virtual StartupMember? Member { get; set; }
        public virtual Milestone? Milestone { get; set; }
    }
}
