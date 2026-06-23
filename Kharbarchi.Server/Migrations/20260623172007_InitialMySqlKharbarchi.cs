using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMySqlKharbarchi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cbi_Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FullName = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: true),
                    AddressLine = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    City = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    PostalCode = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cbi_Customers", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gnr_Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    Slug = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    LogoUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WooCommerceBrandId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gnr_Brands", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gnr_Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gnr_Categories", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gnr_Commodities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    Slug = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    EnglishName = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gnr_Commodities", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gnr_ProductSpecDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(140)", maxLength: 140, nullable: false),
                    Slug = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    Unit = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gnr_ProductSpecDefinitions", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gnr_ProductTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "varchar(140)", maxLength: 140, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gnr_ProductTags", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ord_WooCommerceOrderSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WooCommerceOrderId = table.Column<long>(type: "bigint", nullable: false),
                    WooCommerceOrderNumber = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    WooCommerceStatus = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    InternalStatus = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    PaymentStatus = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    PaymentMethod = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    PaymentMethodTitle = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    TransactionId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true),
                    Currency = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CustomerFullName = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    CustomerPhone = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    CustomerEmail = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: true),
                    CustomerNationalCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    BillingAddress = table.Column<string>(type: "varchar(1500)", maxLength: 1500, nullable: true),
                    ShippingAddress = table.Column<string>(type: "varchar(1500)", maxLength: 1500, nullable: true),
                    CustomerNote = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    WooCreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WooUpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SyncedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastPaymentCheckedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReadyToShipAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastActionByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    LastActionNote = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    RawJson = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ord_WooCommerceOrderSnapshots", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sec_AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_AspNetRoles", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sec_AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    FullName = table.Column<string>(type: "longtext", nullable: true),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true),
                    SecurityStamp = table.Column<string>(type: "longtext", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_AspNetUsers", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sup_GatewayPaymentReceipts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WooCommerceOrderId = table.Column<long>(type: "bigint", nullable: false),
                    LocalOrderId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    GatewayName = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    TransactionId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    PaymentStatus = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    GatewayRawStatus = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    RequestedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    SentToWooCommerce = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SentToWooCommerceAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WooCommerceResponseSummary = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sup_GatewayPaymentReceipts", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sup_WooCommerceSyncLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Operation = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    EntityType = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    LocalEntityId = table.Column<int>(type: "int", nullable: true),
                    WooCommerceEntityId = table.Column<long>(type: "bigint", nullable: true),
                    RequestHash = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    Message = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    PerformedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sup_WooCommerceSyncLogs", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sync_OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    EventType = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    AggregateType = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    AggregateId = table.Column<long>(type: "bigint", nullable: false),
                    PayloadJson = table.Column<string>(type: "longtext", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    QueuedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    SourceWorkflow = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    LastError = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    LockedBy = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LockedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SentAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_OutboxMessages", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "wf_ApprovalAuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    EntityType = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    EntityId = table.Column<long>(type: "bigint", nullable: false),
                    Action = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    UserRole = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Note = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wf_ApprovalAuditLogs", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "com_Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PaymentMethod = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PaymentStatus = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PaymentReference = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true),
                    GatewayName = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WooCommerceOrderId = table.Column<long>(type: "bigint", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_com_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_com_Orders_cbi_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "cbi_Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gnr_Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    Slug = table.Column<string>(type: "varchar(280)", maxLength: 280, nullable: false),
                    Description = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    IsAvailable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: true),
                    CommodityId = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    GalleryJson = table.Column<string>(type: "longtext", nullable: true),
                    Sku = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OldPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    MinStockAlertQuantity = table.Column<int>(type: "int", nullable: true),
                    WooCommerceProductId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gnr_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gnr_Products_gnr_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "gnr_Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_gnr_Products_gnr_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "gnr_Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gnr_Products_gnr_Commodities_CommodityId",
                        column: x => x.CommodityId,
                        principalTable: "gnr_Commodities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ord_WooCommerceOrderItemSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WooCommerceOrderSnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    WooCommerceLineItemId = table.Column<long>(type: "bigint", nullable: false),
                    WooCommerceProductId = table.Column<long>(type: "bigint", nullable: true),
                    WooCommerceVariationId = table.Column<long>(type: "bigint", nullable: true),
                    Sku = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    Name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    UnitType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RawJson = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ord_WooCommerceOrderItemSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ord_WooCommerceOrderItemSnapshots_ord_WooCommerceOrderSnapsh~",
                        column: x => x.WooCommerceOrderSnapshotId,
                        principalTable: "ord_WooCommerceOrderSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pay_BarookPaymentSessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WooCommerceOrderSnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    WooCommerceOrderId = table.Column<long>(type: "bigint", nullable: false),
                    ExternalCode = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    TokenExpireDateUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PaymentUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    LinkSentAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    BarookStatus = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true),
                    MaskedCardNumber = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    TransactionId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    VerifiedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    VerifiedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    StartRequestJson = table.Column<string>(type: "longtext", nullable: true),
                    StartResponseJson = table.Column<string>(type: "longtext", nullable: true),
                    VerifyResponseJson = table.Column<string>(type: "longtext", nullable: true),
                    LastError = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pay_BarookPaymentSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pay_BarookPaymentSessions_ord_WooCommerceOrderSnapshots_WooC~",
                        column: x => x.WooCommerceOrderSnapshotId,
                        principalTable: "ord_WooCommerceOrderSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pay_ManualPaymentReceipts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WooCommerceOrderSnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    WooCommerceOrderId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    ReceiptNumber = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    PaymentSource = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RegisteredByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    Note = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    SentToWooCommerce = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SentToWooCommerceAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WooCommerceResponseSummary = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pay_ManualPaymentReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pay_ManualPaymentReceipts_ord_WooCommerceOrderSnapshots_WooC~",
                        column: x => x.WooCommerceOrderSnapshotId,
                        principalTable: "ord_WooCommerceOrderSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sec_AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sec_AspNetRoleClaims_sec_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "sec_AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sec_AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sec_AspNetUserClaims_sec_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "sec_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sec_AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false),
                    ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_sec_AspNetUserLogins_sec_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "sec_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sec_AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_sec_AspNetUserRoles_sec_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "sec_AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sec_AspNetUserRoles_sec_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "sec_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sec_AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sec_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_sec_AspNetUserTokens_sec_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "sec_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gnr_ProductProductTags",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductTagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gnr_ProductProductTags", x => new { x.ProductId, x.ProductTagId });
                    table.ForeignKey(
                        name: "FK_gnr_ProductProductTags_gnr_ProductTags_ProductTagId",
                        column: x => x.ProductTagId,
                        principalTable: "gnr_ProductTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gnr_ProductProductTags_gnr_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "gnr_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gnr_ProductSpecValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SpecDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gnr_ProductSpecValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gnr_ProductSpecValues_gnr_ProductSpecDefinitions_SpecDefinit~",
                        column: x => x.SpecDefinitionId,
                        principalTable: "gnr_ProductSpecDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_gnr_ProductSpecValues_gnr_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "gnr_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "gnr_ProductVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Sku = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OldPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    MinStockAlertQuantity = table.Column<int>(type: "int", nullable: true),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsAvailable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WooCommerceProductId = table.Column<long>(type: "bigint", nullable: true),
                    WooCommerceVariationId = table.Column<long>(type: "bigint", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gnr_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gnr_ProductVariants_gnr_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "gnr_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "com_OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    VariantName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    Sku = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    WooCommerceProductId = table.Column<long>(type: "bigint", nullable: true),
                    WooCommerceVariationId = table.Column<long>(type: "bigint", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_com_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_com_OrderItems_com_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "com_Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_com_OrderItems_gnr_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "gnr_ProductVariants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_com_OrderItems_gnr_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "gnr_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "wf_InventoryProposals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    CurrentStockQuantity = table.Column<int>(type: "int", nullable: false),
                    ProposedQuantity = table.Column<int>(type: "int", nullable: false),
                    FinalStockQuantity = table.Column<int>(type: "int", nullable: false),
                    AdjustmentKind = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    ManagerApprovedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    SuperAdminApprovedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    RejectedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    Reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    ManagerNote = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    SuperAdminNote = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ManagerApprovedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SuperAdminApprovedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RejectedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    QueuedForSyncAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SyncedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wf_InventoryProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_wf_InventoryProposals_gnr_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "gnr_ProductVariants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_wf_InventoryProposals_gnr_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "gnr_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "wf_PriceProposals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    CurrentSalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProposedSalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentPurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ProposedPurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    ManagerApprovedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    SuperAdminApprovedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    RejectedByUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    Reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    ManagerNote = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    SuperAdminNote = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ManagerApprovedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SuperAdminApprovedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RejectedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    QueuedForSyncAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SyncedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wf_PriceProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_wf_PriceProposals_gnr_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "gnr_ProductVariants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_wf_PriceProposals_gnr_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "gnr_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "gnr_Categories",
                columns: new[] { "Id", "Name", "Slug" },
                values: new object[,]
                {
                    { 1, "لوبیا", "beans" },
                    { 2, "عدس", "lentils" },
                    { 3, "نخود", "chickpeas" }
                });

            migrationBuilder.InsertData(
                table: "sec_AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0c5e0418-46b3-4c6e-887e-0c182171ab11", "d8b522b7-bf1b-4acb-ab32-dac6ee529d90", "SalesManager", "SALESMANAGER" },
                    { "4f43b487-3f8e-426d-9a46-048c7d07f7f9", "4d2df8dd-8f24-4fb6-ac1f-4db646cfb66f", "SuperAdmin", "SUPERADMIN" },
                    { "5f36c2f9-330a-492c-8ebf-65141782f2bb", "d4a7dab0-72ec-4f45-ac7f-8dfcd9158b67", "PricingEmployee", "PRICINGEMPLOYEE" },
                    { "6240e185-5c3a-410b-99d3-9767571fdf24", "7d9e2b52-7518-49c7-ad91-0625deb5448a", "Customer", "CUSTOMER" },
                    { "67320cb2-92a2-4de7-971b-7e9e80244f4b", "fa971a2e-ce68-4314-87ca-92ad7835d7c4", "GatewayAdmin", "GATEWAYADMIN" },
                    { "9ab3d5a7-6722-42f7-9f3a-98bb62c44d1c", "e7a60e90-aae7-4a36-b60b-0307df4a3418", "CentralSyncAgent", "CENTRALSYNCAGENT" },
                    { "b1477f6c-54ef-48d0-b24c-756b3a83b1a1", "7c01a114-3ede-469f-83b3-9fe7e457d82a", "PricingManager", "PRICINGMANAGER" },
                    { "e572b070-82bd-47f0-b486-cc1b644b2d9e", "5a55800b-8979-479f-b45e-896e7f852248", "ShippingOrderManager", "SHIPPINGORDERMANAGER" },
                    { "e5ac8272-7f9f-47c0-8e21-040fe3d242ed", "a9735c29-6424-4ed0-af63-fdba24f66130", "WarehouseEmployee", "WAREHOUSEEMPLOYEE" },
                    { "e8d1a7c0-7763-4fc8-b2fa-1e0df03b8b52", "10c20816-f415-4315-bc76-89af7265626f", "Accountant", "ACCOUNTANT" },
                    { "f517b79d-1fc4-4800-bcb8-ee0ca67dce1e", "50819a95-8ece-4c30-9adb-4e4292ae1302", "Admin", "ADMIN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_com_OrderItems_OrderId",
                table: "com_OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_com_OrderItems_ProductId",
                table: "com_OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_com_OrderItems_ProductVariantId",
                table: "com_OrderItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_com_Orders_CreatedAt",
                table: "com_Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_com_Orders_CustomerId",
                table: "com_Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_com_Orders_WooCommerceOrderId",
                table: "com_Orders",
                column: "WooCommerceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_gnr_Brands_Slug",
                table: "gnr_Brands",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gnr_Brands_WooCommerceBrandId",
                table: "gnr_Brands",
                column: "WooCommerceBrandId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gnr_Categories_Slug",
                table: "gnr_Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gnr_Commodities_Slug",
                table: "gnr_Commodities",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gnr_ProductProductTags_ProductTagId",
                table: "gnr_ProductProductTags",
                column: "ProductTagId");

            migrationBuilder.CreateIndex(
                name: "IX_gnr_Products_BrandId",
                table: "gnr_Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_gnr_Products_CategoryId",
                table: "gnr_Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_gnr_Products_CommodityId",
                table: "gnr_Products",
                column: "CommodityId");

            migrationBuilder.CreateIndex(
                name: "IX_gnr_Products_Sku",
                table: "gnr_Products",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gnr_Products_Slug",
                table: "gnr_Products",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gnr_Products_WooCommerceProductId",
                table: "gnr_Products",
                column: "WooCommerceProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gnr_ProductSpecDefinitions_Slug",
                table: "gnr_ProductSpecDefinitions",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gnr_ProductSpecValues_ProductId_SpecDefinitionId",
                table: "gnr_ProductSpecValues",
                columns: new[] { "ProductId", "SpecDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gnr_ProductSpecValues_SpecDefinitionId",
                table: "gnr_ProductSpecValues",
                column: "SpecDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_gnr_ProductTags_Slug",
                table: "gnr_ProductTags",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gnr_ProductVariants_ProductId_Name",
                table: "gnr_ProductVariants",
                columns: new[] { "ProductId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gnr_ProductVariants_Sku",
                table: "gnr_ProductVariants",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gnr_ProductVariants_WooCommerceProductId_WooCommerceVariatio~",
                table: "gnr_ProductVariants",
                columns: new[] { "WooCommerceProductId", "WooCommerceVariationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ord_WooCommerceOrderItemSnapshots_WooCommerceOrderSnapshotId~",
                table: "ord_WooCommerceOrderItemSnapshots",
                columns: new[] { "WooCommerceOrderSnapshotId", "WooCommerceLineItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ord_WooCommerceOrderItemSnapshots_WooCommerceProductId",
                table: "ord_WooCommerceOrderItemSnapshots",
                column: "WooCommerceProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ord_WooCommerceOrderItemSnapshots_WooCommerceVariationId",
                table: "ord_WooCommerceOrderItemSnapshots",
                column: "WooCommerceVariationId");

            migrationBuilder.CreateIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_CustomerNationalCode",
                table: "ord_WooCommerceOrderSnapshots",
                column: "CustomerNationalCode");

            migrationBuilder.CreateIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_CustomerPhone",
                table: "ord_WooCommerceOrderSnapshots",
                column: "CustomerPhone");

            migrationBuilder.CreateIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_InternalStatus",
                table: "ord_WooCommerceOrderSnapshots",
                column: "InternalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_PaymentStatus",
                table: "ord_WooCommerceOrderSnapshots",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_WooCommerceOrderId",
                table: "ord_WooCommerceOrderSnapshots",
                column: "WooCommerceOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_WooCommerceOrderNumber",
                table: "ord_WooCommerceOrderSnapshots",
                column: "WooCommerceOrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_WooCommerceStatus",
                table: "ord_WooCommerceOrderSnapshots",
                column: "WooCommerceStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_WooCreatedAtUtc",
                table: "ord_WooCommerceOrderSnapshots",
                column: "WooCreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_pay_BarookPaymentSessions_BarookStatus",
                table: "pay_BarookPaymentSessions",
                column: "BarookStatus");

            migrationBuilder.CreateIndex(
                name: "IX_pay_BarookPaymentSessions_ExternalCode",
                table: "pay_BarookPaymentSessions",
                column: "ExternalCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pay_BarookPaymentSessions_Token",
                table: "pay_BarookPaymentSessions",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_pay_BarookPaymentSessions_WooCommerceOrderId",
                table: "pay_BarookPaymentSessions",
                column: "WooCommerceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_pay_BarookPaymentSessions_WooCommerceOrderSnapshotId",
                table: "pay_BarookPaymentSessions",
                column: "WooCommerceOrderSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_pay_ManualPaymentReceipts_ReceiptNumber",
                table: "pay_ManualPaymentReceipts",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pay_ManualPaymentReceipts_WooCommerceOrderId",
                table: "pay_ManualPaymentReceipts",
                column: "WooCommerceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_pay_ManualPaymentReceipts_WooCommerceOrderSnapshotId",
                table: "pay_ManualPaymentReceipts",
                column: "WooCommerceOrderSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_sec_AspNetRoleClaims_RoleId",
                table: "sec_AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "sec_AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sec_AspNetUserClaims_UserId",
                table: "sec_AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_sec_AspNetUserLogins_UserId",
                table: "sec_AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_sec_AspNetUserRoles_RoleId",
                table: "sec_AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "sec_AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "sec_AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sup_GatewayPaymentReceipts_IdempotencyKey",
                table: "sup_GatewayPaymentReceipts",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sup_GatewayPaymentReceipts_TransactionId",
                table: "sup_GatewayPaymentReceipts",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sup_GatewayPaymentReceipts_WooCommerceOrderId",
                table: "sup_GatewayPaymentReceipts",
                column: "WooCommerceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_sup_WooCommerceSyncLogs_CreatedAtUtc",
                table: "sup_WooCommerceSyncLogs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_sup_WooCommerceSyncLogs_Operation_Status",
                table: "sup_WooCommerceSyncLogs",
                columns: new[] { "Operation", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_sync_OutboxMessages_AggregateType_AggregateId",
                table: "sync_OutboxMessages",
                columns: new[] { "AggregateType", "AggregateId" });

            migrationBuilder.CreateIndex(
                name: "IX_sync_OutboxMessages_CreatedAtUtc",
                table: "sync_OutboxMessages",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_sync_OutboxMessages_Status",
                table: "sync_OutboxMessages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_wf_ApprovalAuditLogs_CreatedAtUtc",
                table: "wf_ApprovalAuditLogs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_wf_ApprovalAuditLogs_EntityType_EntityId",
                table: "wf_ApprovalAuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_wf_InventoryProposals_CreatedAtUtc",
                table: "wf_InventoryProposals",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_wf_InventoryProposals_ProductId",
                table: "wf_InventoryProposals",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_wf_InventoryProposals_ProductVariantId",
                table: "wf_InventoryProposals",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_wf_InventoryProposals_Status",
                table: "wf_InventoryProposals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_wf_PriceProposals_CreatedAtUtc",
                table: "wf_PriceProposals",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_wf_PriceProposals_ProductId",
                table: "wf_PriceProposals",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_wf_PriceProposals_ProductVariantId",
                table: "wf_PriceProposals",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_wf_PriceProposals_Status",
                table: "wf_PriceProposals",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "com_OrderItems");

            migrationBuilder.DropTable(
                name: "gnr_ProductProductTags");

            migrationBuilder.DropTable(
                name: "gnr_ProductSpecValues");

            migrationBuilder.DropTable(
                name: "ord_WooCommerceOrderItemSnapshots");

            migrationBuilder.DropTable(
                name: "pay_BarookPaymentSessions");

            migrationBuilder.DropTable(
                name: "pay_ManualPaymentReceipts");

            migrationBuilder.DropTable(
                name: "sec_AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "sec_AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "sec_AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "sec_AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "sec_AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "sup_GatewayPaymentReceipts");

            migrationBuilder.DropTable(
                name: "sup_WooCommerceSyncLogs");

            migrationBuilder.DropTable(
                name: "sync_OutboxMessages");

            migrationBuilder.DropTable(
                name: "wf_ApprovalAuditLogs");

            migrationBuilder.DropTable(
                name: "wf_InventoryProposals");

            migrationBuilder.DropTable(
                name: "wf_PriceProposals");

            migrationBuilder.DropTable(
                name: "com_Orders");

            migrationBuilder.DropTable(
                name: "gnr_ProductTags");

            migrationBuilder.DropTable(
                name: "gnr_ProductSpecDefinitions");

            migrationBuilder.DropTable(
                name: "ord_WooCommerceOrderSnapshots");

            migrationBuilder.DropTable(
                name: "sec_AspNetRoles");

            migrationBuilder.DropTable(
                name: "sec_AspNetUsers");

            migrationBuilder.DropTable(
                name: "gnr_ProductVariants");

            migrationBuilder.DropTable(
                name: "cbi_Customers");

            migrationBuilder.DropTable(
                name: "gnr_Products");

            migrationBuilder.DropTable(
                name: "gnr_Brands");

            migrationBuilder.DropTable(
                name: "gnr_Categories");

            migrationBuilder.DropTable(
                name: "gnr_Commodities");
        }
    }
}
