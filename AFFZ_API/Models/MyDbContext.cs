using AFFZ_API.Controllers.MerchantControllers;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Models;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }
    public DbSet<Review> Review { get; set; }

    public virtual DbSet<ActionLog> ActionLogs { get; set; }

    public virtual DbSet<AdminDashboard> AdminDashboards { get; set; }

    public virtual DbSet<AdminUser> AdminUsers { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<Customers> Customers { get; set; }

    public virtual DbSet<DashboardItem> DashboardItems { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<LoginHistory> LoginHistories { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<MenuPermission> MenuPermissions { get; set; }

    public virtual DbSet<Merchant> Merchants { get; set; }

    public virtual DbSet<MerchantDashboard> MerchantDashboards { get; set; }

    public virtual DbSet<MerchantRating> MerchantRatings { get; set; }
    public virtual DbSet<RequestStatuses> RequestStatuses { get; set; }

    public virtual DbSet<MerchantUser> MerchantUsers { get; set; }

    public virtual DbSet<MerchantUserDashboard> MerchantUserDashboards { get; set; }

    public virtual DbSet<MerchantUserRating> MerchantUserRatings { get; set; }

    public virtual DbSet<PaymentHistory> PaymentHistories { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Policy> Policies { get; set; }

    public virtual DbSet<ProviderUser> ProviderUsers { get; set; }

    public virtual DbSet<RequestForDisCountToMerchant> RequestForDisCountToMerchants { get; set; }

    public virtual DbSet<RequestForDisCountToUser> RequestForDisCountToUsers { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceCategory> ServiceCategories { get; set; }

    public virtual DbSet<ServiceDocument> ServiceDocuments { get; set; }

    public virtual DbSet<SubMenu> SubMenus { get; set; }

    public virtual DbSet<SubscribeChannel> SubscribeChannels { get; set; }

    public virtual DbSet<SuperUserDashboard> SuperUserDashboards { get; set; }
    public virtual DbSet<CurrentServiceStatus> CurrentServiceStatus { get; set; }
    public virtual DbSet<TrackServiceStatusHistory> TrackServiceStatusHistory { get; set; }

    public virtual DbSet<UploadedFile> UploadedFiles { get; set; }
    public virtual DbSet<DocumentInfo> DocumentInfo { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<ReviewSummaryDto> ReviewSummaryDto { get; set; }

    public virtual DbSet<UserGroupPermission> UserGroupPermissions { get; set; }
    public virtual DbSet<M_SericeDefaultDocumentList> m_SericeDefaultDocumentList { get; set; }
    public virtual DbSet<UserRolePermission> UserRolePermissions { get; set; }
    public DbSet<ServicesList> ServicesLists { get; set; }
    public DbSet<ServiceDocumenList> ServiceDocumentList { get; set; } // New table
    public DbSet<ServiceDocumentMapping> ServiceDocumentMapping { get; set; } // New table
    public virtual DbSet<UserSubscriptionChannel> UserSubscriptionChannels { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<Message> Messages { get; set; }//
    public virtual DbSet<ProviderMerchant> ProviderMerchant { get; set; }
    public virtual DbSet<MerchantVerificationDocumentList> MerchantVerificationDocumentList { get; set; }
    public virtual DbSet<MerchantDocuments> MerchantDocuments { get; set; }
    public DbSet<M_SericeDocumentListBinding> M_ServiceDocumentListBinding { get; set; }
    public DbSet<MiniDashboardResult> MiniDashboardResults { get; set; }
    public DbSet<GetRecentTransaction> GetRecentTransaction { get; set; }
    public DbSet<TotalRevenueResult> TotalRevenueResults { get; set; }
    public DbSet<MiniDashBoardData> MiniDashBoardData { get; set; }
    public DbSet<Referral> Referral { get; set; }
    public DbSet<Wallet> Wallet { get; set; }
    public DbSet<Cart> Cart { get; set; }
    public DbSet<ReviewViewModel> ReviewViewModel { get; set; }
    public DbSet<CartItem> CartItem { get; set; }
    // Eligibility DbSets
    public DbSet<EligibilityRequest> EligibilityRequests { get; set; }
    public DbSet<EligibilityDocument> EligibilityDocuments { get; set; }
    public DbSet<Slab> Slab { get; set; }
    public DbSet<MembershipPaymentHistory> MembershipPaymentHistory { get; set; }
    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=ZAINMAQBOOL\\SQLEXPRESS;Database=Test;User Id=zain;Password=zain@123;TrustServerCertificate=Yes;");
    public DbSet<MembershipPlan> MembershipPlans { get; set; }
    public DbSet<AmountChangeRequests> AmountChangeRequests { get; set; }
    public DbSet<ProviderSlab> ProviderSlab { get; set; }
    public DbSet<MembershipPlanSlabs> MembershipPlanSlabs { get; set; }
    public DbSet<ConfigurationSettings> ConfigurationSettings { get; set; }
    public DbSet<BankTransferAccount> BankTransferAccount { get; set; }
    public DbSet<PayoutSettings> PayoutSettings { get; internal set; }
    public DbSet<PayoutTransaction> PayoutTransaction { get; internal set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActionLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__ActionLo__5E5499A82B58D70B");

            entity.ToTable("ActionLog");

            entity.Property(e => e.LogId)
                .ValueGeneratedNever()
                .HasColumnName("LogID");
            entity.Property(e => e.ActionDetails).IsUnicode(false);
            entity.Property(e => e.ActionTimestamp).HasColumnType("datetime");
            entity.Property(e => e.ActionType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.ActionLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ActionLog__UserI__5F7E2DAC");
        });

        modelBuilder.Entity<AdminDashboard>(entity =>
        {
            entity.HasKey(e => e.AdminDashboardId).HasName("PK__AdminDas__40D0D1D802C0AD68");

            entity.ToTable("AdminDashboard");

            entity.Property(e => e.AdminDashboardId)
                .ValueGeneratedNever()
                .HasColumnName("AdminDashboardID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DashboardItemId).HasColumnName("DashboardItemID");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");

            entity.HasOne(d => d.DashboardItem).WithMany(p => p.AdminDashboards)
                .HasForeignKey(d => d.DashboardItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AdminDash__Dashb__607251E5");
        });

        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__AdminUse__1788CC4C0EC7E5F0");

            entity.HasIndex(e => e.Email, "UQ__AdminUse__A9D105340D054614").IsUnique();

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.AdminUsers)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__AdminUser__RoleI__6E01572D");
        });
        modelBuilder.Entity<ServiceDocumenList>(entity =>
        {
            entity.HasKey(e => e.ServiceDocumenListtId);
            entity.Property(e => e.ServiceDocumentName).HasColumnType("varchar(max)");
        });
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId);
            entity.HasOne(r => r.CUser) // Navigation property to Customers
                  .WithMany()           // Assuming Customers doesn't have a collection of Reviews
                  .HasForeignKey(r => r.CustomerId); // Foreign key in Review
        });
        modelBuilder.Entity<M_SericeDefaultDocumentList>()
                .HasKey(e => e.MSericeDefaultDocumentListID);  // If you don't have a primary key
                                                               //.ToTable("M_SericeDefaultDocumentList");

        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId);

            entity.ToTable("ChatMessage");

            entity.Property(e => e.MessageId).HasColumnName("MessageID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.MerchantId).HasColumnName("MerchantID");
            entity.Property(e => e.MessageContent).IsUnicode(false);
            entity.Property(e => e.MessageTimestamp).HasColumnType("datetime");
            entity.Property(e => e.MessageType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.ReceiverId).HasColumnName("ReceiverID");
            entity.Property(e => e.SenderId).HasColumnName("SenderID");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ChatMessageCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessage_CreatedBy");

            entity.HasOne(d => d.Merchant).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.MerchantId)
                .HasConstraintName("FK_ChatMessage_Merchant");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.ChatMessageModifiedByNavigations)
                .HasForeignKey(d => d.ModifiedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessage_ModifiedBy");

            entity.HasOne(d => d.Receiver).WithMany(p => p.ChatMessageReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessage_Receiver");

            entity.HasOne(d => d.Sender).WithMany(p => p.ChatMessageSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessage_Sender");
        });

        modelBuilder.Entity<Customers>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64D8C7BB4B0B");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.Email, "UQ__Customer__A9D10534AC2133AF").IsUnique();

            entity.Property(e => e.Address).IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.DOB).HasColumnName("DOB");
            entity.Property(e => e.Email)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.MemberSince).HasColumnType("datetime");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PostalCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ProfilePicture).IsUnicode(false);
            entity.ToTable(tb => tb.HasTrigger("trg_CreateWallet"));
        });
        // Define composite primary key for ServiceDocumentMapping
        modelBuilder.Entity<ServiceDocumentMapping>()
            .HasKey(sd => new { sd.ServiceID, sd.ServiceDocumentListId });

        modelBuilder.Entity<DashboardItem>(entity =>
        {
            entity.HasKey(e => e.DashboardItemId).HasName("PK__Dashboar__98A75D474ECF2C22");

            entity.Property(e => e.DashboardItemId)
                .ValueGeneratedNever()
                .HasColumnName("DashboardItemID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DashboardItemName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.DashboardItemType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Document");

            entity.Property(e => e.DocumentId).HasColumnName("DocumentID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DocumentName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.FilePath).IsUnicode(false);
            entity.Property(e => e.MerchantId).HasColumnName("MerchantID");
            entity.Property(e => e.MerchantUserId).HasColumnName("MerchantUserID");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.DocumentCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Document_CreatedBy");

            entity.HasOne(d => d.Merchant).WithMany(p => p.Documents)
                .HasForeignKey(d => d.MerchantId)
                .HasConstraintName("FK_Document_Merchant");

            entity.HasOne(d => d.MerchantUser).WithMany(p => p.Documents)
                .HasForeignKey(d => d.MerchantUserId)
                .HasConstraintName("FK_Document_MerchantUser");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.DocumentModifiedByNavigations)
                .HasForeignKey(d => d.ModifiedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Document_ModifiedBy");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.DocumentUploadedByNavigations)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Document_UploadedBy");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__Groups__149AF30A9CA90782");

            entity.Property(e => e.GroupId)
                .ValueGeneratedNever()
                .HasColumnName("GroupID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.GroupName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__LoginHis__5E5499A8C90747CA");

            entity.ToTable("LoginHistory");

            entity.Property(e => e.LogId)
                .ValueGeneratedNever()
                .HasColumnName("LogID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("IPAddress");
            entity.Property(e => e.LoginTimestamp).HasColumnType("datetime");
            entity.Property(e => e.LogoutTimestamp).HasColumnType("datetime");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.LoginHistories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LoginHist__UserI__6AEFE058");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__Menu__C99ED2306ECB791B");

            entity.ToTable("Menu");

            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.MenuIcon)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MenuName)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.MenuUrl)
                .HasMaxLength(250)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MenuPermission>(entity =>
        {
            entity.HasKey(e => e.MenuPermissionId).HasName("PK__MenuPerm__830F63A5A923DD0C");

            entity.Property(e => e.MenuPermissionId)
                .ValueGeneratedNever()
                .HasColumnName("MenuPermissionID");
            entity.Property(e => e.CreatedDate)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.MenuId).HasColumnName("MenuID");
            entity.Property(e => e.ModifyDate)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PermissionId).HasColumnName("PermissionID");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Group).WithMany(p => p.MenuPermissions)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_MenuPermissions_Groups");

            entity.HasOne(d => d.User).WithMany(p => p.MenuPermissions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_MenuPermissions_Users");
        });

        modelBuilder.Entity<Merchant>(entity =>
        {
            entity.HasKey(e => e.MerchantId).HasName("PK__Merchant__044165634314BDE3");

            entity.ToTable("Merchant");

            entity.Property(e => e.MerchantId)
                .ValueGeneratedOnAdd() // Ensure EF treats it as an identity column
                .HasColumnName("MerchantID");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CompanyRegistrationNumber)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ContactInfo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.EmiratesId)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MerchantLocation)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.RegistrationMethod)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TradingLicense)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MerchantDashboard>(entity =>
        {
            entity.HasKey(e => e.MerchantDashboardId).HasName("PK__Merchant__05A4BD181C5FE1E2");

            entity.ToTable("MerchantDashboard");

            entity.Property(e => e.MerchantDashboardId)
                .ValueGeneratedNever()
                .HasColumnName("MerchantDashboardID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DashboardItemId).HasColumnName("DashboardItemID");
            entity.Property(e => e.MerchantId).HasColumnName("MerchantID");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");

            entity.HasOne(d => d.DashboardItem).WithMany(p => p.MerchantDashboards)
                .HasForeignKey(d => d.DashboardItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MerchantD__Dashb__70A8B9AE");

            entity.HasOne(d => d.Merchant).WithMany(p => p.MerchantDashboards)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MerchantD__Merch__719CDDE7");
        });

        modelBuilder.Entity<MerchantRating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("PK__Merchant__FCCDF85CAC718A67");

            entity.ToTable("MerchantRating");

            entity.Property(e => e.RatingId)
                .ValueGeneratedNever()
                .HasColumnName("RatingID");
            entity.Property(e => e.Comments).IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.RatedByUserId).HasColumnName("RatedByUserID");
            entity.Property(e => e.RatedDate).HasColumnType("datetime");
            entity.Property(e => e.RatedMerchantId).HasColumnName("RatedMerchantID");

            entity.HasOne(d => d.RatedByUser).WithMany(p => p.MerchantRatings)
                .HasForeignKey(d => d.RatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MerchantR__Rated__72910220");

            entity.HasOne(d => d.RatedMerchant).WithMany(p => p.MerchantRatings)
                .HasForeignKey(d => d.RatedMerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MerchantR__Rated__73852659");
        });

        modelBuilder.Entity<MerchantUser>(entity =>
        {
            entity.HasKey(e => e.MerchantUserId).HasName("PK__Merchant__AA7C715D1C63D4AF");

            entity.ToTable("MerchantUser");

            entity.Property(e => e.MerchantUserId)
                .ValueGeneratedNever()
                .HasColumnName("MerchantUserID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Lastlogindate)
                .HasColumnType("datetime")
                .HasColumnName("lastlogindate");
            entity.Property(e => e.MerchantId).HasColumnName("MerchantID");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Rfu1)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu1");
            entity.Property(e => e.Rfu10)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu10");
            entity.Property(e => e.Rfu2)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu2");
            entity.Property(e => e.Rfu3)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu3");
            entity.Property(e => e.Rfu4)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu4");
            entity.Property(e => e.Rfu5)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu5");
            entity.Property(e => e.Rfu6)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu6");
            entity.Property(e => e.Rfu7)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu7");
            entity.Property(e => e.Rfu8)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu8");
            entity.Property(e => e.Rfu9)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu9");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Group).WithMany(p => p.MerchantUsers)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__MerchantU__Group__74794A92");

            entity.HasOne(d => d.Merchant).WithMany(p => p.MerchantUsers)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MerchantU__Merch__756D6ECB");

        });

        modelBuilder.Entity<MerchantUserDashboard>(entity =>
        {
            entity.HasKey(e => e.MerchantUserDashboardId).HasName("PK__Merchant__34D3F5A0F2B342F6");

            entity.ToTable("MerchantUserDashboard");

            entity.Property(e => e.MerchantUserDashboardId)
                .ValueGeneratedNever()
                .HasColumnName("MerchantUserDashboardID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DashboardItemId).HasColumnName("DashboardItemID");
            entity.Property(e => e.MerchantUserId).HasColumnName("MerchantUserID");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");

            entity.HasOne(d => d.DashboardItem).WithMany(p => p.MerchantUserDashboards)
                .HasForeignKey(d => d.DashboardItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MerchantU__Dashb__7755B73D");

            entity.HasOne(d => d.MerchantUser).WithMany(p => p.MerchantUserDashboards)
                .HasForeignKey(d => d.MerchantUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MerchantU__Merch__7849DB76");
        });

        modelBuilder.Entity<MerchantUserRating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("PK__Merchant__FCCDF85CD624002E");

            entity.ToTable("MerchantUserRating");

            entity.Property(e => e.RatingId)
                .ValueGeneratedNever()
                .HasColumnName("RatingID");
            entity.Property(e => e.Comments).IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.RatedByUserId).HasColumnName("RatedByUserID");
            entity.Property(e => e.RatedDate).HasColumnType("datetime");
            entity.Property(e => e.RatedMerchantUserId).HasColumnName("RatedMerchantUserID");

            entity.HasOne(d => d.RatedByUser).WithMany(p => p.MerchantUserRatings)
                .HasForeignKey(d => d.RatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MerchantU__Rated__793DFFAF");

            entity.HasOne(d => d.RatedMerchantUser).WithMany(p => p.MerchantUserRatings)
                .HasForeignKey(d => d.RatedMerchantUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MerchantU__Rated__7A3223E8");
        });

        modelBuilder.Entity<PaymentHistory>(entity =>
        {
            entity.HasKey(e => e.ID);

            entity.ToTable("PaymentHistory");
            entity.ToTable(tb => tb.HasTrigger("trg_CreateReferralCodeOnPaymentSuccess"));
        });


        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__EFA6FB2FD1151504");

            entity.Property(e => e.CanView).HasDefaultValue(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");

            entity.HasOne(d => d.Menu).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.MenuId)
                .HasConstraintName("FK__Permissio__MenuI__7F2BE32F");

            entity.HasOne(d => d.Role).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Permissio__RoleI__7E37BEF6");
        });

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__Policies__2E133944FC3277AB");

            entity.Property(e => e.PolicyId)
                .ValueGeneratedNever()
                .HasColumnName("PolicyID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.PolicyContent).IsUnicode(false);
            entity.Property(e => e.PolicyName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ProviderUser>(entity =>
        {
            entity.HasKey(e => e.ProviderId).HasName("PK__Merchant__04416563528C7257");

            entity.ToTable("ProviderUser");

            entity.HasIndex(e => e.Email, "UQ__Merchant__A9D10534EDD25F95").IsUnique();

            entity.Property(e => e.ProviderId).HasColumnName("ProviderID");
            entity.Property(e => e.Address).IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.MemberSince).HasColumnType("datetime");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PostalCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ProfilePicture)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.ProviderName)
                .HasMaxLength(250)
                .IsUnicode(false);
        });

        modelBuilder.Entity<RequestForDisCountToMerchant>(entity =>
        {
            entity.HasKey(e => e.RFDTM);

            entity.ToTable("RequestForDisCountToMerchant");

            // Disable automatic value generation for the primary key
            //entity.Property(e => e.RFDTM).ValueGeneratedNever();

            // Configure RFDTM as an identity column
            entity.Property(e => e.RFDTM).UseIdentityColumn();

            entity.Property(e => e.RFDTM).HasColumnName("RFDTM");
            entity.Property(e => e.MID).HasColumnName("MID");
            entity.Property(e => e.RequestDateTime).HasColumnType("datetime");
            entity.Property(e => e.SID).HasColumnName("SID");
            entity.Property(e => e.UID).HasColumnName("UID");
        });

        modelBuilder.Entity<RequestForDisCountToUser>(entity =>
        {
            entity.HasKey(e => e.RFDFU);

            entity.ToTable("RequestForDisCountToUser");

            entity.Property(e => e.RFDFU).HasColumnName("RFDFU");
            entity.Property(e => e.FINALPRICE).HasColumnName("FINALPRICE");
            entity.Property(e => e.MID).HasColumnName("MID");
            entity.Property(e => e.ResponseDateTime).HasColumnType("datetime");
            entity.Property(e => e.SID).HasColumnName("SID");
            entity.Property(e => e.UID).HasColumnName("UID");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A49AC1356");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.RoleName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Service__C51BB0EA097BD63F");

            entity.ToTable("Service");

            // Update this line
            entity.Property(e => e.ServiceId)
                .ValueGeneratedOnAdd() // Change this to ValueGeneratedOnAdd
                .HasColumnName("ServiceID");

            entity.Property(e => e.CategoryID).HasColumnName("CategoryID");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.MerchantID).HasColumnName("MerchantID");
            entity.Property(e => e.SID)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Category).WithMany(p => p.Services)
                .HasForeignKey(d => d.CategoryID)
                .HasConstraintName("FK_Category_Service");

            entity.HasOne(d => d.Merchant).WithMany(p => p.Services)
                .HasForeignKey(d => d.MerchantID)
                .HasConstraintName("FK_Merchant_Service");
        });

        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__ServiceC__19093A2B2102739A");

            entity.ToTable("ServiceCategory");

            entity.Property(e => e.CategoryId)
                .ValueGeneratedNever()
                .HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ServiceDocument>(entity =>
        {
            entity.ToTable("SERVICE_DOCUMENTS");

            entity.Property(e => e.ServiceDocumentId).HasColumnName("SERVICE_DOCUMENT_ID");
            entity.Property(e => e.DocumentDetail)
                .HasColumnType("text")
                .HasColumnName("DOCUMENT_DETAIL");
            entity.Property(e => e.ServiceId).HasColumnName("SERVICE_ID");
        });

        modelBuilder.Entity<SubMenu>(entity =>
        {
            entity.HasKey(e => e.SubMenuId).HasName("PK__SubMenu__EA065CF9514F8548");

            entity.ToTable("SubMenu");

            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.SubMenuName)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.SubMenuUrl)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.HasOne(d => d.Menu).WithMany(p => p.SubMenus)
                .HasForeignKey(d => d.MenuId)
                .HasConstraintName("FK__SubMenu__MenuId__7B5B524B");
        });

        modelBuilder.Entity<SubscribeChannel>(entity =>
        {
            entity.HasKey(e => e.SubscriptionChannelId);

            entity.ToTable("SubscribeChannel");

            entity.Property(e => e.SubscriptionChannelType)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SuperUserDashboard>(entity =>
        {
            entity.HasKey(e => e.SuperUserDashboardId).HasName("PK__SuperUse__8AC207EA8AA23FFF");

            entity.ToTable("SuperUserDashboard");

            entity.Property(e => e.SuperUserDashboardId)
                .ValueGeneratedNever()
                .HasColumnName("SuperUserDashboardID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DashboardItemId).HasColumnName("DashboardItemID");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");

            entity.HasOne(d => d.DashboardItem).WithMany(p => p.SuperUserDashboards)
                .HasForeignKey(d => d.DashboardItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SuperUser__Dashb__7D0E9093");
        });

        modelBuilder.Entity<UploadedFile>(entity =>
        {
            entity.HasKey(e => e.Ufid).HasName("PK_UploadedFiles");

            entity.ToTable("UploadedFile");

            entity.Property(e => e.Ufid).HasColumnName("UFID");
            entity.Property(e => e.ContentType).IsUnicode(false);
            entity.Property(e => e.DocumentAddedDate).HasColumnType("datetime");
            entity.Property(e => e.DocumentModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.FileName).IsUnicode(false);
            entity.Property(e => e.FolderName).IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACC6F159D0");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Lastlogindate)
                .HasColumnType("datetime")
                .HasColumnName("lastlogindate");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Rfu1)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu1");
            entity.Property(e => e.Rfu10)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu10");
            entity.Property(e => e.Rfu2)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu2");
            entity.Property(e => e.Rfu3)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu3");
            entity.Property(e => e.Rfu4)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu4");
            entity.Property(e => e.Rfu5)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu5");
            entity.Property(e => e.Rfu6)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu6");
            entity.Property(e => e.Rfu7)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu7");
            entity.Property(e => e.Rfu8)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu8");
            entity.Property(e => e.Rfu9)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rfu9");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Group).WithMany(p => p.Users)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__Users__GroupID__01D345B0");

        });

        modelBuilder.Entity<UserGroupPermission>(entity =>
        {
            entity.HasKey(e => e.UserGroupPermissionId).HasName("PK__UserGrou__EDE9BE0B3CA402B7");

            entity.Property(e => e.UserGroupPermissionId)
                .ValueGeneratedNever()
                .HasColumnName("UserGroupPermissionID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.PermissionId).HasColumnName("PermissionID");

            entity.HasOne(d => d.Group).WithMany(p => p.UserGroupPermissions)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserGroup__Group__7E02B4CC");

        });

        modelBuilder.Entity<UserRolePermission>(entity =>
        {
            entity.HasKey(e => e.UserRolePermissionId).HasName("PK__UserRole__2B16EC849464FF25");

            entity.Property(e => e.UserRolePermissionId)
                .ValueGeneratedNever()
                .HasColumnName("UserRolePermissionID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.PermissionId).HasColumnName("PermissionID");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

        });
        modelBuilder.Entity<ServicesList>().ToTable("ServicesList");
        modelBuilder.Entity<ServicesList>().HasKey(s => s.ServiceListID);
        modelBuilder.Entity<UserSubscriptionChannel>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId);

            entity.ToTable("UserSubscriptionChannel");
        });
        // Define EligibilityRequest Primary Key
        //modelBuilder.Entity<EligibilityRequest>()
        //    .HasKey(er => er.RequestID)
        //    .HasTrigger("trg_UpdateRequestDate");
        modelBuilder.Entity<EligibilityRequest>(entity =>
        {
            entity.HasKey(er => er.RequestID);

            entity.ToTable("EligibilityRequests");
            entity.ToTable(tb => tb.HasTrigger("trg_UpdateRequestDate"));
        });
        // Define Relationships for EligibilityRequest
        modelBuilder.Entity<EligibilityRequest>()
            .HasOne(er => er.Customer) // Relationship with Customer
            .WithMany(c => c.CustomerRequests)
            .HasForeignKey(er => er.CustomerID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EligibilityRequest>()
            .HasOne(er => er.Merchant) // Relationship with Merchant
            .WithMany(er => er.MerchantRequests)
            .HasForeignKey(er => er.MerchantID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EligibilityRequest>()
            .HasOne(er => er.Service) // Relationship with Service
            .WithMany(s => s.EligibilityRequests)
            .HasForeignKey(er => er.ServiceID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EligibilityRequest>()
            .HasOne(er => er.Status) // Relationship with RequestStatuses
            .WithMany(rs => rs.EligibilityRequests)
            .HasForeignKey(er => er.StatusID)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_EligibilityRequest_StatusID");

        // EligibilityDocument Relationships
        modelBuilder.Entity<EligibilityDocument>()
            .HasKey(ed => new { ed.DocumentID });
        //.HasKey(ed => new { ed.RequestID, ed.FileID });

        modelBuilder.Entity<EligibilityDocument>()
            .HasOne(ed => ed.EligibilityRequest)
            .WithMany(er => er.EligibilityDocuments)
            .HasForeignKey(ed => ed.RequestID);

        modelBuilder.Entity<EligibilityDocument>()
            .HasOne(ed => ed.File)
            .WithMany()
            .HasForeignKey(ed => ed.FileID);

        modelBuilder.Entity<MembershipPlanSlabs>()
       .HasKey(mps => new { mps.MembershipPlanId, mps.SlabId });
        modelBuilder.Entity<MiniDashboardResult>().HasNoKey().ToView(null);
        modelBuilder.Entity<TotalRevenueResult>().HasNoKey().ToView(null);
        modelBuilder.Entity<GetRecentTransaction>().HasNoKey().ToView(null);
        modelBuilder.Entity<MiniDashBoardData>().HasNoKey().ToView(null);
        modelBuilder.Entity<ReviewSummaryDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<DocumentInfo>().HasNoKey().ToView(null);
        modelBuilder.Entity<ReviewViewModel>().HasNoKey().ToView(null);
        modelBuilder.Entity<MembershipPlanSlabs>()
            .HasOne(mps => mps.MembershipPlan)
            .WithMany(mp => mp.MembershipPlanSlabs)
            .HasForeignKey(mps => mps.MembershipPlanId);

        modelBuilder.Entity<MembershipPlanSlabs>()
            .HasOne(mps => mps.Slab)
            .WithMany(s => s.MembershipPlanSlabs)
            .HasForeignKey(mps => mps.SlabId);
        modelBuilder.Entity<BankTransferAccount>()
            .Property(b => b.Id)
            .ValueGeneratedOnAdd(); // Ensure EF knows it's identity
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
