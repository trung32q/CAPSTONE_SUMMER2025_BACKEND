using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Startup
    {
        public Startup()
        {
            ChatRooms = new HashSet<ChatRoom>();
            FinancePlans = new HashSet<FinancePlan>();
            InvestmentEvents = new HashSet<InvestmentEvent>();
            InvestorComments = new HashSet<InvestorComment>();
            Invites = new HashSet<Invite>();
            RoleInStartups = new HashSet<RoleInStartup>();
            StartupCategories = new HashSet<StartupCategory>();
            StartupImages = new HashSet<StartupImage>();
            StartupLicenses = new HashSet<StartupLicense>();
            StartupMembers = new HashSet<StartupMember>();
            StartupTasks = new HashSet<StartupTask>();
        }

        public int StartupId { get; set; }
        public string? StartupName { get; set; }
        public string? AbbreviationName { get; set; }
        public string? Description { get; set; }
        public string? Vision { get; set; }
        public string? Mission { get; set; }
        public string? Logo { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public int? StageId { get; set; }
        public DateTime? CreateAt { get; set; }

        public virtual StartupStage? Stage { get; set; }
        public virtual BusinessModelCanva? BusinessModelCanva { get; set; }
        public virtual ICollection<ChatRoom> ChatRooms { get; set; }
        public virtual ICollection<FinancePlan> FinancePlans { get; set; }
        public virtual ICollection<InvestmentEvent> InvestmentEvents { get; set; }
        public virtual ICollection<InvestorComment> InvestorComments { get; set; }
        public virtual ICollection<Invite> Invites { get; set; }
        public virtual ICollection<RoleInStartup> RoleInStartups { get; set; }
        public virtual ICollection<StartupCategory> StartupCategories { get; set; }
        public virtual ICollection<StartupImage> StartupImages { get; set; }
        public virtual ICollection<StartupLicense> StartupLicenses { get; set; }
        public virtual ICollection<StartupMember> StartupMembers { get; set; }
        public virtual ICollection<StartupTask> StartupTasks { get; set; }
    }
}
