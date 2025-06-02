using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class CommentTask
    {
        public int CommentTaskId { get; set; }
        public int? TaskId { get; set; }
        public int? AccountId { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreateAt { get; set; }

        public virtual Account? Account { get; set; }
        public virtual StartupTask? Task { get; set; }
    }
}
