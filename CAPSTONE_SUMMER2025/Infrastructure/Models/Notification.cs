using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Notification
    {
        public int NotificationId { get; set; }
        public int? AccountId { get; set; }
        public string? Content { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? SendAt { get; set; }
        public int? SenderId { get; set; }
        public string? NotificationType { get; set; }
        public string? TargetUrl { get; set; }

        public virtual Account? Account { get; set; }
    }
}
