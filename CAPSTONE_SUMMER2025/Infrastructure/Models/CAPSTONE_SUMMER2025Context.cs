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
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<ChatMessage> ChatMessages { get; set; } = null!;
        public virtual DbSet<ChatRoom> ChatRooms { get; set; } = null!;
        public virtual DbSet<ChatRoomMember> ChatRoomMembers { get; set; } = null!;
        public virtual DbSet<FinancePlan> FinancePlans { get; set; } = null!;
        public virtual DbSet<Follow> Follows { get; set; } = null!;
        public virtual DbSet<InvestmentEvent> InvestmentEvents { get; set; } = null!;
        public virtual DbSet<InvestmentEventTicket> InvestmentEventTickets { get; set; } = null!;
        public virtual DbSet<InvestmentEventsRequest> InvestmentEventsRequests { get; set; } = null!;
        public virtual DbSet<InvestorComment> InvestorComments { get; set; } = null!;
        public virtual DbSet<Invite> Invites { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<Policy> Policies { get; set; } = null!;
        public virtual DbSet<PolicyType> PolicyTypes { get; set; } = null!;
        public virtual DbSet<Post> Posts { get; set; } = null!;
        public virtual DbSet<PostComment> PostComments { get; set; } = null!;
        public virtual DbSet<PostLike> PostLikes { get; set; } = null!;
        public virtual DbSet<PostMedium> PostMedia { get; set; } = null!;
        public virtual DbSet<PostReport> PostReports { get; set; } = null!;
        public virtual DbSet<ReportReason> ReportReasons { get; set; } = null!;
        public virtual DbSet<RoleInStartup> RoleInStartups { get; set; } = null!;
        public virtual DbSet<Startup> Startups { get; set; } = null!;
        public virtual DbSet<StartupCategory> StartupCategories { get; set; } = null!;
        public virtual DbSet<StartupImage> StartupImages { get; set; } = null!;
        public virtual DbSet<StartupLicense> StartupLicenses { get; set; } = null!;
        public virtual DbSet<StartupMember> StartupMembers { get; set; } = null!;
        public virtual DbSet<StartupStage> StartupStages { get; set; } = null!;
        public virtual DbSet<StartupTask> StartupTasks { get; set; } = null!;
        public virtual DbSet<TaskAssignment> TaskAssignments { get; set; } = null!;
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

                entity.HasIndex(e => e.Email, "UQ__Account__A9D105341BF650D7")
                    .IsUnique();

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false);

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
                    .HasName("PK__AccountB__A848958691FB7BEB");

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
                    .HasConstraintName("FK__AccountBl__Block__3B40CD36");

                entity.HasOne(d => d.BlockerAccount)
                    .WithMany(p => p.AccountBlockBlockerAccounts)
                    .HasForeignKey(d => d.BlockerAccountId)
                    .HasConstraintName("FK__AccountBl__Block__3A4CA8FD");
            });

            modelBuilder.Entity<AccountProfile>(entity =>
            {
                entity.ToTable("AccountProfile");

                entity.HasIndex(e => e.AccountId, "UQ__AccountP__B19E45C802D2E6EB")
                    .IsUnique();

                entity.Property(e => e.AccountProfileId).HasColumnName("AccountProfile_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.Address).HasMaxLength(200);

                entity.Property(e => e.AvatarUrl)
                    .HasMaxLength(500)
                    .HasColumnName("AvatarURL");

                entity.Property(e => e.Dob)
                    .HasColumnType("date")
                    .HasColumnName("DOB");

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.Gender).HasMaxLength(10);

                entity.Property(e => e.IdentityCardBack).HasMaxLength(255);

                entity.Property(e => e.IdentityCardFront).HasMaxLength(255);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.HasOne(d => d.Account)
                    .WithOne(p => p.AccountProfile)
                    .HasForeignKey<AccountProfile>(d => d.AccountId)
                    .HasConstraintName("FK__AccountPr__Accou__403A8C7D");
            });

            modelBuilder.Entity<Bio>(entity =>
            {
                entity.ToTable("BIO");

                entity.HasIndex(e => e.AccountId, "UQ__BIO__B19E45C8EFFF8DAC")
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
                    .HasConstraintName("FK__BIO__Account_ID__44FF419A");
            });

            modelBuilder.Entity<BusinessModelCanva>(entity =>
            {
                entity.HasKey(e => e.BmcId)
                    .HasName("PK__Business__806D52F6CFE6BAB2");

                entity.HasIndex(e => e.StartupId, "UQ__Business__BB46C8C05681DFA5")
                    .IsUnique();

                entity.Property(e => e.BmcId).HasColumnName("BMC_ID");

                entity.Property(e => e.CustomerSegments).HasColumnName("Customer_Segments");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.HasOne(d => d.Startup)
                    .WithOne(p => p.BusinessModelCanva)
                    .HasForeignKey<BusinessModelCanva>(d => d.StartupId)
                    .HasConstraintName("FK__BusinessM__Start__6FE99F9F");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.HasIndex(e => e.CategoryName, "UQ__Category__B35EB4194593E4F3")
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

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__ChatMessa__Accou__46B27FE2");

                entity.HasOne(d => d.ChatRoom)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.ChatRoomId)
                    .HasConstraintName("FK__ChatMessa__ChatR__45BE5BA9");
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
                    .HasConstraintName("FK__ChatRoom__Startu__3E1D39E1");
            });

            modelBuilder.Entity<ChatRoomMember>(entity =>
            {
                entity.HasKey(e => e.ChatGroupMembersId)
                    .HasName("PK__ChatRoom__AFC056A7D03D35E6");

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
                    .HasConstraintName("FK__ChatRoomM__Accou__42E1EEFE");

                entity.HasOne(d => d.ChatRoom)
                    .WithMany(p => p.ChatRoomMembers)
                    .HasForeignKey(d => d.ChatRoomId)
                    .HasConstraintName("FK__ChatRoomM__ChatR__41EDCAC5");
            });

            modelBuilder.Entity<FinancePlan>(entity =>
            {
                entity.Property(e => e.FinancePlanId).HasColumnName("FinancePlan_ID");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.FinanceplanSheetUrl).HasColumnName("FinanceplanSheetURL");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasColumnName("title");

                entity.Property(e => e.UpdateAt).HasColumnType("date");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.FinancePlans)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__FinancePl__Start__73BA3083");
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
                    .HasConstraintName("FK__Follow__Follower__1332DBDC");

                entity.HasOne(d => d.FollowingAccount)
                    .WithMany(p => p.FollowFollowingAccounts)
                    .HasForeignKey(d => d.FollowingAccountId)
                    .HasConstraintName("FK__Follow__Followin__14270015");
            });

            modelBuilder.Entity<InvestmentEvent>(entity =>
            {
                entity.HasKey(e => e.EventId)
                    .HasName("PK__Investme__FD6BEFE4EC1449BE");

                entity.Property(e => e.EventId).HasColumnName("Event_ID");

                entity.Property(e => e.Schedule).HasColumnType("datetime");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.InvestmentEvents)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__Investmen__Start__03F0984C");
            });

            modelBuilder.Entity<InvestmentEventTicket>(entity =>
            {
                entity.HasKey(e => e.TicketId)
                    .HasName("PK__Investme__ED7260D909231229");

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
                    .HasConstraintName("FK__Investmen__Accou__08B54D69");

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.InvestmentEventTickets)
                    .HasForeignKey(d => d.EventId)
                    .HasConstraintName("FK__Investmen__Event__07C12930");
            });

            modelBuilder.Entity<InvestmentEventsRequest>(entity =>
            {
                entity.HasKey(e => e.RequestId)
                    .HasName("PK__Investme__E9C5B293EEB01EC4");

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
                    .HasConstraintName("FK__Investmen__Accou__0E6E26BF");

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.InvestmentEventsRequests)
                    .HasForeignKey(d => d.EventId)
                    .HasConstraintName("FK__Investmen__Event__0D7A0286");
            });

            modelBuilder.Entity<InvestorComment>(entity =>
            {
                entity.ToTable("InvestorComment");

                entity.Property(e => e.InvestorCommentId).HasColumnName("InvestorComment_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.CommentAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.InvestorComments)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__InvestorC__Accou__17F790F9");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.InvestorComments)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__InvestorC__Start__18EBB532");
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
                    .HasConstraintName("FK__Invite__Receiver__619B8048");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Invites)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK__Invite__Role_ID__6383C8BA");

                entity.HasOne(d => d.SenderAccount)
                    .WithMany(p => p.InviteSenderAccounts)
                    .HasForeignKey(d => d.SenderAccountId)
                    .HasConstraintName("FK__Invite__Sender_A__60A75C0F");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.Invites)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__Invite__Startup___628FA481");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.NotificationId).HasColumnName("Notification_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.SendAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__Notificat__Accou__48CFD27E");
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
                    .HasConstraintName("FK__Policy__PolicyTy__4C6B5938");
            });

            modelBuilder.Entity<PolicyType>(entity =>
            {
                entity.ToTable("PolicyType");

                entity.Property(e => e.PolicyTypeId).HasColumnName("PolicyType_ID");

                entity.Property(e => e.TypeName).HasMaxLength(100);
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("Post");

                entity.Property(e => e.PostId).HasColumnName("Post_ID");

                entity.Property(e => e.AccountId).HasColumnName("Account_ID");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasColumnName("title");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Posts)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__Post__Account_ID__1CBC4616");
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
                    .HasConstraintName("FK__PostComme__Accou__2645B050");

                entity.HasOne(d => d.ParentComment)
                    .WithMany(p => p.InverseParentComment)
                    .HasForeignKey(d => d.ParentCommentId)
                    .HasConstraintName("FK__PostComme__Paren__2739D489");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.PostComments)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK__PostComme__Post___25518C17");
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
                    .HasConstraintName("FK__PostLike__Accoun__3587F3E0");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.PostLikes)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK__PostLike__Post_I__3493CFA7");
            });

            modelBuilder.Entity<PostMedium>(entity =>
            {
                entity.HasKey(e => e.PostMediaId)
                    .HasName("PK__PostMedi__AC7FDCFF5D517D4E");

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
                    .HasConstraintName("FK__PostMedia__Post___2180FB33");
            });

            modelBuilder.Entity<PostReport>(entity =>
            {
                entity.HasKey(e => e.ReportId)
                    .HasName("PK__PostRepo__30FA9DB1D499C9FF");

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
                    .HasConstraintName("FK__PostRepor__Accou__2FCF1A8A");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.PostReports)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK__PostRepor__Post___2EDAF651");

                entity.HasOne(d => d.Reason)
                    .WithMany(p => p.PostReports)
                    .HasForeignKey(d => d.ReasonId)
                    .HasConstraintName("FK__PostRepor__Reaso__2DE6D218");
            });

            modelBuilder.Entity<ReportReason>(entity =>
            {
                entity.HasKey(e => e.ReasonId)
                    .HasName("PK__ReportRe__3435D2D72ED0B16C");

                entity.ToTable("ReportReason");

                entity.HasIndex(e => e.Reason, "UQ__ReportRe__1CC9147A72028889")
                    .IsUnique();

                entity.Property(e => e.ReasonId).HasColumnName("Reason_ID");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.Reason).HasMaxLength(255);
            });

            modelBuilder.Entity<RoleInStartup>(entity =>
            {
                entity.HasKey(e => e.RoleId)
                    .HasName("PK__RoleInSt__D80AB49B8425689A");

                entity.ToTable("RoleInStartup");

                entity.HasIndex(e => e.RoleName, "UQ__RoleInSt__035DB749DDC94C81")
                    .IsUnique();

                entity.Property(e => e.RoleId).HasColumnName("Role_ID");

                entity.Property(e => e.RoleName)
                    .HasMaxLength(100)
                    .HasColumnName("Role_Name");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.RoleInStartups)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__RoleInSta__Start__5BE2A6F2");
            });

            modelBuilder.Entity<Startup>(entity =>
            {
                entity.ToTable("Startup");

                entity.HasIndex(e => e.Email, "UQ__Startup__A9D10534737B6F3D")
                    .IsUnique();

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.Property(e => e.AbbreviationName).HasMaxLength(30);

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
                    .HasConstraintName("FK__Startup__Stage_I__5070F446");
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
                    .HasConstraintName("FK__StartupCa__Categ__5812160E");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.StartupCategories)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__StartupCa__Start__571DF1D5");
            });

            modelBuilder.Entity<StartupImage>(entity =>
            {
                entity.ToTable("StartupImage");

                entity.Property(e => e.StartupImageId).HasColumnName("StartupImage_ID");

                entity.Property(e => e.ImageUrl).HasColumnName("ImageURL");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.StartupImages)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__StartupIm__Start__76969D2E");
            });

            modelBuilder.Entity<StartupLicense>(entity =>
            {
                entity.HasKey(e => e.LicenseId)
                    .HasName("PK__StartupL__5CA896B686C3BD61");

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
                    .HasConstraintName("FK__StartupLi__Start__6C190EBB");
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
                    .HasConstraintName("FK__StartupMe__Accou__68487DD7");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.StartupMembers)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK__StartupMe__Role___693CA210");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.StartupMembers)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__StartupMe__Start__6754599E");
            });

            modelBuilder.Entity<StartupStage>(entity =>
            {
                entity.HasKey(e => e.StageId)
                    .HasName("PK__StartupS__32456A37839B2BAF");

                entity.ToTable("StartupStage");

                entity.HasIndex(e => e.StageName, "UQ__StartupS__8FE31B33025A1CE5")
                    .IsUnique();

                entity.Property(e => e.StageId).HasColumnName("Stage_ID");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.StageName).HasMaxLength(50);
            });

            modelBuilder.Entity<StartupTask>(entity =>
            {
                entity.HasKey(e => e.TaskId)
                    .HasName("PK__StartupT__716F4ACDC7DCC793");

                entity.ToTable("StartupTask");

                entity.Property(e => e.TaskId).HasColumnName("Task_ID");

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.Priority).HasMaxLength(50);

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.StartupId).HasColumnName("Startup_ID");

                entity.Property(e => e.Status).HasMaxLength(30);

                entity.Property(e => e.Tags).HasMaxLength(100);

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasColumnName("title");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Startup)
                    .WithMany(p => p.StartupTasks)
                    .HasForeignKey(d => d.StartupId)
                    .HasConstraintName("FK__StartupTa__Start__7B5B524B");
            });

            modelBuilder.Entity<TaskAssignment>(entity =>
            {
                entity.HasKey(e => e.TaskAssignmentsId)
                    .HasName("PK__TaskAssi__D1235985470CBFAA");

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
                    .HasConstraintName("FK__TaskAssig__Assig__01142BA1");

                entity.HasOne(d => d.AssignedByAccount)
                    .WithMany(p => p.TaskAssignmentAssignedByAccounts)
                    .HasForeignKey(d => d.AssignedByAccountId)
                    .HasConstraintName("FK__TaskAssig__Assig__00200768");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.TaskAssignments)
                    .HasForeignKey(d => d.TaskId)
                    .HasConstraintName("FK__TaskAssig__Task___7F2BE32F");
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
                    .HasConstraintName("FK__UserOTP__Account__3C69FB99");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
