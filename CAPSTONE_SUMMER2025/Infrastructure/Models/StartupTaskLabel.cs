using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class StartupTaskLabel
    {
        public int TaskId { get; set; }
        public int LabelId { get; set; }
        public int StartupTaskLabelId { get; set; }

        public virtual Label Label { get; set; } = null!;
        public virtual StartupTask Task { get; set; } = null!;
    }
}
