using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Startup
    {
        public Startup()
        {
            ChatRooms = new HashSet<ChatRoom>();
            InternshipPosts = new HashSet<InternshipPost>();
            InvestmentEvents = new HashSet<InvestmentEvent>();
            Invites = new HashSet<Invite>();
            Milestones = new HashSet<Milestone>();
            Posts = new HashSet<Post>();
            RoleInStartups = new HashSet<RoleInStartup>();
            StartupCategories = new HashSet<StartupCategory>();
            StartupClicks = new HashSet<StartupClick>();
            StartupLicenses = new HashSet<StartupLicense>();
            StartupMembers = new HashSet<StartupMember>();
            StartupPitchings = new HashSet<StartupPitching>();
            Subcribes = new HashSet<Subcribe>();
            UserChatRoomMembers = new HashSet<UserChatRoomMember>();
        }

        public int StartupId { get; set; }
        public string? StartupName { get; set; }
        public string? AbbreviationName { get; set; }
        public string? Description { get; set; }
        public string? Vision { get; set; }
        public string? Mission { get; set; }
        public string? Logo { get; set; }
        public string? BackgroundUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public int? StageId { get; set; }
        public DateTime? CreateAt { get; set; }

        public virtual StartupStage? Stage { get; set; }
        public virtual BusinessModelCanva? BusinessModelCanva { get; set; }
        public virtual ICollection<ChatRoom> ChatRooms { get; set; }
        public virtual ICollection<InternshipPost> InternshipPosts { get; set; }
        public virtual ICollection<InvestmentEvent> InvestmentEvents { get; set; }
        public virtual ICollection<Invite> Invites { get; set; }
        public virtual ICollection<Milestone> Milestones { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<RoleInStartup> RoleInStartups { get; set; }
        public virtual ICollection<StartupCategory> StartupCategories { get; set; }
        public virtual ICollection<StartupClick> StartupClicks { get; set; }
        public virtual ICollection<StartupLicense> StartupLicenses { get; set; }
        public virtual ICollection<StartupMember> StartupMembers { get; set; }
        public virtual ICollection<StartupPitching> StartupPitchings { get; set; }
        public virtual ICollection<Subcribe> Subcribes { get; set; }
        public virtual ICollection<UserChatRoomMember> UserChatRoomMembers { get; set; }
    }
}
