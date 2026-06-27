using Kharbarchi.Server.Models;
using Kharbarchi.Shared.Models;
using Kharbarchi.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Data;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Commodity> Commodities => Set<Commodity>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();
    public DbSet<ProductProductTag> ProductProductTags => Set<ProductProductTag>();
    public DbSet<ProductSpecDefinition> ProductSpecDefinitions => Set<ProductSpecDefinition>();
    public DbSet<ProductSpecValue> ProductSpecValues => Set<ProductSpecValue>();
    public DbSet<PriceProposal> PriceProposals => Set<PriceProposal>();
    public DbSet<InventoryProposal> InventoryProposals => Set<InventoryProposal>();
    public DbSet<ApprovalAuditLog> ApprovalAuditLogs => Set<ApprovalAuditLog>();
    public DbSet<SyncOutboxMessage> SyncOutboxMessages => Set<SyncOutboxMessage>();
    public DbSet<WooCommerceSyncLog> WooCommerceSyncLogs => Set<WooCommerceSyncLog>();
    public DbSet<GatewayPaymentReceipt> GatewayPaymentReceipts => Set<GatewayPaymentReceipt>();
    public DbSet<WooCommerceOrderSnapshot> WooCommerceOrderSnapshots => Set<WooCommerceOrderSnapshot>();
    public DbSet<WooCommerceOrderItemSnapshot> WooCommerceOrderItemSnapshots => Set<WooCommerceOrderItemSnapshot>();
    public DbSet<BarookPaymentSession> BarookPaymentSessions => Set<BarookPaymentSession>();
    public DbSet<ManualPaymentReceipt> ManualPaymentReceipts => Set<ManualPaymentReceipt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureIdentity(modelBuilder);
        ConfigureCatalog(modelBuilder);
        ConfigureStore(modelBuilder);
        ConfigureWorkflow(modelBuilder);
        ConfigureWooCommerceSupport(modelBuilder);
        ConfigureOrderWorkflow(modelBuilder);
        SeedBaseData(modelBuilder);
    }

    private static void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>().ToTable("sec_AspNetUsers");
        modelBuilder.Entity<IdentityRole>().ToTable("sec_AspNetRoles");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("sec_AspNetUserRoles");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("sec_AspNetUserClaims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("sec_AspNetUserLogins");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("sec_AspNetRoleClaims");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("sec_AspNetUserTokens");
    }

    private static void ConfigureCatalog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("gnr_Brands");
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(180).IsRequired();
            entity.Property(x => x.LogoUrl).HasMaxLength(1000);
            entity.HasIndex(x => x.WooCommerceBrandId).IsUnique();
        });

        modelBuilder.Entity<Commodity>(entity =>
        {
            entity.ToTable("gnr_Commodities");
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(200).IsRequired();
            entity.Property(x => x.EnglishName).HasMaxLength(120);
            entity.Property(x => x.Description).HasMaxLength(1000);
        });

        modelBuilder.Entity<ProductTag>(entity =>
        {
            entity.ToTable("gnr_ProductTags");
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(140).IsRequired();
        });

        modelBuilder.Entity<ProductProductTag>(entity =>
        {
            entity.ToTable("gnr_ProductProductTags");
            entity.HasKey(x => new { x.ProductId, x.ProductTagId });
            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductTags)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProductTag)
                .WithMany(x => x.ProductTags)
                .HasForeignKey(x => x.ProductTagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductSpecDefinition>(entity =>
        {
            entity.ToTable("gnr_ProductSpecDefinitions");
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(140).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Unit).HasMaxLength(40);
        });

        modelBuilder.Entity<ProductSpecValue>(entity =>
        {
            entity.ToTable("gnr_ProductSpecValues");
            entity.HasIndex(x => new { x.ProductId, x.SpecDefinitionId }).IsUnique();
            entity.Property(x => x.Value).HasMaxLength(500).IsRequired();
            entity.HasOne(x => x.Product)
                .WithMany(x => x.SpecValues)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.SpecDefinition)
                .WithMany(x => x.Values)
                .HasForeignKey(x => x.SpecDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureStore(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("gnr_Categories");
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(180).IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("gnr_Products");
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.HasIndex(x => x.WooCommerceProductId).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(280).IsRequired();
            entity.Property(x => x.Sku).HasMaxLength(120);
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.ImageUrl).HasMaxLength(1000);
            entity.Property(x => x.GalleryJson).HasColumnType("longtext");
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.OldPrice).HasPrecision(18, 2);
            entity.Property(x => x.PurchasePrice).HasPrecision(18, 2);

            entity.HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.Commodity)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CommodityId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(p => p.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.ToTable("gnr_ProductVariants");
            entity.HasIndex(x => new { x.ProductId, x.Name }).IsUnique();
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.HasIndex(x => new { x.WooCommerceProductId, x.WooCommerceVariationId }).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Sku).HasMaxLength(120);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.OldPrice).HasPrecision(18, 2);
            entity.Property(x => x.PurchasePrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("cbi_Customers");
            entity.Property(x => x.FullName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(320);
            entity.Property(x => x.AddressLine).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.City).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PostalCode).HasMaxLength(30);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("com_Orders");
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PaymentMethod).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PaymentStatus).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PaymentReference).HasMaxLength(160);
            entity.Property(x => x.GatewayName).HasMaxLength(80);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.WooCommerceOrderId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("com_OrderItems");
            entity.Property(x => x.VariantName).HasMaxLength(150);
            entity.Property(x => x.Sku).HasMaxLength(120);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
        });
    }

    private static void ConfigureWorkflow(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PriceProposal>(entity =>
        {
            entity.ToTable("wf_PriceProposals");
            entity.Property(x => x.CurrentSalePrice).HasPrecision(18, 2);
            entity.Property(x => x.ProposedSalePrice).HasPrecision(18, 2);
            entity.Property(x => x.CurrentPurchasePrice).HasPrecision(18, 2);
            entity.Property(x => x.ProposedPurchasePrice).HasPrecision(18, 2);
            entity.Property(x => x.CreatedByUserName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ManagerApprovedByUserName).HasMaxLength(256);
            entity.Property(x => x.SuperAdminApprovedByUserName).HasMaxLength(256);
            entity.Property(x => x.RejectedByUserName).HasMaxLength(256);
            entity.Property(x => x.Reason).HasMaxLength(1000);
            entity.Property(x => x.ManagerNote).HasMaxLength(1000);
            entity.Property(x => x.SuperAdminNote).HasMaxLength(1000);
            entity.Property(x => x.RejectionReason).HasMaxLength(1000);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.CreatedAtUtc);
        });

        modelBuilder.Entity<InventoryProposal>(entity =>
        {
            entity.ToTable("wf_InventoryProposals");
            entity.Property(x => x.CreatedByUserName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ManagerApprovedByUserName).HasMaxLength(256);
            entity.Property(x => x.SuperAdminApprovedByUserName).HasMaxLength(256);
            entity.Property(x => x.RejectedByUserName).HasMaxLength(256);
            entity.Property(x => x.Reason).HasMaxLength(1000);
            entity.Property(x => x.ManagerNote).HasMaxLength(1000);
            entity.Property(x => x.SuperAdminNote).HasMaxLength(1000);
            entity.Property(x => x.RejectionReason).HasMaxLength(1000);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.CreatedAtUtc);
        });

        modelBuilder.Entity<ApprovalAuditLog>(entity =>
        {
            entity.ToTable("wf_ApprovalAuditLogs");
            entity.Property(x => x.EntityType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(80).IsRequired();
            entity.Property(x => x.UserName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.UserRole).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Note).HasMaxLength(1000);
            entity.HasIndex(x => new { x.EntityType, x.EntityId });
            entity.HasIndex(x => x.CreatedAtUtc);
        });

        modelBuilder.Entity<SyncOutboxMessage>(entity =>
        {
            entity.ToTable("sync_OutboxMessages");
            entity.Property(x => x.EventType).HasMaxLength(120).IsRequired();
            entity.Property(x => x.AggregateType).HasMaxLength(120).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("longtext").IsRequired();
            entity.Property(x => x.QueuedByUserName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SourceWorkflow).HasMaxLength(120);
            entity.Property(x => x.LastError).HasMaxLength(1000);
            entity.Property(x => x.LockedBy).HasMaxLength(256);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.CreatedAtUtc);
            entity.HasIndex(x => new { x.AggregateType, x.AggregateId });
        });
    }

    private static void ConfigureWooCommerceSupport(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WooCommerceSyncLog>(entity =>
        {
            entity.ToTable("sup_WooCommerceSyncLogs");
            entity.Property(x => x.Operation).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.EntityType).HasMaxLength(80);
            entity.Property(x => x.RequestHash).HasMaxLength(128);
            entity.Property(x => x.Message).HasMaxLength(2000);
            entity.Property(x => x.PerformedByUserName).HasMaxLength(256);
            entity.HasIndex(x => x.CreatedAtUtc);
            entity.HasIndex(x => new { x.Operation, x.Status });
        });

        modelBuilder.Entity<GatewayPaymentReceipt>(entity =>
        {
            entity.ToTable("sup_GatewayPaymentReceipts");
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.GatewayName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.TransactionId).HasMaxLength(160).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PaymentStatus).HasMaxLength(50).IsRequired();
            entity.Property(x => x.GatewayRawStatus).HasMaxLength(100);
            entity.Property(x => x.Note).HasMaxLength(1000);
            entity.Property(x => x.RequestedByUserName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.WooCommerceResponseSummary).HasMaxLength(2000);
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasIndex(x => x.TransactionId).IsUnique();
            entity.HasIndex(x => x.WooCommerceOrderId);
        });
    }


    private static void ConfigureOrderWorkflow(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WooCommerceOrderSnapshot>(entity =>
        {
            entity.ToTable("ord_WooCommerceOrderSnapshots");
            entity.HasIndex(x => x.WooCommerceOrderId).IsUnique();
            entity.HasIndex(x => x.WooCommerceOrderNumber);
            entity.HasIndex(x => x.WooCommerceStatus);
            entity.HasIndex(x => x.InternalStatus);
            entity.HasIndex(x => x.PaymentStatus);
            entity.HasIndex(x => x.CustomerPhone);
            entity.HasIndex(x => x.CustomerNationalCode);
            entity.HasIndex(x => x.WooCreatedAtUtc);
            entity.Property(x => x.WooCommerceOrderNumber).HasMaxLength(80).IsRequired();
            entity.Property(x => x.WooCommerceStatus).HasMaxLength(80).IsRequired();
            entity.Property(x => x.InternalStatus).HasMaxLength(80).IsRequired();
            entity.Property(x => x.PaymentStatus).HasMaxLength(80).IsRequired();
            entity.Property(x => x.PaymentMethod).HasMaxLength(120);
            entity.Property(x => x.PaymentMethodTitle).HasMaxLength(200);
            entity.Property(x => x.TransactionId).HasMaxLength(160);
            entity.Property(x => x.Currency).HasMaxLength(8);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.ShippingTotal).HasPrecision(18, 2);
            entity.Property(x => x.DiscountTotal).HasPrecision(18, 2);
            entity.Property(x => x.CustomerFullName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.CustomerPhone).HasMaxLength(40);
            entity.Property(x => x.CustomerEmail).HasMaxLength(320);
            entity.Property(x => x.CustomerNationalCode).HasMaxLength(20);
            entity.Property(x => x.BillingAddress).HasMaxLength(1500);
            entity.Property(x => x.ShippingAddress).HasMaxLength(1500);
            entity.Property(x => x.CustomerNote).HasMaxLength(2000);
            entity.Property(x => x.LastActionByUserName).HasMaxLength(256);
            entity.Property(x => x.LastActionNote).HasMaxLength(1000);
            entity.Property(x => x.RawJson).HasColumnType("longtext");

            entity.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.WooCommerceOrderSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WooCommerceOrderItemSnapshot>(entity =>
        {
            entity.ToTable("ord_WooCommerceOrderItemSnapshots");
            entity.HasIndex(x => new { x.WooCommerceOrderSnapshotId, x.WooCommerceLineItemId }).IsUnique();
            entity.HasIndex(x => x.WooCommerceProductId);
            entity.HasIndex(x => x.WooCommerceVariationId);
            entity.Property(x => x.Sku).HasMaxLength(120);
            entity.Property(x => x.Name).HasMaxLength(300).IsRequired();
            entity.Property(x => x.UnitType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.Property(x => x.RawJson).HasColumnType("longtext");
        });

        modelBuilder.Entity<BarookPaymentSession>(entity =>
        {
            entity.ToTable("pay_BarookPaymentSessions");
            entity.HasIndex(x => x.ExternalCode).IsUnique();
            entity.HasIndex(x => x.Token);
            entity.HasIndex(x => x.WooCommerceOrderId);
            entity.HasIndex(x => x.BarookStatus);
            entity.Property(x => x.ExternalCode).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.Token).HasMaxLength(200);
            entity.Property(x => x.PaymentUrl).HasMaxLength(1000);
            entity.Property(x => x.BarookStatus).HasMaxLength(80);
            entity.Property(x => x.ReferenceNumber).HasMaxLength(160);
            entity.Property(x => x.MaskedCardNumber).HasMaxLength(40);
            entity.Property(x => x.TransactionId).HasMaxLength(160);
            entity.Property(x => x.CreatedByUserName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.VerifiedByUserName).HasMaxLength(256);
            entity.Property(x => x.StartRequestJson).HasColumnType("longtext");
            entity.Property(x => x.StartResponseJson).HasColumnType("longtext");
            entity.Property(x => x.VerifyResponseJson).HasColumnType("longtext");
            entity.Property(x => x.LastError).HasMaxLength(2000);
            entity.HasOne(x => x.Order)
                .WithMany(x => x.BarookPayments)
                .HasForeignKey(x => x.WooCommerceOrderSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ManualPaymentReceipt>(entity =>
        {
            entity.ToTable("pay_ManualPaymentReceipts");
            entity.HasIndex(x => x.ReceiptNumber).IsUnique();
            entity.HasIndex(x => x.WooCommerceOrderId);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.ReceiptNumber).HasMaxLength(160).IsRequired();
            entity.Property(x => x.PaymentSource).HasMaxLength(80).IsRequired();
            entity.Property(x => x.RegisteredByUserName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Note).HasMaxLength(1000);
            entity.Property(x => x.WooCommerceResponseSummary).HasMaxLength(2000);
            entity.HasOne(x => x.Order)
                .WithMany(x => x.ManualReceipts)
                .HasForeignKey(x => x.WooCommerceOrderSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void SeedBaseData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "لوبیا", Slug = "beans" },
            new Category { Id = 2, Name = "عدس", Slug = "lentils" },
            new Category { Id = 3, Name = "نخود", Slug = "chickpeas" });

        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "f517b79d-1fc4-4800-bcb8-ee0ca67dce1e", Name = KharbarchiRoles.LegacyAdmin, NormalizedName = "ADMIN" },
            new IdentityRole { Id = "4f43b487-3f8e-426d-9a46-048c7d07f7f9", Name = KharbarchiRoles.SuperAdmin, NormalizedName = "SUPERADMIN" },
            new IdentityRole { Id = "b1477f6c-54ef-48d0-b24c-756b3a83b1a1", Name = KharbarchiRoles.PricingManager, NormalizedName = "PRICINGMANAGER" },
            new IdentityRole { Id = "5f36c2f9-330a-492c-8ebf-65141782f2bb", Name = KharbarchiRoles.PricingEmployee, NormalizedName = "PRICINGEMPLOYEE" },
            new IdentityRole { Id = "e5ac8272-7f9f-47c0-8e21-040fe3d242ed", Name = KharbarchiRoles.WarehouseEmployee, NormalizedName = "WAREHOUSEEMPLOYEE" },
            new IdentityRole { Id = "9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c", Name = KharbarchiRoles.CentralSyncAgent, NormalizedName = "CENTRALSYNCAGENT" },
            new IdentityRole { Id = "6240e185-5c3a-410b-99d3-9767571fdf24", Name = KharbarchiRoles.Customer, NormalizedName = "CUSTOMER" },
            new IdentityRole { Id = "67320cb2-92a2-4de7-971b-7e9e80244f4b", Name = KharbarchiRoles.GatewayAdmin, NormalizedName = "GATEWAYADMIN" },
            new IdentityRole { Id = "0c5e0418-46b3-4c6e-887e-0c182171ab11", Name = KharbarchiRoles.SalesManager, NormalizedName = "SALESMANAGER" },
            new IdentityRole { Id = "e572b070-82bd-47f0-b486-cc1b644b2d9e", Name = KharbarchiRoles.ShippingOrderManager, NormalizedName = "SHIPPINGORDERMANAGER" },
            new IdentityRole { Id = "e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52", Name = KharbarchiRoles.Accountant, NormalizedName = "ACCOUNTANT" });
    }
}
