﻿using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class StartupTask
    {
        public StartupTask()
        {
            CommentTasks = new HashSet<CommentTask>();
            TaskAssignments = new HashSet<TaskAssignment>();
            Labels = new HashSet<Label>();
        }

        public int TaskId { get; set; }
        public int? MilestoneId { get; set; }
        public string? Title { get; set; }
        public string? Priority { get; set; }
        public string? Description { get; set; }
        public DateTime? Duedate { get; set; }
        public int? Progress { get; set; }
        public int? ColumnnStatusId { get; set; }
        public string? Note { get; set; }

        public virtual ColumnnStatus? ColumnnStatus { get; set; }
        public virtual Milestone? Milestone { get; set; }
        public virtual ICollection<CommentTask> CommentTasks { get; set; }
        public virtual ICollection<TaskAssignment> TaskAssignments { get; set; }

        public virtual ICollection<Label> Labels { get; set; }
    }
}
