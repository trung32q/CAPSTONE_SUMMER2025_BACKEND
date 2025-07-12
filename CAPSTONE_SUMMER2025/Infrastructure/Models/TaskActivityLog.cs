using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class TaskActivityLog
    {
        public int ActivityId { get; set; }
        public int TaskId { get; set; }
        public string ActionType { get; set; } = null!;
        public int ByAccountId { get; set; }
        public DateTime AtTime { get; set; }
        public string? Content { get; set; }

        public virtual Account ByAccount { get; set; } = null!;
        public virtual StartupTask Task { get; set; } = null!;
    }
}
