using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Milestone
    {
        public Milestone()
        {
            ColumnnStatuses = new HashSet<ColumnnStatus>();
            MilestoneAssignments = new HashSet<MilestoneAssignment>();
            StartupTasks = new HashSet<StartupTask>();
        }

        public int MilestoneId { get; set; }
        public int? StartupId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }

        public virtual Startup? Startup { get; set; }
        public virtual ICollection<ColumnnStatus> ColumnnStatuses { get; set; }
        public virtual ICollection<MilestoneAssignment> MilestoneAssignments { get; set; }
        public virtual ICollection<StartupTask> StartupTasks { get; set; }
    }
}
