using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Label
    {
        public Label()
        {
            StartupTaskLabels = new HashSet<StartupTaskLabel>();
        }

        public int LabelId { get; set; }
        public string LabelName { get; set; } = null!;
        public string? Color { get; set; }

        public virtual ICollection<StartupTaskLabel> StartupTaskLabels { get; set; }
    }
}
