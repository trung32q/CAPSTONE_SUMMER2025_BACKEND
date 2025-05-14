using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Invite
    {
        public int InviteId { get; set; }
        public int? SenderAccountId { get; set; }
        public int? ReceiverAccountId { get; set; }
        public int? StartupId { get; set; }
        public int? RoleId { get; set; }
        public DateTime? InviteSentAt { get; set; }
        public string? InviteStatus { get; set; }

        public virtual Account? ReceiverAccount { get; set; }
        public virtual RoleInStartup? Role { get; set; }
        public virtual Account? SenderAccount { get; set; }
        public virtual Startup? Startup { get; set; }
    }
}
