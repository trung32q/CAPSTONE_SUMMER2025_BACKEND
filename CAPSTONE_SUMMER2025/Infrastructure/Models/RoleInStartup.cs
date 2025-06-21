using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class RoleInStartup
    {
        public RoleInStartup()
        {
            Invites = new HashSet<Invite>();
            PermissionInStartups = new HashSet<PermissionInStartup>();
            StartupMembers = new HashSet<StartupMember>();
        }

        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public int? StartupId { get; set; }

        public virtual Startup? Startup { get; set; }
        public virtual ICollection<Invite> Invites { get; set; }
        public virtual ICollection<PermissionInStartup> PermissionInStartups { get; set; }
        public virtual ICollection<StartupMember> StartupMembers { get; set; }
    }
}
