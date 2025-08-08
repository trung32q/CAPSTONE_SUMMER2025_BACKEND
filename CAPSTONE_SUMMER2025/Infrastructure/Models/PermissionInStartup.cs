using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class PermissionInStartup
    {
        public int PermissionId { get; set; }
        public int RoleId { get; set; }
        public bool CanManagePost { get; set; }
        public bool CanManageCandidate { get; set; }
        public bool CanManageChatRoom { get; set; }
        public bool CanManageMember { get; set; }
        public bool CanManageMilestone { get; set; }
        public bool CanManageStartupChat { get; set; }

        public virtual RoleInStartup Role { get; set; } = null!;
    }
}
