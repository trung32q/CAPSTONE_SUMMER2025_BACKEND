using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Models
{
    public partial class CAPSTONE_SUMMER2025Context : DbContext
    {
        public CAPSTONE_SUMMER2025Context()
        {
        }

        public CAPSTONE_SUMMER2025Context(DbContextOptions<CAPSTONE_SUMMER2025Context> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<AccountBlock> AccountBlocks { get; set; } = null!;
        public virtual DbSet<AccountProfile> AccountProfiles { get; set; } = null!;
        public virtual DbSet<Bio> Bios { get; set; } = null!;
        public virtual DbSet<BusinessModelCanva> BusinessModelCanvas { get; set; } = null!;
        public virtual DbSet<CandidateCv> CandidateCvs { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<ChatMessage> ChatMessages { get; set; } = null!;
        public virtual DbSet<ChatRoom> ChatRooms { get; set; } = null!;
        public virtual DbSet<ChatRoomMember> ChatRoomMembers { get; set; } = null!;
        public virtual DbSet<ColumnnStatus> ColumnnStatuses { get; set; } = null!;
        public virtual DbSet<CommentTask> CommentTasks { get; set; } = null!;
        public virtual DbSet<CvrequirementEvaluation> CvrequirementEvaluations { get; set; } = null!;
        public virtual DbSet<Follow> Follows { get; set; } = null!;
        public virtual DbSet<InternshipPost> InternshipPosts { get; set; } = null!;
        public virtual DbSet<InvestmentEvent> InvestmentEvents { get; set; } = null!;
        public virtual DbSet<InvestmentEventTicket> InvestmentEventTickets { get; set; } = null!;
        public virtual DbSet<InvestmentEventsRequest> InvestmentEventsRequests { get; set; } = null!;
        public virtual DbSet<Invite> Invites { get; set; } = null!;
        public virtual DbSet<Label> Labels { get; set; } = null!;
        public virtual DbSet<Milestone> Milestones { get; set; } = null!;
        public virtual DbSet<MilestoneAssignment> MilestoneAssignments { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<PermissionInStartup> PermissionInStartups { get; set; } = null!;
        public virtual DbSet<Policy> Policies { get; set; } = null!;
        public virtual DbSet<PolicyType> PolicyTypes { get; set; } = null!;
        public virtual DbSet<PositionRequirement> PositionRequirements { get; set; } = null!;
        public virtual DbSet<Post> Posts { get; set; } = null!;
        public virtual DbSet<PostComment> PostComments { get; set; } = null!;
        public virtual DbSet<PostHide> PostHides { get; set; } = null!;
        public virtual DbSet<PostLike> PostLikes { get; set; } = null!;
        public virtual DbSet<PostMedium> PostMedia { get; set; } = null!;
        public virtual DbSet<PostReport> PostReports { get; set; } = null!;
        public virtual DbSet<ReportReason> ReportReasons { get; set; } = null!;
        public virtual DbSet<RoleInStartup> RoleInStartups { get; set; } = null!;
        public virtual DbSet<Startup> Startups { get; set; } = null!;
        public virtual DbSet<StartupCategory> StartupCategories { get; set; } = null!;
        public virtual DbSet<StartupClick> StartupClicks { get; set; } = null!;
        public virtual DbSet<StartupLicense> StartupLicenses { get; set; } = null!;
        public virtual DbSet<StartupMember> StartupMembers { get; set; } = null!;
        public virtual DbSet<StartupPitching> StartupPitchings { get; set; } = null!;
        public virtual DbSet<StartupStage> StartupStages { get; set; } = null!;
        public virtual DbSet<StartupTask> StartupTasks { get; set; } = null!;
        public virtual DbSet<StartupTaskLabel> StartupTaskLabels { get; set; } = null!;
        public virtual DbSet<Subcribe> Subcribes { get; set; } = null!;
        public virtual DbSet<TaskActivityLog> TaskActivityLogs { get; set; } = null!;
        public virtual DbSet<TaskAssignment> TaskAssignments { get; set; } = null!;
        public virtual DbSet<UserCallSession> UserCallSessions { get; set; } = null!;
        public virtual DbSet<UserChatRoom> UserChatRooms { get; set; } = null!;
        public virtual DbSet<UserChatRoomMember> UserChatRoomMembers { get; set; } = null!;
        public virtual DbSet<UserMessage> UserMessages { get; set; } = null!;
        public virtual DbSet<UserOtp> UserOtps { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            optionsBuilder.UseSqlServer(config.GetConnectionString("DBContext"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");

                entity.HasIndex(e => e.Email, "UQ__Account__A9D10534F99CE875")
                    .IsUnique();

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Flag).HasColumnName("flag");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RefreshToken).IsUnicode(false);

                entity.Property(e => e.RefreshTokenExpiry).HasColumnType("datetime");

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AccountBlock>(entity =>
            {
                entity.HasKey(e => e.BlockId)
                    .HasName("PK__AccountB__A848958614581D83");

                entity.ToTable("AccountBlock");

                entity.HasIndex(e => new { e.BlockerAccountId, e.BlockedAccountId }, "UC_AccountBlock")
                    .IsUnique();

                entity.Property(e => e.BlockId).HasColumnName("Block_ID");

                entity.Property(e => e.BlockedAccountId).HasColumnName("Blocked_Account_ID");

                entity.Property(e => e.BlockedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.BlockerAccountId).HasColumnName("Blocker_Account_ID");

                entity.HasOne(d => d.BlockedAccount)
                    .WithMany(p => p.AccountBlockBlockedAccounts)
                    .HasForeignKey(d => d.BlockedAccountId)
                    .HasConstraintName("FK__AccountBl__Block__489AC854");

                entity.HasOne(d => d.BlockerAccount)
                    .WithMany(p => p.AccountBlockBlockerAccounts)
                    .HasForeignKey(d => d.BlockerAccountId)
                    .HasConstraintName("FK__AccountBl__Block__47A6A41B");
            });

            modelBuilder.Entity<AccountProfile>(entity =>
            {
                entity.ToTable("AccountProfile");

                entity.HasIndex(e => e.AccountId, "UQ__AccountP__B19E45C85FFEC27E")
                    .IsUnique();

                entity.Property(e => e.AccountProfileId).HasColumnName("AccountProfile_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.Address).HasMaxLength(200);

                entity.Property(e => e.AvatarUrl)
                    .HasMaxLength(500)
                    .HasColumnName("AvatarURL");

                entity.Property(e => e.BackgroundUrl)
                    .HasMaxLength(500)
                    .HasColumnName("backgroundURL");

                entity.Property(e => e.Dob)
                    .HasColumnType("date")
                    .HasColumnName("DOB");

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.Gender).HasMaxLength(10);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.HasOne(d => d.Account)
                    .WithOne(p => p.AccountProfile)
                    .HasForeignKey<AccountProfile>(d => d.AccountId)
                    .HasConstraintName("FK__AccountPr__Accou__2D27B809");
            });

            modelBuilder.Entity<Bio>(entity =>
            {
                entity.ToTable("BIO");

                entity.HasIndex(e => e.AccountId, "UQ__BIO__B19E45C828528E94")
                    .IsUnique();

                entity.Property(e => e.BioId).HasColumnName("Bio_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.Country).HasMaxLength(100);

                entity.Property(e => e.FacebookUrl)
                    .HasMaxLength(255)
                    .HasColumnName("FacebookURL");

                entity.Property(e => e.GithubUrl)
                    .HasMaxLength(255)
                    .HasColumnName("GithubURL");

                entity.Property(e => e.IntroTitle).HasMaxLength(255);

                entity.Property(e => e.IsPublicProfile).HasDefaultValueSql("((1))");

                entity.Property(e => e.LinkedinUrl)
                    .HasMaxLength(255)
                    .HasColumnName("LinkedinURL");

                entity.Property(e => e.PersonalWebsiteUrl)
                    .HasMaxLength(255)
                    .HasColumnName("PersonalWebsiteURL");

                entity.Property(e => e.PortfolioUrl)
                    .HasMaxLength(255)
                    .HasColumnName("PortfolioURL");

                entity.Property(e => e.Position).HasMaxLength(255);

                entity.Property(e => e.Workplace).HasMaxLength(255);

                entity.HasOne(d => d.Account)
                    .WithOne(p => p.Bio)
                    .HasForeignKey<Bio>(d => d.AccountId)
                    .HasConstraintName("FK__BIO__Account_ID__31EC6D26");
            });

            modelBuilder.Entity<BusinessModelCanva>(entity =>
            {
                entity.HasKey(e => e.BmcId)
                    .HasName("PK__Business__806D52F655948A67");

                entity.HasIndex(e => e.StartupId, "UQ__Business__BB46C8C087A3C0A9")
                    .IsUnique();

                entity.Property(e => e.BmcId).HasColumnName("BMC_ID");

                entity.Property(e => e.CustomerSegments).HasColumnName("Customer_Segments");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.HasOne(d => d.Startup)
                    .WithOne(p => p.BusinessModelCanva)
                    .HasForeignKey<BusinessModelCanva>(d => d.StartupId)
                    .HasConstraintName("FK__BusinessM__Start__6477ECF3");
            });

            modelBuilder.Entity<CandidateCv>(entity =>
            {
                entity.ToTable("CandidateCV");

                entity.Property(e => e.CandidateCvId).HasColumnName("CandidateCV_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Cvurl).HasColumnName("CVURL");

                entity.Property(e => e.InternshipId).HasColumnName("Internship_ID");

                entity.Property(e => e.Status).HasMaxLength(30);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.CandidateCvs)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__Candidate__Accou__1AD3FDA4");

                entity.HasOne(d => d.Internship)
                    .WithMany(p => p.CandidateCvs)
                    .HasForeignKey(d => d.InternshipId)
                    .HasConstraintName("FK__Candidate__Inter__1BC821DD");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.HasIndex(e => e.CategoryName, "UQ__Category__B35EB419EA70543B")
                    .IsUnique();

                entity.Property(e => e.CategoryId).HasColumnName("Category_ID");

                entity.Property(e => e.CategoryName)
                    .HasMaxLength(100)
                    .HasColumnName("Category_Name");
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.ToTable("ChatMessage");

                entity.Property(e => e.ChatMessageId).HasColumnName("ChatMessage_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.ChatRoomId).HasColumnName("ChatRoom_ID");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.SentAt).HasColumnType("datetime");

                entity.Property(e => e.TypeMessage).HasMaxLength(100);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__ChatMessa__Accou__540C7B00");

                entity.HasOne(d => d.ChatRoom)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.ChatRoomId)
                    .HasConstraintName("FK__ChatMessa__ChatR__531856C7");
            });

            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.ToTable("ChatRoom");

                entity.Property(e => e.ChatRoomId).HasColumnName("ChatRoom_ID");

                entity.Property(e => e.RoomName).HasMaxLength(100);

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.ChatRooms)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__ChatRoom__Startu__4B7734FF");
            });

            modelBuilder.Entity<ChatRoomMember>(entity =>
            {
                entity.HasKey(e => e.ChatGroupMembersId)
                    .HasName("PK__ChatRoom__AFC056A7097FF714");

                entity.HasIndex(e => new { e.ChatRoomId, e.AccountId }, "UC_ChatRoomMember")
                    .IsUnique();

                entity.Property(e => e.ChatGroupMembersId).HasColumnName("ChatGroupMembers_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.ChatRoomId).HasColumnName("ChatRoom_ID");

                entity.Property(e => e.JoinedAt).HasColumnType("datetime");

                entity.Property(e => e.MemberTitle).HasMaxLength(100);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.ChatRoomMembers)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__ChatRoomM__Accou__503BEA1C");

                entity.HasOne(d => d.ChatRoom)
                    .WithMany(p => p.ChatRoomMembers)
                    .HasForeignKey(d => d.ChatRoomId)
                    .HasConstraintName("FK__ChatRoomM__ChatR__4F47C5E3");
            });

            modelBuilder.Entity<ColumnnStatus>(entity =>
            {
                entity.ToTable("ColumnnStatus");

                entity.Property(e => e.ColumnnStatusId).HasColumnName("ColumnnStatus_ID");

                entity.Property(e => e.MilestoneId).HasColumnName("Milestone_ID");

                entity.HasOne(d => d.Milestone)
                    .WithMany(p => p.ColumnnStatuses)
                    .HasForeignKey(d => d.MilestoneId)
                    .HasConstraintName("FK__ColumnnSt__Miles__6754599E");
            });

            modelBuilder.Entity<CommentTask>(entity =>
            {
                entity.ToTable("CommentTask");

                entity.Property(e => e.CommentTaskId).HasColumnName("CommentTask_ID");

                entity.Property(e => e.AccountId).HasColumnName("AccountID");

                entity.Property(e => e.Comment).HasColumnName("comment");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TaskId).HasColumnName("Task_ID");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.CommentTasks)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__CommentTa__Accou__7C4F7684");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.CommentTasks)
                    .HasForeignKey(d => d.TaskId)
                    .HasConstraintName("FK__CommentTa__Task___7B5B524B");
            });

            modelBuilder.Entity<CvrequirementEvaluation>(entity =>
            {
                entity.HasKey(e => e.EvaluationId)
                    .HasName("PK__CVRequir__7EF3566EEB3B1856");

                entity.ToTable("CVRequirementEvaluation");

                entity.Property(e => e.EvaluationId).HasColumnName("Evaluation_ID");

                entity.Property(e => e.CandidateCvId).HasColumnName("CandidateCV_ID");

                entity.Property(e => e.EvaluationExperience)
                    .HasMaxLength(2000)
                    .HasColumnName("Evaluation_Experience");

                entity.Property(e => e.EvaluationOverallSummary)
                    .HasMaxLength(2000)
                    .HasColumnName("Evaluation_OverallSummary");

                entity.Property(e => e.EvaluationSoftSkills)
                    .HasMaxLength(2000)
                    .HasColumnName("Evaluation_SoftSkills");

                entity.Property(e => e.EvaluationTechSkills)
                    .HasMaxLength(2000)
                    .HasColumnName("Evaluation_TechSkills");

                entity.Property(e => e.InternshipId).HasColumnName("Internship_ID");

                entity.HasOne(d => d.CandidateCv)
                    .WithMany(p => p.CvrequirementEvaluations)
                    .HasForeignKey(d => d.CandidateCvId)
                    .HasConstraintName("FK_CVRequirementEvaluation_CandidateCV");

                entity.HasOne(d => d.Internship)
                    .WithMany(p => p.CvrequirementEvaluations)
                    .HasForeignKey(d => d.InternshipId)
                    .HasConstraintName("FK_CVRequirementEvaluation_InternshipPost");
            });

            modelBuilder.Entity<Follow>(entity =>
            {
                entity.ToTable("Follow");

                entity.HasIndex(e => new { e.FollowerAccountId, e.FollowingAccountId }, "UC_Follow")
                    .IsUnique();

                entity.Property(e => e.FollowId).HasColumnName("Follow_ID");

                entity.Property(e => e.FollowDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FollowerAccountId).HasColumnName("Follower_Account_ID");

                entity.Property(e => e.FollowingAccountId).HasColumnName("Following_Account_ID");

                entity.HasOne(d => d.FollowerAccount)
                    .WithMany(p => p.FollowFollowerAccounts)
                    .HasForeignKey(d => d.FollowerAccountId)
                    .HasConstraintName("FK__Follow__Follower__0E6E26BF");

                entity.HasOne(d => d.FollowingAccount)
                    .WithMany(p => p.FollowFollowingAccounts)
                    .HasForeignKey(d => d.FollowingAccountId)
                    .HasConstraintName("FK__Follow__Followin__0F624AF8");
            });

            modelBuilder.Entity<InternshipPost>(entity =>
            {
                entity.HasKey(e => e.InternshipId)
                    .HasName("PK__Internsh__A16168F7AE998B3C");

                entity.ToTable("InternshipPost");

                entity.Property(e => e.InternshipId).HasColumnName("Internship_ID");

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Deadline).HasColumnType("datetime");

                entity.Property(e => e.PositionId).HasColumnName("Position_ID");

                entity.Property(e => e.Salary).HasMaxLength(255);

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(d => d.Position)
                    .WithMany(p => p.InternshipPosts)
                    .HasForeignKey(d => d.PositionId)
                    .HasConstraintName("FK__Internshi__Posit__160F4887");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.InternshipPosts)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__Internshi__Start__17036CC0");
            });

            modelBuilder.Entity<InvestmentEvent>(entity =>
            {
                entity.HasKey(e => e.EventId)
                    .HasName("PK__Investme__FD6BEFE450626E2D");

                entity.Property(e => e.EventId).HasColumnName("Event_ID");

                entity.Property(e => e.Schedule).HasColumnType("datetime");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.InvestmentEvents)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__Investmen__Start__7F2BE32F");
            });

            modelBuilder.Entity<InvestmentEventTicket>(entity =>
            {
                entity.HasKey(e => e.TicketId)
                    .HasName("PK__Investme__ED7260D997A1237D");

                entity.ToTable("InvestmentEventTicket");

                entity.HasIndex(e => new { e.EventId, e.AccountId }, "UC_EventTicket")
                    .IsUnique();

                entity.Property(e => e.TicketId).HasColumnName("Ticket_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.EventId).HasColumnName("Event_ID");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.InvestmentEventTickets)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__Investmen__Accou__03F0984C");

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.InvestmentEventTickets)
                    .HasForeignKey(d => d.EventId)
                    .HasConstraintName("FK__Investmen__Event__02FC7413");
            });

            modelBuilder.Entity<InvestmentEventsRequest>(entity =>
            {
                entity.HasKey(e => e.RequestId)
                    .HasName("PK__Investme__E9C5B29326248B28");

                entity.ToTable("InvestmentEventsRequest");

                entity.HasIndex(e => new { e.EventId, e.AccountId }, "UC_EventRequest")
                    .IsUnique();

                entity.Property(e => e.RequestId).HasColumnName("Request_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.EventId).HasColumnName("Event_ID");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.InvestmentEventsRequests)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__Investmen__Accou__09A971A2");

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.InvestmentEventsRequests)
                    .HasForeignKey(d => d.EventId)
                    .HasConstraintName("FK__Investmen__Event__08B54D69");
            });

            modelBuilder.Entity<Invite>(entity =>
            {
                entity.ToTable("Invite");

                entity.Property(e => e.InviteId).HasColumnName("Invite_ID");

                entity.Property(e => e.InviteSentAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.InviteStatus).HasMaxLength(20);

                entity.Property(e => e.ReceiverAccountId).HasColumnName("Receiver_Account_ID");

                entity.Property(e => e.RoleId).HasColumnName("Role_ID");

                entity.Property(e => e.SenderAccountId).HasColumnName("Sender_Account_ID");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.HasOne(d => d.ReceiverAccount)
                    .WithMany(p => p.InviteReceiverAccounts)
                    .HasForeignKey(d => d.ReceiverAccountId)
                    .HasConstraintName("FK__Invite__Receiver__4E88ABD4");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Invites)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK__Invite__Role_ID__5070F446");

                entity.HasOne(d => d.SenderAccount)
                    .WithMany(p => p.InviteSenderAccounts)
                    .HasForeignKey(d => d.SenderAccountId)
                    .HasConstraintName("FK__Invite__Sender_A__4D94879B");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.Invites)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__Invite__Startup___4F7CD00D");
            });

            modelBuilder.Entity<Label>(entity =>
            {
                entity.ToTable("Label");

                entity.Property(e => e.LabelId).HasColumnName("Label_ID");

                entity.Property(e => e.Color).HasMaxLength(50);

                entity.Property(e => e.LabelName).HasMaxLength(100);
            });

            modelBuilder.Entity<Milestone>(entity =>
            {
                entity.Property(e => e.MilestoneId).HasColumnName("Milestone_ID");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.Property(e => e.Status)
                    .HasMaxLength(30)
                    .HasColumnName("status");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.Milestones)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__Milestone__Start__59FA5E80");
            });

            modelBuilder.Entity<MilestoneAssignment>(entity =>
            {
                entity.ToTable("MilestoneAssignment");

                entity.Property(e => e.MilestoneAssignmentId).HasColumnName("MilestoneAssignment_ID");

                entity.Property(e => e.MemberId).HasColumnName("Member_ID");

                entity.Property(e => e.MilestoneId).HasColumnName("Milestone_ID");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.MilestoneAssignments)
                    .HasForeignKey(d => d.MemberId)
                    .HasConstraintName("FK__Milestone__Membe__5DCAEF64");

                entity.HasOne(d => d.Milestone)
                    .WithMany(p => p.MilestoneAssignments)
                    .HasForeignKey(d => d.MilestoneId)
                    .HasConstraintName("FK__Milestone__Miles__5CD6CB2B");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.NotificationId).HasColumnName("Notification_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.NotificationType).HasMaxLength(100);

                entity.Property(e => e.SendAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SenderId).HasColumnName("SenderID");

                entity.Property(e => e.TargetUrl)
                    .HasMaxLength(500)
                    .HasColumnName("TargetURL");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__Notificat__Accou__35BCFE0A");
            });

            modelBuilder.Entity<PermissionInStartup>(entity =>
            {
                entity.HasKey(e => e.PermissionId)
                    .HasName("PK__Permissi__89B744E54A367E58");

                entity.ToTable("PermissionInStartup");

                entity.Property(e => e.PermissionId).HasColumnName("Permission_ID");

                entity.Property(e => e.RoleId).HasColumnName("Role_ID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.PermissionInStartups)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_Permission_Role");
            });

            modelBuilder.Entity<Policy>(entity =>
            {
                entity.ToTable("Policy");

                entity.Property(e => e.PolicyId).HasColumnName("Policy_ID");

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.PolicyTypeId).HasColumnName("PolicyType_ID");

                entity.HasOne(d => d.PolicyType)
                    .WithMany(p => p.Policies)
                    .HasForeignKey(d => d.PolicyTypeId)
                    .HasConstraintName("FK__Policy__PolicyTy__59C55456");
            });

            modelBuilder.Entity<PolicyType>(entity =>
            {
                entity.ToTable("PolicyType");

                entity.Property(e => e.PolicyTypeId).HasColumnName("PolicyType_ID");

                entity.Property(e => e.TypeName).HasMaxLength(100);
            });

            modelBuilder.Entity<PositionRequirement>(entity =>
            {
                entity.HasKey(e => e.PositionId)
                    .HasName("PK__Position__3C3EAFE606822E24");

                entity.ToTable("PositionRequirement");

                entity.Property(e => e.PositionId).HasColumnName("Position_ID");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.Requirement).HasColumnName("requirement");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("Post");

                entity.Property(e => e.PostId).HasColumnName("Post_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PostShareId).HasColumnName("PostShare_ID");

                entity.Property(e => e.Schedule).HasColumnType("datetime");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasColumnName("title");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Posts)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__Post__Account_ID__1F98B2C1");

                entity.HasOne(d => d.PostShare)
                    .WithMany(p => p.InversePostShare)
                    .HasForeignKey(d => d.PostShareId)
                    .HasConstraintName("FK_Post_Share");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.Posts)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__Post__Startup_ID__208CD6FA");
            });

            modelBuilder.Entity<PostComment>(entity =>
            {
                entity.ToTable("PostComment");

                entity.Property(e => e.PostcommentId).HasColumnName("Postcomment_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.CommentAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ParentCommentId).HasColumnName("ParentComment_ID");

                entity.Property(e => e.PostId).HasColumnName("Post_ID");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.PostComments)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__PostComme__Accou__2A164134");

                entity.HasOne(d => d.ParentComment)
                    .WithMany(p => p.InverseParentComment)
                    .HasForeignKey(d => d.ParentCommentId)
                    .HasConstraintName("FK__PostComme__Paren__2B0A656D");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.PostComments)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK__PostComme__Post___29221CFB");
            });

            modelBuilder.Entity<PostHide>(entity =>
            {
                entity.ToTable("PostHide");

                entity.Property(e => e.PostHideId).HasColumnName("PostHide_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.HideAt)
                    .HasColumnType("datetime")
                    .HasColumnName("HideAT")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PostId).HasColumnName("Post_ID");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.PostHides)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__PostHide__Accoun__3493CFA7");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.PostHides)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK__PostHide__Post_I__339FAB6E");
            });

            modelBuilder.Entity<PostLike>(entity =>
            {
                entity.ToTable("PostLike");

                entity.HasIndex(e => new { e.PostId, e.AccountId }, "UC_PostLike")
                    .IsUnique();

                entity.Property(e => e.PostLikeId).HasColumnName("PostLike_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.LikedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PostId).HasColumnName("Post_ID");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.PostLikes)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__PostLike__Accoun__42E1EEFE");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.PostLikes)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK__PostLike__Post_I__41EDCAC5");
            });

            modelBuilder.Entity<PostMedium>(entity =>
            {
                entity.HasKey(e => e.PostMediaId)
                    .HasName("PK__PostMedi__AC7FDCFF3F324546");

                entity.Property(e => e.PostMediaId).HasColumnName("PostMedia_ID");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DisplayOrder).HasDefaultValueSql("((0))");

                entity.Property(e => e.MediaUrl).HasColumnName("MediaURl");

                entity.Property(e => e.PostId).HasColumnName("Post_ID");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.PostMedia)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK__PostMedia__Post___25518C17");
            });

            modelBuilder.Entity<PostReport>(entity =>
            {
                entity.HasKey(e => e.ReportId)
                    .HasName("PK__PostRepo__30FA9DB1287081AD");

                entity.ToTable("PostReport");

                entity.Property(e => e.ReportId).HasColumnName("Report_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PostId).HasColumnName("Post_ID");

                entity.Property(e => e.ReasonId).HasColumnName("Reason_ID");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.PostReports)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__PostRepor__Accou__3D2915A8");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.PostReports)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK__PostRepor__Post___3C34F16F");

                entity.HasOne(d => d.Reason)
                    .WithMany(p => p.PostReports)
                    .HasForeignKey(d => d.ReasonId)
                    .HasConstraintName("FK__PostRepor__Reaso__3B40CD36");
            });

            modelBuilder.Entity<ReportReason>(entity =>
            {
                entity.HasKey(e => e.ReasonId)
                    .HasName("PK__ReportRe__3435D2D7D1539224");

                entity.ToTable("ReportReason");

                entity.HasIndex(e => e.Reason, "UQ__ReportRe__1CC9147A0F1A1339")
                    .IsUnique();

                entity.Property(e => e.ReasonId).HasColumnName("Reason_ID");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.Reason).HasMaxLength(255);
            });

            modelBuilder.Entity<RoleInStartup>(entity =>
            {
                entity.HasKey(e => e.RoleId)
                    .HasName("PK__RoleInSt__D80AB49BD3C32957");

                entity.ToTable("RoleInStartup");

                entity.Property(e => e.RoleId).HasColumnName("Role_ID");

                entity.Property(e => e.RoleName)
                    .HasMaxLength(100)
                    .HasColumnName("Role_Name");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.RoleInStartups)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__RoleInSta__Start__48CFD27E");
            });

            modelBuilder.Entity<Startup>(entity =>
            {
                entity.ToTable("Startup");

                entity.HasIndex(e => e.Email, "UQ__Startup__A9D10534687DDA3B")
                    .IsUnique();

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.Property(e => e.AbbreviationName).HasMaxLength(30);

                entity.Property(e => e.BackgroundUrl)
                    .HasMaxLength(500)
                    .HasColumnName("backgroundURL");

                entity.Property(e => e.CreateAt).HasColumnType("date");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Logo).HasMaxLength(200);

                entity.Property(e => e.Mission).HasMaxLength(500);

                entity.Property(e => e.StageId).HasColumnName("Stage_ID");

                entity.Property(e => e.StartupName)
                    .HasMaxLength(255)
                    .HasColumnName("Startup_Name");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.Vision).HasMaxLength(500);

                entity.Property(e => e.WebsiteUrl)
                    .HasMaxLength(255)
                    .HasColumnName("WebsiteURL");

                entity.HasOne(d => d.Stage)
                    .WithMany(p => p.Startups)
                    .HasForeignKey(d => d.StageId)
                    .HasConstraintName("FK__Startup__Stage_I__3D5E1FD2");
            });

            modelBuilder.Entity<StartupCategory>(entity =>
            {
                entity.ToTable("StartupCategory");

                entity.HasIndex(e => new { e.StartupId, e.CategoryId }, "UC_StartupCategory")
                    .IsUnique();

                entity.Property(e => e.StartupCategoryId).HasColumnName("StartupCategory_ID");

                entity.Property(e => e.CategoryId).HasColumnName("Category_ID");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.StartupCategories)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK__StartupCa__Categ__44FF419A");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.StartupCategories)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__StartupCa__Start__440B1D61");
            });

            modelBuilder.Entity<StartupClick>(entity =>
            {
                entity.ToTable("StartupClick");

                entity.Property(e => e.StartupClickId).HasColumnName("StartupClick_ID");

                entity.Property(e => e.DateClick)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.StartupClicks)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK_StartupId");
            });

            modelBuilder.Entity<StartupLicense>(entity =>
            {
                entity.HasKey(e => e.LicenseId)
                    .HasName("PK__StartupL__5CA896B6136D5AF2");

                entity.Property(e => e.LicenseId).HasColumnName("License_ID");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.ExpiryDate).HasColumnType("date");

                entity.Property(e => e.IssueDate).HasColumnType("date");

                entity.Property(e => e.IssuedBy).HasMaxLength(200);

                entity.Property(e => e.LicenseName).HasMaxLength(200);

                entity.Property(e => e.LicenseNumber).HasMaxLength(100);

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.StartupLicenses)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__StartupLi__Start__60A75C0F");
            });

            modelBuilder.Entity<StartupMember>(entity =>
            {
                entity.ToTable("StartupMember");

                entity.HasIndex(e => new { e.StartupId, e.AccountId }, "UC_StartupMember")
                    .IsUnique();

                entity.Property(e => e.StartupMemberId).HasColumnName("StartupMember_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.JoinedAt).HasColumnType("date");

                entity.Property(e => e.RoleId).HasColumnName("Role_ID");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.StartupMembers)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__StartupMe__Accou__5535A963");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.StartupMembers)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK__StartupMe__Role___5629CD9C");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.StartupMembers)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__StartupMe__Start__5441852A");
            });

            modelBuilder.Entity<StartupPitching>(entity =>
            {
                entity.HasKey(e => e.PitchingId)
                    .HasName("PK__Startup___C55608ACE83ABCC4");

                entity.ToTable("Startup_Pitching");

                entity.Property(e => e.PitchingId).HasColumnName("Pitching_ID");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Link).HasMaxLength(500);

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.Property(e => e.Type).HasMaxLength(100);

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.StartupPitchings)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK_Startup_Pitching_Startup");
            });

            modelBuilder.Entity<StartupStage>(entity =>
            {
                entity.HasKey(e => e.StageId)
                    .HasName("PK__StartupS__32456A37F4978335");

                entity.ToTable("StartupStage");

                entity.HasIndex(e => e.StageName, "UQ__StartupS__8FE31B335BCBAC56")
                    .IsUnique();

                entity.Property(e => e.StageId).HasColumnName("Stage_ID");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.StageName).HasMaxLength(50);
            });

            modelBuilder.Entity<StartupTask>(entity =>
            {
                entity.HasKey(e => e.TaskId)
                    .HasName("PK__StartupT__716F4ACD3892639B");

                entity.ToTable("StartupTask");

                entity.Property(e => e.TaskId).HasColumnName("Task_ID");

                entity.Property(e => e.ColumnnStatusId).HasColumnName("ColumnnStatus_ID");

                entity.Property(e => e.Duedate).HasColumnType("datetime");

                entity.Property(e => e.MilestoneId).HasColumnName("Milestone_ID");

                entity.Property(e => e.Priority).HasMaxLength(50);

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasColumnName("title");

                entity.HasOne(d => d.ColumnnStatus)
                    .WithMany(p => p.StartupTasks)
                    .HasForeignKey(d => d.ColumnnStatusId)
                    .HasConstraintName("FK__StartupTa__Colum__6A30C649");

                entity.HasOne(d => d.Milestone)
                    .WithMany(p => p.StartupTasks)
                    .HasForeignKey(d => d.MilestoneId)
                    .HasConstraintName("FK__StartupTa__Miles__6B24EA82");
            });

            modelBuilder.Entity<StartupTaskLabel>(entity =>
            {
                entity.HasKey(e => new { e.TaskId, e.LabelId })
                    .HasName("PK__StartupT__42BD0FAF0D7B4724");

                entity.ToTable("StartupTask_Label");

                entity.Property(e => e.TaskId).HasColumnName("Task_ID");

                entity.Property(e => e.LabelId).HasColumnName("Label_ID");

                entity.Property(e => e.StartupTaskLabelId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("StartupTask_LabelId");

                entity.HasOne(d => d.Label)
                    .WithMany(p => p.StartupTaskLabels)
                    .HasForeignKey(d => d.LabelId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__StartupTa__Label__71D1E811");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.StartupTaskLabels)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__StartupTa__Task___70DDC3D8");
            });

            modelBuilder.Entity<Subcribe>(entity =>
            {
                entity.ToTable("Subcribe");

                entity.HasIndex(e => new { e.FollowerAccountId, e.FollowingStartUpId }, "UC_Subcribe")
                    .IsUnique();

                entity.Property(e => e.SubcribeId).HasColumnName("Subcribe_ID");

                entity.Property(e => e.FollowDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FollowerAccountId).HasColumnName("Follower_Account_ID");

                entity.Property(e => e.FollowingStartUpId).HasColumnName("Following_StartUp_ID");

                entity.HasOne(d => d.FollowerAccount)
                    .WithMany(p => p.Subcribes)
                    .HasForeignKey(d => d.FollowerAccountId)
                    .HasConstraintName("FK__Subcribe__Follow__6BE40491");

                entity.HasOne(d => d.FollowingStartUp)
                    .WithMany(p => p.Subcribes)
                    .HasForeignKey(d => d.FollowingStartUpId)
                    .HasConstraintName("FK__Subcribe__Follow__6AEFE058");
            });

            modelBuilder.Entity<TaskActivityLog>(entity =>
            {
                entity.HasKey(e => e.ActivityId)
                    .HasName("PK__TaskActi__45F4A79184027D9B");

                entity.ToTable("TaskActivityLog");

                entity.Property(e => e.ActionType).HasMaxLength(50);

                entity.Property(e => e.AtTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Content).HasMaxLength(500);

                entity.HasOne(d => d.ByAccount)
                    .WithMany(p => p.TaskActivityLogs)
                    .HasForeignKey(d => d.ByAccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TaskActivityLog_Account");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.TaskActivityLogs)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TaskActivityLog_Task");
            });

            modelBuilder.Entity<TaskAssignment>(entity =>
            {
                entity.HasKey(e => e.TaskAssignmentsId)
                    .HasName("PK__TaskAssi__D123598578A622A6");

                entity.Property(e => e.TaskAssignmentsId).HasColumnName("TaskAssignments_ID");

                entity.Property(e => e.AssignAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.AssignToAccountId).HasColumnName("AssignTo_Account_ID");

                entity.Property(e => e.AssignedByAccountId).HasColumnName("AssignedBy_Account_ID");

                entity.Property(e => e.TaskId).HasColumnName("Task_ID");

                entity.HasOne(d => d.AssignToAccount)
                    .WithMany(p => p.TaskAssignmentAssignToAccounts)
                    .HasForeignKey(d => d.AssignToAccountId)
                    .HasConstraintName("FK__TaskAssig__Assig__778AC167");

                entity.HasOne(d => d.AssignedByAccount)
                    .WithMany(p => p.TaskAssignmentAssignedByAccounts)
                    .HasForeignKey(d => d.AssignedByAccountId)
                    .HasConstraintName("FK__TaskAssig__Assig__76969D2E");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.TaskAssignments)
                    .HasForeignKey(d => d.TaskId)
                    .HasConstraintName("FK__TaskAssig__Task___75A278F5");
            });

            modelBuilder.Entity<UserCallSession>(entity =>
            {
                entity.HasKey(e => e.CallSessionId)
                    .HasName("PK__UserCall__F6D4C0C6D5BE6128");

                entity.ToTable("UserCallSession");

                entity.Property(e => e.CallSessionId).ValueGeneratedNever();

                entity.Property(e => e.EndedAt).HasColumnType("datetime");

                entity.Property(e => e.StartedAt).HasColumnType("datetime");

                entity.Property(e => e.Status).HasMaxLength(20);

                entity.HasOne(d => d.ChatRoom)
                    .WithMany(p => p.UserCallSessions)
                    .HasForeignKey(d => d.ChatRoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Call_ChatRoom");
            });

            modelBuilder.Entity<UserChatRoom>(entity =>
            {
                entity.HasKey(e => e.ChatRoomId)
                    .HasName("PK__UserChat__69733CF7E8AFD60F");

                entity.ToTable("UserChatRoom");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Type).HasMaxLength(20);
            });

            modelBuilder.Entity<UserChatRoomMember>(entity =>
            {
                entity.HasKey(e => e.ChatRoomMemberId)
                    .HasName("PK__UserChat__E5EEAA0EAEA7E8FD");

                entity.ToTable("UserChatRoomMember");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.UserChatRoomMembers)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__UserChatR__Start__42ACE4D4");

                entity.HasOne(d => d.ChatRoom)
                    .WithMany(p => p.UserChatRoomMembers)
                    .HasForeignKey(d => d.ChatRoomId)
                    .HasConstraintName("FK__UserChatR__ChatR__41B8C09B");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.UserChatRoomMembers)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__UserChatR__Start__43A1090D");
            });

            modelBuilder.Entity<UserMessage>(entity =>
            {
                entity.HasKey(e => e.MessageId)
                    .HasName("PK__UserMess__C87C0C9CE7F5D034");

                entity.ToTable("UserMessage");

                entity.Property(e => e.FileType).HasMaxLength(50);

                entity.Property(e => e.FileUrl).HasMaxLength(500);

                entity.Property(e => e.SentAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.ChatRoom)
                    .WithMany(p => p.UserMessages)
                    .HasForeignKey(d => d.ChatRoomId)
                    .HasConstraintName("FK__UserMessa__ChatR__467D75B8");
            });

            modelBuilder.Entity<UserOtp>(entity =>
            {
                entity.ToTable("UserOTP");

                entity.Property(e => e.UserOtpId).HasColumnName("UserOtp_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.ExpiresAt).HasColumnType("datetime");

                entity.Property(e => e.OtpCode)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.UserOtps)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__UserOTP__Account__29572725");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
