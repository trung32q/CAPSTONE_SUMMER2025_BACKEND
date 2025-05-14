using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class StartupTask
    {
        public StartupTask()
        {
            TaskAssignments = new HashSet<TaskAssignment>();
        }

        public int TaskId { get; set; }
        public int? StartupId { get; set; }
        public string? Title { get; set; }
        public string? Priority { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Progress { get; set; }
        public string? Tags { get; set; }
        public string? Status { get; set; }
        public DateTime? UpdateAt { get; set; }

        public virtual Startup? Startup { get; set; }
        public virtual ICollection<TaskAssignment> TaskAssignments { get; set; }
    }
}
