using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class StartupMember
    {
        public StartupMember()
        {
            MilestoneAssignments = new HashSet<MilestoneAssignment>();
        }

        public int StartupMemberId { get; set; }
        public int? StartupId { get; set; }
        public int? AccountId { get; set; }
        public int? RoleId { get; set; }
        public DateTime? JoinedAt { get; set; }

        public virtual Account? Account { get; set; }
        public virtual RoleInStartup? Role { get; set; }
        public virtual Startup? Startup { get; set; }
        public virtual ICollection<MilestoneAssignment> MilestoneAssignments { get; set; }
    }
}
