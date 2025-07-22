using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Account
    {
        public Account()
        {
            AccountBlockBlockedAccounts = new HashSet<AccountBlock>();
            AccountBlockBlockerAccounts = new HashSet<AccountBlock>();
            CandidateCvs = new HashSet<CandidateCv>();
            ChatMessages = new HashSet<ChatMessage>();
            ChatRoomMembers = new HashSet<ChatRoomMember>();
            CommentTasks = new HashSet<CommentTask>();
            FollowFollowerAccounts = new HashSet<Follow>();
            FollowFollowingAccounts = new HashSet<Follow>();
            InvestmentEventTickets = new HashSet<InvestmentEventTicket>();
            InvestmentEventsRequests = new HashSet<InvestmentEventsRequest>();
            InviteReceiverAccounts = new HashSet<Invite>();
            InviteSenderAccounts = new HashSet<Invite>();
            Notifications = new HashSet<Notification>();
            PostComments = new HashSet<PostComment>();
            PostHides = new HashSet<PostHide>();
            PostLikes = new HashSet<PostLike>();
            PostReports = new HashSet<PostReport>();
            Posts = new HashSet<Post>();
            StartupMembers = new HashSet<StartupMember>();
            Subcribes = new HashSet<Subcribe>();
            TaskActivityLogs = new HashSet<TaskActivityLog>();
            TaskAssignmentAssignToAccounts = new HashSet<TaskAssignment>();
            TaskAssignmentAssignedByAccounts = new HashSet<TaskAssignment>();
            UserChatRoomMembers = new HashSet<UserChatRoomMember>();
            UserOtps = new HashSet<UserOtp>();
        }

        public int AccountId { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public int? Flag { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual AccountProfile? AccountProfile { get; set; }
        public virtual Bio? Bio { get; set; }
        public virtual ICollection<AccountBlock> AccountBlockBlockedAccounts { get; set; }
        public virtual ICollection<AccountBlock> AccountBlockBlockerAccounts { get; set; }
        public virtual ICollection<CandidateCv> CandidateCvs { get; set; }
        public virtual ICollection<ChatMessage> ChatMessages { get; set; }
        public virtual ICollection<ChatRoomMember> ChatRoomMembers { get; set; }
        public virtual ICollection<CommentTask> CommentTasks { get; set; }
        public virtual ICollection<Follow> FollowFollowerAccounts { get; set; }
        public virtual ICollection<Follow> FollowFollowingAccounts { get; set; }
        public virtual ICollection<InvestmentEventTicket> InvestmentEventTickets { get; set; }
        public virtual ICollection<InvestmentEventsRequest> InvestmentEventsRequests { get; set; }
        public virtual ICollection<Invite> InviteReceiverAccounts { get; set; }
        public virtual ICollection<Invite> InviteSenderAccounts { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<PostComment> PostComments { get; set; }
        public virtual ICollection<PostHide> PostHides { get; set; }
        public virtual ICollection<PostLike> PostLikes { get; set; }
        public virtual ICollection<PostReport> PostReports { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<StartupMember> StartupMembers { get; set; }
        public virtual ICollection<Subcribe> Subcribes { get; set; }
        public virtual ICollection<TaskActivityLog> TaskActivityLogs { get; set; }
        public virtual ICollection<TaskAssignment> TaskAssignmentAssignToAccounts { get; set; }
        public virtual ICollection<TaskAssignment> TaskAssignmentAssignedByAccounts { get; set; }
        public virtual ICollection<UserChatRoomMember> UserChatRoomMembers { get; set; }
        public virtual ICollection<UserOtp> UserOtps { get; set; }
    }
}
