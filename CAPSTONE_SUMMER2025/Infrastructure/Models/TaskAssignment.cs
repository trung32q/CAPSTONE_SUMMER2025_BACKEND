using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class TaskAssignment
    {
        public int TaskAssignmentsId { get; set; }
        public int? TaskId { get; set; }
        public int? AssignedByAccountId { get; set; }
        public int? AssignToAccountId { get; set; }
        public DateTime? AssignAt { get; set; }

        public virtual Account? AssignToAccount { get; set; }
        public virtual Account? AssignedByAccount { get; set; }
        public virtual StartupTask? Task { get; set; }
    }
}
