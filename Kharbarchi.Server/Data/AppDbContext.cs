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
    public DbSet<CustomerCreditHistory> CustomerCreditHistory => Set<CustomerCreditHistory>();
    public DbSet<ProductPriceHistory> ProductPriceHistory => Set<ProductPriceHistory>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductWooControlProfile> ProductWooControlProfiles => Set<ProductWooControlProfile>();
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
    public DbSet<AllProductWithProcess> AllProductsWithProcess => Set<AllProductWithProcess>();
    public DbSet<KhbProductMainGroup> KhbProductMainGroups => Set<KhbProductMainGroup>();
    public DbSet<KhbSaleProduct> KhbSaleProducts => Set<KhbSaleProduct>();
    public DbSet<KhbSourceProduct> KhbSourceProducts => Set<KhbSourceProduct>();
    public DbSet<KhbCategoryMap> KhbCategoryMaps => Set<KhbCategoryMap>();
    public DbSet<KhbCommodity> KhbCommodities => Set<KhbCommodity>();
    public DbSet<KhbPackageType> KhbPackageTypes => Set<KhbPackageType>();
    public DbSet<KhbProductFinal> KhbProductFinals => Set<KhbProductFinal>();
    public DbSet<KhbProductUpdateQueue> KhbProductUpdateQueue => Set<KhbProductUpdateQueue>();
    public DbSet<KhbProductPriceHistory> KhbProductPriceHistory => Set<KhbProductPriceHistory>();
    public DbSet<KhbProductChangeLog> KhbProductChangeLogs => Set<KhbProductChangeLog>();
    public DbSet<KhbImportedWooCommerceRecord> KhbImportedWooCommerceRecords => Set<KhbImportedWooCommerceRecord>();
    public DbSet<KhbWorkflowJob> KhbWorkflowJobs => Set<KhbWorkflowJob>();
    public DbSet<KhbWorkflowJobLog> KhbWorkflowJobLogs => Set<KhbWorkflowJobLog>();
    public DbSet<WooCommerceConnectionProfile> WooCommerceConnectionProfiles => Set<WooCommerceConnectionProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureIdentity(modelBuilder);
        ConfigureCatalog(modelBuilder);
        ConfigureStore(modelBuilder);
        ConfigureWorkflow(modelBuilder);
        ConfigureWooCommerceSupport(modelBuilder);
        ConfigureOrderWorkflow(modelBuilder);
        modelBuilder.ConfigureKharbarchiWorkflow();
        SeedBaseData(modelBuilder);
    }

    private static void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>().ToTable("sec_aspnetusers");
        modelBuilder.Entity<IdentityRole>().ToTable("sec_aspnetroles");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("sec_aspnetuserroles");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("sec_aspnetuserclaims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("sec_aspnetuserlogins");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("sec_aspnetroleclaims");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("sec_aspnetusertokens");
    }

    private static void ConfigureCatalog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("gnr_brands");
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(180).IsRequired();
            entity.Property(x => x.LogoUrl).HasMaxLength(1000);
            entity.HasIndex(x => x.WooCommerceBrandId).IsUnique();
        });

        modelBuilder.Entity<Commodity>(entity =>
        {
            entity.ToTable("gnr_commodities");
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(200).IsRequired();
            entity.Property(x => x.EnglishName).HasMaxLength(120);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.HasIndex(x => x.WooCommerceCommodityId).IsUnique();
        });

        modelBuilder.Entity<ProductTag>(entity =>
        {
            entity.ToTable("gnr_producttags");
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(140).IsRequired();
        });

        modelBuilder.Entity<ProductProductTag>(entity =>
        {
            entity.ToTable("gnr_productproducttags");
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
            entity.ToTable("gnr_productspecdefinitions");
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(140).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Unit).HasMaxLength(40);
        });

        modelBuilder.Entity<ProductSpecValue>(entity =>
        {
            entity.ToTable("gnr_productspecvalues");
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
            entity.ToTable("gnr_categories");
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => x.WooCommerceCategoryId).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(180).IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("gnr_products");
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

            entity.HasOne(p => p.WooControlProfile)
                .WithOne(p => p.Product)
                .HasForeignKey<ProductWooControlProfile>(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.ToTable("gnr_productvariants");
            entity.HasIndex(x => new { x.ProductId, x.Name }).IsUnique();
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.HasIndex(x => new { x.WooCommerceProductId, x.WooCommerceVariationId }).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Sku).HasMaxLength(120);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.OldPrice).HasPrecision(18, 2);
            entity.Property(x => x.PurchasePrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ProductWooControlProfile>(entity =>
        {
            entity.ToTable("khb_productwoocontrolprofiles");
            entity.HasIndex(x => x.ProductId).IsUnique();
            entity.HasIndex(x => x.PriceCheckStatus);
            entity.HasIndex(x => x.WooSyncStatus);
            entity.Property(x => x.PriceSourceMode).HasMaxLength(80).IsRequired();
            entity.Property(x => x.PackageGroup).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PackageCode).HasMaxLength(80);
            entity.Property(x => x.PackageTitle).HasMaxLength(300);
            entity.Property(x => x.ImageTag).HasMaxLength(300);
            entity.Property(x => x.SaleUnit).HasMaxLength(50).IsRequired();
            entity.Property(x => x.WoodmartPriceUnitOfMeasure).HasMaxLength(80).IsRequired();
            entity.Property(x => x.PriceCheckStatus).HasMaxLength(20).IsRequired();
            entity.Property(x => x.PriceCheckCode).HasMaxLength(120).IsRequired();
            entity.Property(x => x.PriceCheckNote).HasMaxLength(2000);
            entity.Property(x => x.WooSyncStatus).HasMaxLength(50).IsRequired();
            entity.Property(x => x.WooLastError).HasMaxLength(2000);
            entity.Property(x => x.UnitWeightKg).HasPrecision(18, 6);
            entity.Property(x => x.BulkWeightKg).HasPrecision(18, 6);
            entity.Property(x => x.MinPurchaseKg).HasPrecision(18, 6);
            entity.Property(x => x.SaleCashPrice).HasPrecision(18, 2);
            entity.Property(x => x.SaleCreditPrice).HasPrecision(18, 2);
            entity.Property(x => x.BuyCashPrice).HasPrecision(18, 2);
            entity.Property(x => x.BuyCreditPrice).HasPrecision(18, 2);
            entity.Property(x => x.SaleCashPricePerKg).HasPrecision(18, 2);
            entity.Property(x => x.SaleCreditPricePerKg).HasPrecision(18, 2);
            entity.Property(x => x.BuyCashPricePerKg).HasPrecision(18, 2);
            entity.Property(x => x.BuyCreditPricePerKg).HasPrecision(18, 2);
            entity.Property(x => x.ExpectedSaleCreditPrice).HasPrecision(18, 2);
            entity.Property(x => x.ExpectedSaleCashPrice).HasPrecision(18, 2);
            entity.Property(x => x.ExpectedBuyCreditPrice).HasPrecision(18, 2);
            entity.Property(x => x.ExpectedBuyCashPrice).HasPrecision(18, 2);
            entity.Property(x => x.SaleCreditDiff).HasPrecision(18, 2);
            entity.Property(x => x.SaleCashDiff).HasPrecision(18, 2);
            entity.Property(x => x.BuyCreditDiff).HasPrecision(18, 2);
            entity.Property(x => x.BuyCashDiff).HasPrecision(18, 2);
            entity.Property(x => x.PriceCheckPercent).HasPrecision(9, 4);
            entity.Property(x => x.PriceCheckAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("cbi_customers");
            entity.HasIndex(x => x.LegalEntityId).IsUnique();
            entity.HasIndex(x => x.PhoneNumber);
            entity.Property(x => x.FullName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(320);
            entity.Property(x => x.AddressLine).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.City).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PostalCode).HasMaxLength(30);
            entity.Property(x => x.LegalEntityId).HasMaxLength(30);
            entity.Property(x => x.CreditLimit).HasPrecision(18, 2);
            entity.Property(x => x.UsedCredit).HasPrecision(18, 2);
            entity.Property(x => x.CreditPlanTitle).HasMaxLength(250);
            entity.Property(x => x.DistributionStatus).HasMaxLength(100);
        });

        modelBuilder.Entity<CustomerCreditHistory>(entity =>
        {
            entity.ToTable("cbi_customercredithistory");
            entity.HasIndex(x => new { x.CustomerId, x.ChangedAtUtc });
            entity.Property(x => x.PreviousCreditLimit).HasPrecision(18, 2);
            entity.Property(x => x.NewCreditLimit).HasPrecision(18, 2);
            entity.Property(x => x.Source).HasMaxLength(80).IsRequired();
            entity.Property(x => x.ChangedByUserName).HasMaxLength(256).IsRequired();
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.CreditHistory)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductPriceHistory>(entity =>
        {
            entity.ToTable("prc_productpricehistory");
            entity.HasIndex(x => new { x.ProductId, x.ProductVariantId, x.PriceType, x.IsCurrent });
            entity.Property(x => x.PriceType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Source).HasMaxLength(80).IsRequired();
            entity.Property(x => x.ChangedByUserName).HasMaxLength(256).IsRequired();
            entity.HasOne(x => x.Product)
                .WithMany(x => x.PriceHistory)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("com_orders");
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.GrossAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalDiscount).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PaymentMethod).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PaymentStatus).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PaymentReference).HasMaxLength(160);
            entity.Property(x => x.GatewayName).HasMaxLength(80);
            entity.Property(x => x.DeliveryAddressLine).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.DeliveryCity).HasMaxLength(150).IsRequired();
            entity.Property(x => x.DeliveryPostalCode).HasMaxLength(30);
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.CreatedByUserName).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.WooCommerceOrderId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("com_orderitems");
            entity.Property(x => x.VariantName).HasMaxLength(150);
            entity.Property(x => x.Sku).HasMaxLength(120);
            entity.Property(x => x.ProductName).HasMaxLength(300).IsRequired();
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.OriginalUnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.LineDiscount).HasPrecision(18, 2);
        });
    }

    private static void ConfigureWorkflow(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PriceProposal>(entity =>
        {
            entity.ToTable("wf_priceproposals");
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
            entity.ToTable("wf_inventoryproposals");
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
            entity.ToTable("wf_approvalauditlogs");
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
            entity.ToTable("sync_outboxmessages");
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
            entity.ToTable("sup_woocommercesynclogs");
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
            entity.ToTable("sup_gatewaypaymentreceipts");
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
            entity.ToTable("ord_woocommerceordersnapshots");
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
            entity.ToTable("ord_woocommerceorderitemsnapshots");
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
            entity.ToTable("pay_barookpaymentsessions");
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
            entity.ToTable("pay_manualpaymentreceipts");
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
            new IdentityRole { Id = "f517b79d-1fc4-4800-bcb8-ee0ca67dce1e", Name = KharbarchiRoles.LegacyAdmin, NormalizedName = "ADMIN", ConcurrencyStamp = "f517b79d-1fc4-4800-bcb8-ee0ca67dce1e" },
            new IdentityRole { Id = "4f43b487-3f8e-426d-9a46-048c7d07f7f9", Name = KharbarchiRoles.SuperAdmin, NormalizedName = "SUPERADMIN", ConcurrencyStamp = "4f43b487-3f8e-426d-9a46-048c7d07f7f9" },
            new IdentityRole { Id = "b1477f6c-54ef-48d0-b24c-756b3a83b1a1", Name = KharbarchiRoles.PricingManager, NormalizedName = "PRICINGMANAGER", ConcurrencyStamp = "b1477f6c-54ef-48d0-b24c-756b3a83b1a1" },
            new IdentityRole { Id = "5f36c2f9-330a-492c-8ebf-65141782f2bb", Name = KharbarchiRoles.PricingEmployee, NormalizedName = "PRICINGEMPLOYEE", ConcurrencyStamp = "5f36c2f9-330a-492c-8ebf-65141782f2bb" },
            new IdentityRole { Id = "e5ac8272-7f9f-47c0-8e21-040fe3d242ed", Name = KharbarchiRoles.WarehouseEmployee, NormalizedName = "WAREHOUSEEMPLOYEE", ConcurrencyStamp = "e5ac8272-7f9f-47c0-8e21-040fe3d242ed" },
            new IdentityRole { Id = "9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c", Name = KharbarchiRoles.CentralSyncAgent, NormalizedName = "CENTRALSYNCAGENT", ConcurrencyStamp = "9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c" },
            new IdentityRole { Id = "6240e185-5c3a-410b-99d3-9767571fdf24", Name = KharbarchiRoles.Customer, NormalizedName = "CUSTOMER", ConcurrencyStamp = "6240e185-5c3a-410b-99d3-9767571fdf24" },
            new IdentityRole { Id = "67320cb2-92a2-4de7-971b-7e9e80244f4b", Name = KharbarchiRoles.GatewayAdmin, NormalizedName = "GATEWAYADMIN", ConcurrencyStamp = "67320cb2-92a2-4de7-971b-7e9e80244f4b" },
            new IdentityRole { Id = "0c5e0418-46b3-4c6e-887e-0c182171ab11", Name = KharbarchiRoles.SalesManager, NormalizedName = "SALESMANAGER", ConcurrencyStamp = "0c5e0418-46b3-4c6e-887e-0c182171ab11" },
            new IdentityRole { Id = "b52865dd-a178-45a1-82ae-59f2e75c9c17", Name = KharbarchiRoles.Seller, NormalizedName = "SELLER", ConcurrencyStamp = "b52865dd-a178-45a1-82ae-59f2e75c9c17" },
            new IdentityRole { Id = "e572b070-82bd-47f0-b486-cc1b644b2d9e", Name = KharbarchiRoles.ShippingOrderManager, NormalizedName = "SHIPPINGORDERMANAGER", ConcurrencyStamp = "e572b070-82bd-47f0-b486-cc1b644b2d9e" },
            new IdentityRole { Id = "e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52", Name = KharbarchiRoles.Accountant, NormalizedName = "ACCOUNTANT", ConcurrencyStamp = "e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52" });
    }
}
