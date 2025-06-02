using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Label
    {
        public Label()
        {
            Tasks = new HashSet<StartupTask>();
        }

        public int LabelId { get; set; }
        public int? MilestoneId { get; set; }
        public string LabelName { get; set; } = null!;
        public string? Color { get; set; }

        public virtual Milestone? Milestone { get; set; }

        public virtual ICollection<StartupTask> Tasks { get; set; }
    }
}
