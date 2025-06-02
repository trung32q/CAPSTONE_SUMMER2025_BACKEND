using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class ColumnnStatus
    {
        public ColumnnStatus()
        {
            StartupTasks = new HashSet<StartupTask>();
        }

        public int ColumnnStatusId { get; set; }
        public int? MilestoneId { get; set; }
        public string? ColumnName { get; set; }
        public int? SortOrder { get; set; }
        public string? Description { get; set; }

        public virtual Milestone? Milestone { get; set; }
        public virtual ICollection<StartupTask> StartupTasks { get; set; }
    }
}
