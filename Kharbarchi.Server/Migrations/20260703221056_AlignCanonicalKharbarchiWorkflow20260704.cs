using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Kharbarchi.Server.Migrations
{
    /// <inheritdoc />
    public partial class AlignCanonicalKharbarchiWorkflow20260704 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
#if false
            migrationBuilder.DropForeignKey(
                name: "FK_com_OrderItems_com_Orders_OrderId",
                table: "com_OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_com_OrderItems_gnr_ProductVariants_ProductVariantId",
                table: "com_OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_com_OrderItems_gnr_Products_ProductId",
                table: "com_OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_com_Orders_cbi_Customers_CustomerId",
                table: "com_Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_ProductProductTags_gnr_ProductTags_ProductTagId",
                table: "gnr_ProductProductTags");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_ProductProductTags_gnr_Products_ProductId",
                table: "gnr_ProductProductTags");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_Products_gnr_Brands_BrandId",
                table: "gnr_Products");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_Products_gnr_Categories_CategoryId",
                table: "gnr_Products");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_Products_gnr_Commodities_CommodityId",
                table: "gnr_Products");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_ProductSpecValues_gnr_ProductSpecDefinitions_SpecDefinit~",
                table: "gnr_ProductSpecValues");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_ProductSpecValues_gnr_Products_ProductId",
                table: "gnr_ProductSpecValues");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_ProductVariants_gnr_Products_ProductId",
                table: "gnr_ProductVariants");

            migrationBuilder.DropForeignKey(
                name: "FK_khb_ProductWooControlProfiles_gnr_Products_ProductId",
                table: "khb_ProductWooControlProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ord_WooCommerceOrderItemSnapshots_ord_WooCommerceOrderSnapsh~",
                table: "ord_WooCommerceOrderItemSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_pay_BarookPaymentSessions_ord_WooCommerceOrderSnapshots_WooC~",
                table: "pay_BarookPaymentSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_pay_ManualPaymentReceipts_ord_WooCommerceOrderSnapshots_WooC~",
                table: "pay_ManualPaymentReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_sec_AspNetRoleClaims_sec_AspNetRoles_RoleId",
                table: "sec_AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_sec_AspNetUserClaims_sec_AspNetUsers_UserId",
                table: "sec_AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_sec_AspNetUserLogins_sec_AspNetUsers_UserId",
                table: "sec_AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_sec_AspNetUserRoles_sec_AspNetRoles_RoleId",
                table: "sec_AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_sec_AspNetUserRoles_sec_AspNetUsers_UserId",
                table: "sec_AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_sec_AspNetUserTokens_sec_AspNetUsers_UserId",
                table: "sec_AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_wf_InventoryProposals_gnr_ProductVariants_ProductVariantId",
                table: "wf_InventoryProposals");

            migrationBuilder.DropForeignKey(
                name: "FK_wf_InventoryProposals_gnr_Products_ProductId",
                table: "wf_InventoryProposals");

            migrationBuilder.DropForeignKey(
                name: "FK_wf_PriceProposals_gnr_ProductVariants_ProductVariantId",
                table: "wf_PriceProposals");

            migrationBuilder.DropForeignKey(
                name: "FK_wf_PriceProposals_gnr_Products_ProductId",
                table: "wf_PriceProposals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_wf_PriceProposals",
                table: "wf_PriceProposals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_wf_InventoryProposals",
                table: "wf_InventoryProposals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_wf_ApprovalAuditLogs",
                table: "wf_ApprovalAuditLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sync_OutboxMessages",
                table: "sync_OutboxMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sup_WooCommerceSyncLogs",
                table: "sup_WooCommerceSyncLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sup_GatewayPaymentReceipts",
                table: "sup_GatewayPaymentReceipts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sec_AspNetUserTokens",
                table: "sec_AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sec_AspNetUsers",
                table: "sec_AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sec_AspNetUserRoles",
                table: "sec_AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sec_AspNetUserLogins",
                table: "sec_AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sec_AspNetUserClaims",
                table: "sec_AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sec_AspNetRoles",
                table: "sec_AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sec_AspNetRoleClaims",
                table: "sec_AspNetRoleClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_pay_ManualPaymentReceipts",
                table: "pay_ManualPaymentReceipts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_pay_BarookPaymentSessions",
                table: "pay_BarookPaymentSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ord_WooCommerceOrderSnapshots",
                table: "ord_WooCommerceOrderSnapshots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ord_WooCommerceOrderItemSnapshots",
                table: "ord_WooCommerceOrderItemSnapshots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_khb_ProductWooControlProfiles",
                table: "khb_ProductWooControlProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_ProductVariants",
                table: "gnr_ProductVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_ProductTags",
                table: "gnr_ProductTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_ProductSpecValues",
                table: "gnr_ProductSpecValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_ProductSpecDefinitions",
                table: "gnr_ProductSpecDefinitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_Products",
                table: "gnr_Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_ProductProductTags",
                table: "gnr_ProductProductTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_Commodities",
                table: "gnr_Commodities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_Categories",
                table: "gnr_Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_Brands",
                table: "gnr_Brands");

            migrationBuilder.DropPrimaryKey(
                name: "PK_com_Orders",
                table: "com_Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_com_OrderItems",
                table: "com_OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cbi_Customers",
                table: "cbi_Customers");

            migrationBuilder.RenameTable(
                name: "wf_PriceProposals",
                newName: "wf_priceproposals");

            migrationBuilder.RenameTable(
                name: "wf_InventoryProposals",
                newName: "wf_inventoryproposals");

            migrationBuilder.RenameTable(
                name: "wf_ApprovalAuditLogs",
                newName: "wf_approvalauditlogs");

            migrationBuilder.RenameTable(
                name: "sync_OutboxMessages",
                newName: "sync_outboxmessages");

            migrationBuilder.RenameTable(
                name: "sup_WooCommerceSyncLogs",
                newName: "sup_woocommercesynclogs");

            migrationBuilder.RenameTable(
                name: "sup_GatewayPaymentReceipts",
                newName: "sup_gatewaypaymentreceipts");

            migrationBuilder.RenameTable(
                name: "sec_AspNetUserTokens",
                newName: "sec_aspnetusertokens");

            migrationBuilder.RenameTable(
                name: "sec_AspNetUsers",
                newName: "sec_aspnetusers");

            migrationBuilder.RenameTable(
                name: "sec_AspNetUserRoles",
                newName: "sec_aspnetuserroles");

            migrationBuilder.RenameTable(
                name: "sec_AspNetUserLogins",
                newName: "sec_aspnetuserlogins");

            migrationBuilder.RenameTable(
                name: "sec_AspNetUserClaims",
                newName: "sec_aspnetuserclaims");

            migrationBuilder.RenameTable(
                name: "sec_AspNetRoles",
                newName: "sec_aspnetroles");

            migrationBuilder.RenameTable(
                name: "sec_AspNetRoleClaims",
                newName: "sec_aspnetroleclaims");

            migrationBuilder.RenameTable(
                name: "pay_ManualPaymentReceipts",
                newName: "pay_manualpaymentreceipts");

            migrationBuilder.RenameTable(
                name: "pay_BarookPaymentSessions",
                newName: "pay_barookpaymentsessions");

            migrationBuilder.RenameTable(
                name: "ord_WooCommerceOrderSnapshots",
                newName: "ord_woocommerceordersnapshots");

            migrationBuilder.RenameTable(
                name: "ord_WooCommerceOrderItemSnapshots",
                newName: "ord_woocommerceorderitemsnapshots");

            migrationBuilder.RenameTable(
                name: "khb_ProductWooControlProfiles",
                newName: "khb_productwoocontrolprofiles");

            migrationBuilder.RenameTable(
                name: "gnr_ProductVariants",
                newName: "gnr_productvariants");

            migrationBuilder.RenameTable(
                name: "gnr_ProductTags",
                newName: "gnr_producttags");

            migrationBuilder.RenameTable(
                name: "gnr_ProductSpecValues",
                newName: "gnr_productspecvalues");

            migrationBuilder.RenameTable(
                name: "gnr_ProductSpecDefinitions",
                newName: "gnr_productspecdefinitions");

            migrationBuilder.RenameTable(
                name: "gnr_Products",
                newName: "gnr_products");

            migrationBuilder.RenameTable(
                name: "gnr_ProductProductTags",
                newName: "gnr_productproducttags");

            migrationBuilder.RenameTable(
                name: "gnr_Commodities",
                newName: "gnr_commodities");

            migrationBuilder.RenameTable(
                name: "gnr_Categories",
                newName: "gnr_categories");

            migrationBuilder.RenameTable(
                name: "gnr_Brands",
                newName: "gnr_brands");

            migrationBuilder.RenameTable(
                name: "com_Orders",
                newName: "com_orders");

            migrationBuilder.RenameTable(
                name: "com_OrderItems",
                newName: "com_orderitems");

            migrationBuilder.RenameTable(
                name: "cbi_Customers",
                newName: "cbi_customers");

            migrationBuilder.RenameIndex(
                name: "IX_wf_PriceProposals_Status",
                table: "wf_priceproposals",
                newName: "IX_wf_priceproposals_Status");

            migrationBuilder.RenameIndex(
                name: "IX_wf_PriceProposals_ProductVariantId",
                table: "wf_priceproposals",
                newName: "IX_wf_priceproposals_ProductVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_wf_PriceProposals_ProductId",
                table: "wf_priceproposals",
                newName: "IX_wf_priceproposals_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_wf_PriceProposals_CreatedAtUtc",
                table: "wf_priceproposals",
                newName: "IX_wf_priceproposals_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_wf_InventoryProposals_Status",
                table: "wf_inventoryproposals",
                newName: "IX_wf_inventoryproposals_Status");

            migrationBuilder.RenameIndex(
                name: "IX_wf_InventoryProposals_ProductVariantId",
                table: "wf_inventoryproposals",
                newName: "IX_wf_inventoryproposals_ProductVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_wf_InventoryProposals_ProductId",
                table: "wf_inventoryproposals",
                newName: "IX_wf_inventoryproposals_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_wf_InventoryProposals_CreatedAtUtc",
                table: "wf_inventoryproposals",
                newName: "IX_wf_inventoryproposals_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_wf_ApprovalAuditLogs_EntityType_EntityId",
                table: "wf_approvalauditlogs",
                newName: "IX_wf_approvalauditlogs_EntityType_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_wf_ApprovalAuditLogs_CreatedAtUtc",
                table: "wf_approvalauditlogs",
                newName: "IX_wf_approvalauditlogs_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_sync_OutboxMessages_Status",
                table: "sync_outboxmessages",
                newName: "IX_sync_outboxmessages_Status");

            migrationBuilder.RenameIndex(
                name: "IX_sync_OutboxMessages_CreatedAtUtc",
                table: "sync_outboxmessages",
                newName: "IX_sync_outboxmessages_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_sync_OutboxMessages_AggregateType_AggregateId",
                table: "sync_outboxmessages",
                newName: "IX_sync_outboxmessages_AggregateType_AggregateId");

            migrationBuilder.RenameIndex(
                name: "IX_sup_WooCommerceSyncLogs_Operation_Status",
                table: "sup_woocommercesynclogs",
                newName: "IX_sup_woocommercesynclogs_Operation_Status");

            migrationBuilder.RenameIndex(
                name: "IX_sup_WooCommerceSyncLogs_CreatedAtUtc",
                table: "sup_woocommercesynclogs",
                newName: "IX_sup_woocommercesynclogs_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_sup_GatewayPaymentReceipts_WooCommerceOrderId",
                table: "sup_gatewaypaymentreceipts",
                newName: "IX_sup_gatewaypaymentreceipts_WooCommerceOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_sup_GatewayPaymentReceipts_TransactionId",
                table: "sup_gatewaypaymentreceipts",
                newName: "IX_sup_gatewaypaymentreceipts_TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_sup_GatewayPaymentReceipts_IdempotencyKey",
                table: "sup_gatewaypaymentreceipts",
                newName: "IX_sup_gatewaypaymentreceipts_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "IX_sec_AspNetUserRoles_RoleId",
                table: "sec_aspnetuserroles",
                newName: "IX_sec_aspnetuserroles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_sec_AspNetUserLogins_UserId",
                table: "sec_aspnetuserlogins",
                newName: "IX_sec_aspnetuserlogins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_sec_AspNetUserClaims_UserId",
                table: "sec_aspnetuserclaims",
                newName: "IX_sec_aspnetuserclaims_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_sec_AspNetRoleClaims_RoleId",
                table: "sec_aspnetroleclaims",
                newName: "IX_sec_aspnetroleclaims_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_pay_ManualPaymentReceipts_WooCommerceOrderSnapshotId",
                table: "pay_manualpaymentreceipts",
                newName: "IX_pay_manualpaymentreceipts_WooCommerceOrderSnapshotId");

            migrationBuilder.RenameIndex(
                name: "IX_pay_ManualPaymentReceipts_WooCommerceOrderId",
                table: "pay_manualpaymentreceipts",
                newName: "IX_pay_manualpaymentreceipts_WooCommerceOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_pay_ManualPaymentReceipts_ReceiptNumber",
                table: "pay_manualpaymentreceipts",
                newName: "IX_pay_manualpaymentreceipts_ReceiptNumber");

            migrationBuilder.RenameIndex(
                name: "IX_pay_BarookPaymentSessions_WooCommerceOrderSnapshotId",
                table: "pay_barookpaymentsessions",
                newName: "IX_pay_barookpaymentsessions_WooCommerceOrderSnapshotId");

            migrationBuilder.RenameIndex(
                name: "IX_pay_BarookPaymentSessions_WooCommerceOrderId",
                table: "pay_barookpaymentsessions",
                newName: "IX_pay_barookpaymentsessions_WooCommerceOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_pay_BarookPaymentSessions_Token",
                table: "pay_barookpaymentsessions",
                newName: "IX_pay_barookpaymentsessions_Token");

            migrationBuilder.RenameIndex(
                name: "IX_pay_BarookPaymentSessions_ExternalCode",
                table: "pay_barookpaymentsessions",
                newName: "IX_pay_barookpaymentsessions_ExternalCode");

            migrationBuilder.RenameIndex(
                name: "IX_pay_BarookPaymentSessions_BarookStatus",
                table: "pay_barookpaymentsessions",
                newName: "IX_pay_barookpaymentsessions_BarookStatus");

            migrationBuilder.RenameIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_WooCreatedAtUtc",
                table: "ord_woocommerceordersnapshots",
                newName: "IX_ord_woocommerceordersnapshots_WooCreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_WooCommerceStatus",
                table: "ord_woocommerceordersnapshots",
                newName: "IX_ord_woocommerceordersnapshots_WooCommerceStatus");

            migrationBuilder.RenameIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_WooCommerceOrderNumber",
                table: "ord_woocommerceordersnapshots",
                newName: "IX_ord_woocommerceordersnapshots_WooCommerceOrderNumber");

            migrationBuilder.RenameIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_WooCommerceOrderId",
                table: "ord_woocommerceordersnapshots",
                newName: "IX_ord_woocommerceordersnapshots_WooCommerceOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_PaymentStatus",
                table: "ord_woocommerceordersnapshots",
                newName: "IX_ord_woocommerceordersnapshots_PaymentStatus");

            migrationBuilder.RenameIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_InternalStatus",
                table: "ord_woocommerceordersnapshots",
                newName: "IX_ord_woocommerceordersnapshots_InternalStatus");

            migrationBuilder.RenameIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_CustomerPhone",
                table: "ord_woocommerceordersnapshots",
                newName: "IX_ord_woocommerceordersnapshots_CustomerPhone");

            migrationBuilder.RenameIndex(
                name: "IX_ord_WooCommerceOrderSnapshots_CustomerNationalCode",
                table: "ord_woocommerceordersnapshots",
                newName: "IX_ord_woocommerceordersnapshots_CustomerNationalCode");

            migrationBuilder.RenameIndex(
                name: "IX_ord_WooCommerceOrderItemSnapshots_WooCommerceVariationId",
                table: "ord_woocommerceorderitemsnapshots",
                newName: "IX_ord_woocommerceorderitemsnapshots_WooCommerceVariationId");

            migrationBuilder.RenameIndex(
                name: "IX_ord_WooCommerceOrderItemSnapshots_WooCommerceProductId",
                table: "ord_woocommerceorderitemsnapshots",
                newName: "IX_ord_woocommerceorderitemsnapshots_WooCommerceProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ord_WooCommerceOrderItemSnapshots_WooCommerceOrderSnapshotId~",
                table: "ord_woocommerceorderitemsnapshots",
                newName: "IX_ord_woocommerceorderitemsnapshots_WooCommerceOrderSnapshotId~");

            migrationBuilder.RenameIndex(
                name: "IX_khb_ProductWooControlProfiles_WooSyncStatus",
                table: "khb_productwoocontrolprofiles",
                newName: "IX_khb_productwoocontrolprofiles_WooSyncStatus");

            migrationBuilder.RenameIndex(
                name: "IX_khb_ProductWooControlProfiles_ProductId",
                table: "khb_productwoocontrolprofiles",
                newName: "IX_khb_productwoocontrolprofiles_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_khb_ProductWooControlProfiles_PriceCheckStatus",
                table: "khb_productwoocontrolprofiles",
                newName: "IX_khb_productwoocontrolprofiles_PriceCheckStatus");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_ProductVariants_WooCommerceProductId_WooCommerceVariatio~",
                table: "gnr_productvariants",
                newName: "IX_gnr_productvariants_WooCommerceProductId_WooCommerceVariatio~");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_ProductVariants_Sku",
                table: "gnr_productvariants",
                newName: "IX_gnr_productvariants_Sku");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_ProductVariants_ProductId_Name",
                table: "gnr_productvariants",
                newName: "IX_gnr_productvariants_ProductId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_ProductTags_Slug",
                table: "gnr_producttags",
                newName: "IX_gnr_producttags_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_ProductSpecValues_SpecDefinitionId",
                table: "gnr_productspecvalues",
                newName: "IX_gnr_productspecvalues_SpecDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_ProductSpecValues_ProductId_SpecDefinitionId",
                table: "gnr_productspecvalues",
                newName: "IX_gnr_productspecvalues_ProductId_SpecDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_ProductSpecDefinitions_Slug",
                table: "gnr_productspecdefinitions",
                newName: "IX_gnr_productspecdefinitions_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_Products_WooCommerceProductId",
                table: "gnr_products",
                newName: "IX_gnr_products_WooCommerceProductId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_Products_Slug",
                table: "gnr_products",
                newName: "IX_gnr_products_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_Products_Sku",
                table: "gnr_products",
                newName: "IX_gnr_products_Sku");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_Products_CommodityId",
                table: "gnr_products",
                newName: "IX_gnr_products_CommodityId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_Products_CategoryId",
                table: "gnr_products",
                newName: "IX_gnr_products_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_Products_BrandId",
                table: "gnr_products",
                newName: "IX_gnr_products_BrandId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_ProductProductTags_ProductTagId",
                table: "gnr_productproducttags",
                newName: "IX_gnr_productproducttags_ProductTagId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_Commodities_WooCommerceCommodityId",
                table: "gnr_commodities",
                newName: "IX_gnr_commodities_WooCommerceCommodityId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_Commodities_Slug",
                table: "gnr_commodities",
                newName: "IX_gnr_commodities_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_Categories_WooCommerceCategoryId",
                table: "gnr_categories",
                newName: "IX_gnr_categories_WooCommerceCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_Categories_Slug",
                table: "gnr_categories",
                newName: "IX_gnr_categories_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_Brands_WooCommerceBrandId",
                table: "gnr_brands",
                newName: "IX_gnr_brands_WooCommerceBrandId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_Brands_Slug",
                table: "gnr_brands",
                newName: "IX_gnr_brands_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_com_Orders_WooCommerceOrderId",
                table: "com_orders",
                newName: "IX_com_orders_WooCommerceOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_com_Orders_CustomerId",
                table: "com_orders",
                newName: "IX_com_orders_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_com_Orders_CreatedAt",
                table: "com_orders",
                newName: "IX_com_orders_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_com_OrderItems_ProductVariantId",
                table: "com_orderitems",
                newName: "IX_com_orderitems_ProductVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_com_OrderItems_ProductId",
                table: "com_orderitems",
                newName: "IX_com_orderitems_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_com_OrderItems_OrderId",
                table: "com_orderitems",
                newName: "IX_com_orderitems_OrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_wf_priceproposals",
                table: "wf_priceproposals",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_wf_inventoryproposals",
                table: "wf_inventoryproposals",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_wf_approvalauditlogs",
                table: "wf_approvalauditlogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sync_outboxmessages",
                table: "sync_outboxmessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sup_woocommercesynclogs",
                table: "sup_woocommercesynclogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sup_gatewaypaymentreceipts",
                table: "sup_gatewaypaymentreceipts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sec_aspnetusertokens",
                table: "sec_aspnetusertokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_sec_aspnetusers",
                table: "sec_aspnetusers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sec_aspnetuserroles",
                table: "sec_aspnetuserroles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_sec_aspnetuserlogins",
                table: "sec_aspnetuserlogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_sec_aspnetuserclaims",
                table: "sec_aspnetuserclaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sec_aspnetroles",
                table: "sec_aspnetroles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sec_aspnetroleclaims",
                table: "sec_aspnetroleclaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_pay_manualpaymentreceipts",
                table: "pay_manualpaymentreceipts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_pay_barookpaymentsessions",
                table: "pay_barookpaymentsessions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ord_woocommerceordersnapshots",
                table: "ord_woocommerceordersnapshots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ord_woocommerceorderitemsnapshots",
                table: "ord_woocommerceorderitemsnapshots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_khb_productwoocontrolprofiles",
                table: "khb_productwoocontrolprofiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_productvariants",
                table: "gnr_productvariants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_producttags",
                table: "gnr_producttags",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_productspecvalues",
                table: "gnr_productspecvalues",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_productspecdefinitions",
                table: "gnr_productspecdefinitions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_products",
                table: "gnr_products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_productproducttags",
                table: "gnr_productproducttags",
                columns: new[] { "ProductId", "ProductTagId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_commodities",
                table: "gnr_commodities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_categories",
                table: "gnr_categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_brands",
                table: "gnr_brands",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_com_orders",
                table: "com_orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_com_orderitems",
                table: "com_orderitems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cbi_customers",
                table: "cbi_customers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "all_product_with_process",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ImportBatchId = table.Column<string>(type: "longtext", nullable: true),
                    SourceRowNumber = table.Column<int>(type: "int", nullable: true),
                    SourceRowHash = table.Column<string>(type: "char(64)", nullable: false),
                    RawJson = table.Column<string>(type: "longtext", nullable: true),
                    MainProductName = table.Column<string>(type: "varchar(255)", nullable: true),
                    MainProductSlug = table.Column<string>(type: "longtext", nullable: true),
                    GroupName = table.Column<string>(type: "longtext", nullable: true),
                    CategoryName = table.Column<string>(type: "longtext", nullable: true),
                    CategorySlug = table.Column<string>(type: "longtext", nullable: true),
                    ProductName = table.Column<string>(type: "varchar(700)", maxLength: 700, nullable: true),
                    ProductEnglishName = table.Column<string>(type: "varchar(700)", maxLength: 700, nullable: true),
                    ProductSlug = table.Column<string>(type: "varchar(700)", maxLength: 700, nullable: true),
                    SKU = table.Column<string>(type: "varchar(191)", maxLength: 191, nullable: true),
                    BrandName = table.Column<string>(type: "longtext", nullable: true),
                    BrandEnglishName = table.Column<string>(type: "longtext", nullable: true),
                    PackageName = table.Column<string>(type: "longtext", nullable: true),
                    UnitWeight = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    PacksPerCarton = table.Column<int>(type: "int", nullable: true),
                    CartonQuantity = table.Column<int>(type: "int", nullable: true),
                    PackagingPricePerPack = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SalePriceCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SalePriceInstallment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PurchasePriceCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PurchasePriceInstallment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ShortDescription = table.Column<string>(type: "longtext", nullable: true),
                    FullDescription = table.Column<string>(type: "longtext", nullable: true),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: true),
                    GalleryJson = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    WooProductId = table.Column<long>(type: "bigint", nullable: true),
                    HaveOtherPackage = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    PackageOne = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_all_product_with_process", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khb_category_map",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SourceKey = table.Column<string>(type: "varchar(255)", nullable: false),
                    CategoryName = table.Column<string>(type: "longtext", nullable: true),
                    CategorySlug = table.Column<string>(type: "longtext", nullable: true),
                    WooCategoryId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khb_category_map", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khb_commodity",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SourceKey = table.Column<string>(type: "varchar(255)", nullable: false),
                    CommodityName = table.Column<string>(type: "longtext", nullable: true),
                    CommoditySlug = table.Column<string>(type: "longtext", nullable: true),
                    WooCommodityId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khb_commodity", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khb_imported_woocommerce_records",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SourceType = table.Column<string>(type: "varchar(255)", nullable: false),
                    ExternalId = table.Column<string>(type: "varchar(255)", nullable: true),
                    Slug = table.Column<string>(type: "longtext", nullable: true),
                    Title = table.Column<string>(type: "longtext", nullable: true),
                    RawJson = table.Column<string>(type: "longtext", nullable: false),
                    ImportedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SourceUrl = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khb_imported_woocommerce_records", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khb_package_type",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SourceKey = table.Column<string>(type: "varchar(255)", nullable: false),
                    PackageGroup = table.Column<string>(type: "longtext", nullable: true),
                    PackageCode = table.Column<string>(type: "longtext", nullable: true),
                    PackageTitle = table.Column<string>(type: "longtext", nullable: true),
                    UnitWeightKg = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    PacksPerCarton = table.Column<int>(type: "int", nullable: true),
                    PackagingPricePerPack = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    WooPackageId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khb_package_type", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khb_product_change_log",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ChangeType = table.Column<string>(type: "longtext", nullable: false),
                    Summary = table.Column<string>(type: "longtext", nullable: true),
                    Payload = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khb_product_change_log", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khb_product_final",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SourceKey = table.Column<string>(type: "char(64)", nullable: false),
                    MainGroupId = table.Column<long>(type: "bigint", nullable: true),
                    CategorySourceKey = table.Column<string>(type: "longtext", nullable: true),
                    CommoditySourceKey = table.Column<string>(type: "longtext", nullable: true),
                    PackageSourceKey = table.Column<string>(type: "longtext", nullable: true),
                    ProductName = table.Column<string>(type: "varchar(700)", maxLength: 700, nullable: true),
                    ProductEnglishName = table.Column<string>(type: "varchar(700)", maxLength: 700, nullable: true),
                    ProductSlug = table.Column<string>(type: "varchar(700)", maxLength: 700, nullable: true),
                    WooProductId = table.Column<long>(type: "bigint", nullable: true),
                    SKU = table.Column<string>(type: "varchar(191)", maxLength: 191, nullable: true),
                    PackageGroup = table.Column<string>(type: "longtext", nullable: true),
                    PackageCode = table.Column<string>(type: "longtext", nullable: true),
                    UnitWeightKg = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    PacksPerCarton = table.Column<int>(type: "int", nullable: true),
                    PackagingPricePerPack = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    KgCashPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    KgCreditPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SaleCashPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SaleCreditPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BuyCashPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BuyCreditPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    CatalogVisibility = table.Column<string>(type: "longtext", nullable: true),
                    WooPayloadJson = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    BrandName = table.Column<string>(type: "longtext", nullable: true),
                    BrandEnglishName = table.Column<string>(type: "longtext", nullable: true),
                    PackageTitle = table.Column<string>(type: "longtext", nullable: true),
                    BulkWeightKg = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    MinPurchaseKg = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    ImageTag = table.Column<string>(type: "longtext", nullable: true),
                    SaleMode = table.Column<string>(type: "longtext", nullable: true),
                    PriceCalculationBasis = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khb_product_final", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khb_product_main_groups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MainProductName = table.Column<string>(type: "longtext", nullable: true),
                    MainProductSlug = table.Column<string>(type: "varchar(255)", nullable: true),
                    CategoryName = table.Column<string>(type: "longtext", nullable: true),
                    EnTaxonomic = table.Column<string>(type: "longtext", nullable: true),
                    CategorySlug = table.Column<string>(type: "longtext", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SourceKey = table.Column<string>(type: "longtext", nullable: true),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khb_product_main_groups", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khb_product_price_history",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductSourceKey = table.Column<string>(type: "char(64)", nullable: false),
                    ProductName = table.Column<string>(type: "longtext", nullable: true),
                    SKU = table.Column<string>(type: "varchar(191)", maxLength: 191, nullable: true),
                    ProductType = table.Column<string>(type: "varchar(255)", nullable: false),
                    PackageGroup = table.Column<string>(type: "longtext", nullable: true),
                    PackageCode = table.Column<string>(type: "longtext", nullable: true),
                    PriceType = table.Column<string>(type: "varchar(255)", nullable: false),
                    PriceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "longtext", nullable: false),
                    ValidFromUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ValidToUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khb_product_price_history", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khb_product_update_queue",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SourceKey = table.Column<string>(type: "char(64)", nullable: false),
                    EntityType = table.Column<string>(type: "longtext", nullable: false),
                    QueueStatus = table.Column<string>(type: "varchar(255)", nullable: false),
                    ActionType = table.Column<string>(type: "longtext", nullable: false),
                    SKU = table.Column<string>(type: "varchar(191)", maxLength: 191, nullable: true),
                    ProductSlug = table.Column<string>(type: "longtext", nullable: true),
                    WooProductId = table.Column<long>(type: "bigint", nullable: true),
                    WooPayloadJson = table.Column<string>(type: "longtext", nullable: true),
                    LastError = table.Column<string>(type: "longtext", nullable: true),
                    JobId = table.Column<Guid>(type: "char(36)", nullable: true),
                    TryCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khb_product_update_queue", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khb_sale_products",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MainGroupId = table.Column<long>(type: "bigint", nullable: true),
                    SourceRowHash = table.Column<string>(type: "char(64)", nullable: false),
                    WooProductId = table.Column<long>(type: "bigint", nullable: true),
                    ProductName = table.Column<string>(type: "varchar(700)", maxLength: 700, nullable: true),
                    ProductEnglishName = table.Column<string>(type: "varchar(700)", maxLength: 700, nullable: true),
                    ProductSlug = table.Column<string>(type: "varchar(700)", maxLength: 700, nullable: true),
                    SKU = table.Column<string>(type: "varchar(191)", maxLength: 191, nullable: true),
                    BrandName = table.Column<string>(type: "longtext", nullable: true),
                    BrandEnglishName = table.Column<string>(type: "longtext", nullable: true),
                    PackageName = table.Column<string>(type: "longtext", nullable: true),
                    PackagingGroup = table.Column<string>(type: "longtext", nullable: true),
                    PackageCode = table.Column<string>(type: "longtext", nullable: true),
                    UnitWeight = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    PacksPerCarton = table.Column<int>(type: "int", nullable: true),
                    CartonQuantity = table.Column<int>(type: "int", nullable: true),
                    PackagingPricePerPack = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    KgPriceCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    KgPriceInstallment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SalePriceCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SalePriceInstallment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PurchasePriceCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PurchasePriceInstallment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ShortDescription = table.Column<string>(type: "longtext", nullable: true),
                    FullDescription = table.Column<string>(type: "longtext", nullable: true),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: true),
                    GalleryJson = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, defaultValue: "draft"),
                    RawJson = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SaleMode = table.Column<string>(type: "longtext", nullable: true),
                    PriceCalculationBasis = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khb_sale_products", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khb_source_product",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SourceKey = table.Column<string>(type: "char(64)", nullable: false),
                    SourceRowId = table.Column<long>(type: "bigint", nullable: true),
                    ProductName = table.Column<string>(type: "longtext", nullable: true),
                    ProductEnglishName = table.Column<string>(type: "longtext", nullable: true),
                    MainProductName = table.Column<string>(type: "longtext", nullable: true),
                    CategoryName = table.Column<string>(type: "longtext", nullable: true),
                    CategorySlug = table.Column<string>(type: "longtext", nullable: true),
                    BrandName = table.Column<string>(type: "longtext", nullable: true),
                    BrandEnglishName = table.Column<string>(type: "longtext", nullable: true),
                    PackageOne = table.Column<string>(type: "longtext", nullable: true),
                    UnitWeightKg = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    KgCashPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    KgCreditPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    RawJson = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khb_source_product", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khb_woocommerce_connection_profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProfileName = table.Column<string>(type: "varchar(255)", nullable: false),
                    EnvironmentType = table.Column<string>(type: "varchar(255)", nullable: false),
                    BaseUrl = table.Column<string>(type: "longtext", nullable: false),
                    ConsumerKey = table.Column<string>(type: "longtext", nullable: false),
                    ProtectedConsumerSecret = table.Column<string>(type: "longtext", nullable: false),
                    ApiVersion = table.Column<string>(type: "longtext", nullable: false),
                    VerifySsl = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastTestedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastTestSucceeded = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    LastTestMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khb_woocommerce_connection_profiles", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khb_workflow_jobs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    JobId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Type = table.Column<string>(type: "varchar(255)", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    CurrentStep = table.Column<string>(type: "longtext", nullable: false),
                    TotalItems = table.Column<int>(type: "int", nullable: false),
                    ProcessedItems = table.Column<int>(type: "int", nullable: false),
                    SuccessCount = table.Column<int>(type: "int", nullable: false),
                    ErrorCount = table.Column<int>(type: "int", nullable: false),
                    DraftCount = table.Column<int>(type: "int", nullable: false),
                    SkippedCount = table.Column<int>(type: "int", nullable: false),
                    PendingCount = table.Column<int>(type: "int", nullable: false),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FinishedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khb_workflow_jobs", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khb_workflow_job_logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WorkflowJobId = table.Column<long>(type: "bigint", nullable: false),
                    JobId = table.Column<Guid>(type: "char(36)", nullable: false),
                    StepName = table.Column<string>(type: "longtext", nullable: false),
                    EntityType = table.Column<string>(type: "longtext", nullable: true),
                    EntityId = table.Column<string>(type: "longtext", nullable: true),
                    Sku = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    Message = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true),
                    RequestUrl = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    ResponseCode = table.Column<int>(type: "int", nullable: true),
                    ResponseBodySummary = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khb_workflow_job_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_khb_workflow_job_logs_khb_workflow_jobs_WorkflowJobId",
                        column: x => x.WorkflowJobId,
                        principalTable: "khb_workflow_jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_All_Product_With_Process_MainProductName",
                table: "all_product_with_process",
                column: "MainProductName");

            migrationBuilder.CreateIndex(
                name: "IX_All_Product_With_Process_ProductName",
                table: "all_product_with_process",
                column: "ProductName");

            migrationBuilder.CreateIndex(
                name: "IX_All_Product_With_Process_SKU",
                table: "all_product_with_process",
                column: "SKU");

            migrationBuilder.CreateIndex(
                name: "UX_All_Product_With_Process_SourceRowHash",
                table: "all_product_with_process",
                column: "SourceRowHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_KHB_Category_Map_SourceKey",
                table: "khb_category_map",
                column: "SourceKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_KHB_Commodity_SourceKey",
                table: "khb_commodity",
                column: "SourceKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_khb_imported_woocommerce_records_Source_External",
                table: "khb_imported_woocommerce_records",
                columns: new[] { "SourceType", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_khb_package_type_WooPackageId",
                table: "khb_package_type",
                column: "WooPackageId");

            migrationBuilder.CreateIndex(
                name: "UX_KHB_Package_Type_SourceKey",
                table: "khb_package_type",
                column: "SourceKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_khb_product_change_log_ProductId",
                table: "khb_product_change_log",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_khb_product_final_ProductSlug",
                table: "khb_product_final",
                column: "ProductSlug");

            migrationBuilder.CreateIndex(
                name: "IX_KHB_Product_Final_SKU",
                table: "khb_product_final",
                column: "SKU");

            migrationBuilder.CreateIndex(
                name: "IX_khb_product_final_WooProductId",
                table: "khb_product_final",
                column: "WooProductId");

            migrationBuilder.CreateIndex(
                name: "UX_KHB_Product_Final_SourceKey",
                table: "khb_product_final",
                column: "SourceKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_khb_product_main_groups_slug",
                table: "khb_product_main_groups",
                column: "MainProductSlug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KHB_Product_Price_History_Date",
                table: "khb_product_price_history",
                columns: new[] { "ValidFromUtc", "ValidToUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_KHB_Product_Price_History_Product",
                table: "khb_product_price_history",
                columns: new[] { "ProductSourceKey", "ProductType", "PriceType", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_KHB_Product_Price_History_SKU",
                table: "khb_product_price_history",
                column: "SKU");

            migrationBuilder.CreateIndex(
                name: "IX_khb_product_update_queue_JobId",
                table: "khb_product_update_queue",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_KHB_Product_Update_Queue_Status",
                table: "khb_product_update_queue",
                column: "QueueStatus");

            migrationBuilder.CreateIndex(
                name: "UX_KHB_Product_Update_Queue_SourceKey",
                table: "khb_product_update_queue",
                column: "SourceKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_khb_sale_products_name",
                table: "khb_sale_products",
                column: "ProductName");

            migrationBuilder.CreateIndex(
                name: "IX_khb_sale_products_sku",
                table: "khb_sale_products",
                column: "SKU");

            migrationBuilder.CreateIndex(
                name: "IX_khb_sale_products_slug",
                table: "khb_sale_products",
                column: "ProductSlug");

            migrationBuilder.CreateIndex(
                name: "IX_khb_sale_products_woo",
                table: "khb_sale_products",
                column: "WooProductId");

            migrationBuilder.CreateIndex(
                name: "UX_khb_sale_products_hash",
                table: "khb_sale_products",
                column: "SourceRowHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_KHB_Source_Product_SourceKey",
                table: "khb_source_product",
                column: "SourceKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_khb_woocommerce_connection_profiles_EnvironmentType_IsActive",
                table: "khb_woocommerce_connection_profiles",
                columns: new[] { "EnvironmentType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_khb_woocommerce_connection_profiles_ProfileName",
                table: "khb_woocommerce_connection_profiles",
                column: "ProfileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_khb_workflow_job_logs_JobId_CreatedAtUtc",
                table: "khb_workflow_job_logs",
                columns: new[] { "JobId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_khb_workflow_job_logs_WorkflowJobId",
                table: "khb_workflow_job_logs",
                column: "WorkflowJobId");

            migrationBuilder.CreateIndex(
                name: "IX_khb_workflow_jobs_JobId",
                table: "khb_workflow_jobs",
                column: "JobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_khb_workflow_jobs_Type_CreatedAtUtc",
                table: "khb_workflow_jobs",
                columns: new[] { "Type", "CreatedAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_com_orderitems_com_orders_OrderId",
                table: "com_orderitems",
                column: "OrderId",
                principalTable: "com_orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_com_orderitems_gnr_products_ProductId",
                table: "com_orderitems",
                column: "ProductId",
                principalTable: "gnr_products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_com_orderitems_gnr_productvariants_ProductVariantId",
                table: "com_orderitems",
                column: "ProductVariantId",
                principalTable: "gnr_productvariants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_com_orders_cbi_customers_CustomerId",
                table: "com_orders",
                column: "CustomerId",
                principalTable: "cbi_customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_productproducttags_gnr_products_ProductId",
                table: "gnr_productproducttags",
                column: "ProductId",
                principalTable: "gnr_products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_productproducttags_gnr_producttags_ProductTagId",
                table: "gnr_productproducttags",
                column: "ProductTagId",
                principalTable: "gnr_producttags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_products_gnr_brands_BrandId",
                table: "gnr_products",
                column: "BrandId",
                principalTable: "gnr_brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_products_gnr_categories_CategoryId",
                table: "gnr_products",
                column: "CategoryId",
                principalTable: "gnr_categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_products_gnr_commodities_CommodityId",
                table: "gnr_products",
                column: "CommodityId",
                principalTable: "gnr_commodities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_productspecvalues_gnr_products_ProductId",
                table: "gnr_productspecvalues",
                column: "ProductId",
                principalTable: "gnr_products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_productspecvalues_gnr_productspecdefinitions_SpecDefinit~",
                table: "gnr_productspecvalues",
                column: "SpecDefinitionId",
                principalTable: "gnr_productspecdefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_productvariants_gnr_products_ProductId",
                table: "gnr_productvariants",
                column: "ProductId",
                principalTable: "gnr_products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_khb_productwoocontrolprofiles_gnr_products_ProductId",
                table: "khb_productwoocontrolprofiles",
                column: "ProductId",
                principalTable: "gnr_products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ord_woocommerceorderitemsnapshots_ord_woocommerceordersnapsh~",
                table: "ord_woocommerceorderitemsnapshots",
                column: "WooCommerceOrderSnapshotId",
                principalTable: "ord_woocommerceordersnapshots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_pay_barookpaymentsessions_ord_woocommerceordersnapshots_WooC~",
                table: "pay_barookpaymentsessions",
                column: "WooCommerceOrderSnapshotId",
                principalTable: "ord_woocommerceordersnapshots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_pay_manualpaymentreceipts_ord_woocommerceordersnapshots_WooC~",
                table: "pay_manualpaymentreceipts",
                column: "WooCommerceOrderSnapshotId",
                principalTable: "ord_woocommerceordersnapshots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sec_aspnetroleclaims_sec_aspnetroles_RoleId",
                table: "sec_aspnetroleclaims",
                column: "RoleId",
                principalTable: "sec_aspnetroles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sec_aspnetuserclaims_sec_aspnetusers_UserId",
                table: "sec_aspnetuserclaims",
                column: "UserId",
                principalTable: "sec_aspnetusers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sec_aspnetuserlogins_sec_aspnetusers_UserId",
                table: "sec_aspnetuserlogins",
                column: "UserId",
                principalTable: "sec_aspnetusers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sec_aspnetuserroles_sec_aspnetroles_RoleId",
                table: "sec_aspnetuserroles",
                column: "RoleId",
                principalTable: "sec_aspnetroles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sec_aspnetuserroles_sec_aspnetusers_UserId",
                table: "sec_aspnetuserroles",
                column: "UserId",
                principalTable: "sec_aspnetusers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sec_aspnetusertokens_sec_aspnetusers_UserId",
                table: "sec_aspnetusertokens",
                column: "UserId",
                principalTable: "sec_aspnetusers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_wf_inventoryproposals_gnr_products_ProductId",
                table: "wf_inventoryproposals",
                column: "ProductId",
                principalTable: "gnr_products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_wf_inventoryproposals_gnr_productvariants_ProductVariantId",
                table: "wf_inventoryproposals",
                column: "ProductVariantId",
                principalTable: "gnr_productvariants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_wf_priceproposals_gnr_products_ProductId",
                table: "wf_priceproposals",
                column: "ProductId",
                principalTable: "gnr_products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_wf_priceproposals_gnr_productvariants_ProductVariantId",
                table: "wf_priceproposals",
                column: "ProductVariantId",
                principalTable: "gnr_productvariants",
                principalColumn: "Id");
#endif
            ApplySafeForwardOnlyMigration(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
#if false
            migrationBuilder.DropForeignKey(
                name: "FK_com_orderitems_com_orders_OrderId",
                table: "com_orderitems");

            migrationBuilder.DropForeignKey(
                name: "FK_com_orderitems_gnr_products_ProductId",
                table: "com_orderitems");

            migrationBuilder.DropForeignKey(
                name: "FK_com_orderitems_gnr_productvariants_ProductVariantId",
                table: "com_orderitems");

            migrationBuilder.DropForeignKey(
                name: "FK_com_orders_cbi_customers_CustomerId",
                table: "com_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_productproducttags_gnr_products_ProductId",
                table: "gnr_productproducttags");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_productproducttags_gnr_producttags_ProductTagId",
                table: "gnr_productproducttags");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_products_gnr_brands_BrandId",
                table: "gnr_products");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_products_gnr_categories_CategoryId",
                table: "gnr_products");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_products_gnr_commodities_CommodityId",
                table: "gnr_products");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_productspecvalues_gnr_products_ProductId",
                table: "gnr_productspecvalues");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_productspecvalues_gnr_productspecdefinitions_SpecDefinit~",
                table: "gnr_productspecvalues");

            migrationBuilder.DropForeignKey(
                name: "FK_gnr_productvariants_gnr_products_ProductId",
                table: "gnr_productvariants");

            migrationBuilder.DropForeignKey(
                name: "FK_khb_productwoocontrolprofiles_gnr_products_ProductId",
                table: "khb_productwoocontrolprofiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ord_woocommerceorderitemsnapshots_ord_woocommerceordersnapsh~",
                table: "ord_woocommerceorderitemsnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_pay_barookpaymentsessions_ord_woocommerceordersnapshots_WooC~",
                table: "pay_barookpaymentsessions");

            migrationBuilder.DropForeignKey(
                name: "FK_pay_manualpaymentreceipts_ord_woocommerceordersnapshots_WooC~",
                table: "pay_manualpaymentreceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_sec_aspnetroleclaims_sec_aspnetroles_RoleId",
                table: "sec_aspnetroleclaims");

            migrationBuilder.DropForeignKey(
                name: "FK_sec_aspnetuserclaims_sec_aspnetusers_UserId",
                table: "sec_aspnetuserclaims");

            migrationBuilder.DropForeignKey(
                name: "FK_sec_aspnetuserlogins_sec_aspnetusers_UserId",
                table: "sec_aspnetuserlogins");

            migrationBuilder.DropForeignKey(
                name: "FK_sec_aspnetuserroles_sec_aspnetroles_RoleId",
                table: "sec_aspnetuserroles");

            migrationBuilder.DropForeignKey(
                name: "FK_sec_aspnetuserroles_sec_aspnetusers_UserId",
                table: "sec_aspnetuserroles");

            migrationBuilder.DropForeignKey(
                name: "FK_sec_aspnetusertokens_sec_aspnetusers_UserId",
                table: "sec_aspnetusertokens");

            migrationBuilder.DropForeignKey(
                name: "FK_wf_inventoryproposals_gnr_products_ProductId",
                table: "wf_inventoryproposals");

            migrationBuilder.DropForeignKey(
                name: "FK_wf_inventoryproposals_gnr_productvariants_ProductVariantId",
                table: "wf_inventoryproposals");

            migrationBuilder.DropForeignKey(
                name: "FK_wf_priceproposals_gnr_products_ProductId",
                table: "wf_priceproposals");

            migrationBuilder.DropForeignKey(
                name: "FK_wf_priceproposals_gnr_productvariants_ProductVariantId",
                table: "wf_priceproposals");

            migrationBuilder.DropTable(
                name: "all_product_with_process");

            migrationBuilder.DropTable(
                name: "khb_category_map");

            migrationBuilder.DropTable(
                name: "khb_commodity");

            migrationBuilder.DropTable(
                name: "khb_imported_woocommerce_records");

            migrationBuilder.DropTable(
                name: "khb_package_type");

            migrationBuilder.DropTable(
                name: "khb_product_change_log");

            migrationBuilder.DropTable(
                name: "khb_product_final");

            migrationBuilder.DropTable(
                name: "khb_product_main_groups");

            migrationBuilder.DropTable(
                name: "khb_product_price_history");

            migrationBuilder.DropTable(
                name: "khb_product_update_queue");

            migrationBuilder.DropTable(
                name: "khb_sale_products");

            migrationBuilder.DropTable(
                name: "khb_source_product");

            migrationBuilder.DropTable(
                name: "khb_woocommerce_connection_profiles");

            migrationBuilder.DropTable(
                name: "khb_workflow_job_logs");

            migrationBuilder.DropTable(
                name: "khb_workflow_jobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_wf_priceproposals",
                table: "wf_priceproposals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_wf_inventoryproposals",
                table: "wf_inventoryproposals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_wf_approvalauditlogs",
                table: "wf_approvalauditlogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sync_outboxmessages",
                table: "sync_outboxmessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sup_woocommercesynclogs",
                table: "sup_woocommercesynclogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sup_gatewaypaymentreceipts",
                table: "sup_gatewaypaymentreceipts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sec_aspnetusertokens",
                table: "sec_aspnetusertokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sec_aspnetusers",
                table: "sec_aspnetusers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sec_aspnetuserroles",
                table: "sec_aspnetuserroles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sec_aspnetuserlogins",
                table: "sec_aspnetuserlogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sec_aspnetuserclaims",
                table: "sec_aspnetuserclaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sec_aspnetroles",
                table: "sec_aspnetroles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sec_aspnetroleclaims",
                table: "sec_aspnetroleclaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_pay_manualpaymentreceipts",
                table: "pay_manualpaymentreceipts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_pay_barookpaymentsessions",
                table: "pay_barookpaymentsessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ord_woocommerceordersnapshots",
                table: "ord_woocommerceordersnapshots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ord_woocommerceorderitemsnapshots",
                table: "ord_woocommerceorderitemsnapshots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_khb_productwoocontrolprofiles",
                table: "khb_productwoocontrolprofiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_productvariants",
                table: "gnr_productvariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_producttags",
                table: "gnr_producttags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_productspecvalues",
                table: "gnr_productspecvalues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_productspecdefinitions",
                table: "gnr_productspecdefinitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_products",
                table: "gnr_products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_productproducttags",
                table: "gnr_productproducttags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_commodities",
                table: "gnr_commodities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_categories",
                table: "gnr_categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_gnr_brands",
                table: "gnr_brands");

            migrationBuilder.DropPrimaryKey(
                name: "PK_com_orders",
                table: "com_orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_com_orderitems",
                table: "com_orderitems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cbi_customers",
                table: "cbi_customers");

            migrationBuilder.RenameTable(
                name: "wf_priceproposals",
                newName: "wf_PriceProposals");

            migrationBuilder.RenameTable(
                name: "wf_inventoryproposals",
                newName: "wf_InventoryProposals");

            migrationBuilder.RenameTable(
                name: "wf_approvalauditlogs",
                newName: "wf_ApprovalAuditLogs");

            migrationBuilder.RenameTable(
                name: "sync_outboxmessages",
                newName: "sync_OutboxMessages");

            migrationBuilder.RenameTable(
                name: "sup_woocommercesynclogs",
                newName: "sup_WooCommerceSyncLogs");

            migrationBuilder.RenameTable(
                name: "sup_gatewaypaymentreceipts",
                newName: "sup_GatewayPaymentReceipts");

            migrationBuilder.RenameTable(
                name: "sec_aspnetusertokens",
                newName: "sec_AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "sec_aspnetusers",
                newName: "sec_AspNetUsers");

            migrationBuilder.RenameTable(
                name: "sec_aspnetuserroles",
                newName: "sec_AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "sec_aspnetuserlogins",
                newName: "sec_AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "sec_aspnetuserclaims",
                newName: "sec_AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "sec_aspnetroles",
                newName: "sec_AspNetRoles");

            migrationBuilder.RenameTable(
                name: "sec_aspnetroleclaims",
                newName: "sec_AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "pay_manualpaymentreceipts",
                newName: "pay_ManualPaymentReceipts");

            migrationBuilder.RenameTable(
                name: "pay_barookpaymentsessions",
                newName: "pay_BarookPaymentSessions");

            migrationBuilder.RenameTable(
                name: "ord_woocommerceordersnapshots",
                newName: "ord_WooCommerceOrderSnapshots");

            migrationBuilder.RenameTable(
                name: "ord_woocommerceorderitemsnapshots",
                newName: "ord_WooCommerceOrderItemSnapshots");

            migrationBuilder.RenameTable(
                name: "khb_productwoocontrolprofiles",
                newName: "khb_ProductWooControlProfiles");

            migrationBuilder.RenameTable(
                name: "gnr_productvariants",
                newName: "gnr_ProductVariants");

            migrationBuilder.RenameTable(
                name: "gnr_producttags",
                newName: "gnr_ProductTags");

            migrationBuilder.RenameTable(
                name: "gnr_productspecvalues",
                newName: "gnr_ProductSpecValues");

            migrationBuilder.RenameTable(
                name: "gnr_productspecdefinitions",
                newName: "gnr_ProductSpecDefinitions");

            migrationBuilder.RenameTable(
                name: "gnr_products",
                newName: "gnr_Products");

            migrationBuilder.RenameTable(
                name: "gnr_productproducttags",
                newName: "gnr_ProductProductTags");

            migrationBuilder.RenameTable(
                name: "gnr_commodities",
                newName: "gnr_Commodities");

            migrationBuilder.RenameTable(
                name: "gnr_categories",
                newName: "gnr_Categories");

            migrationBuilder.RenameTable(
                name: "gnr_brands",
                newName: "gnr_Brands");

            migrationBuilder.RenameTable(
                name: "com_orders",
                newName: "com_Orders");

            migrationBuilder.RenameTable(
                name: "com_orderitems",
                newName: "com_OrderItems");

            migrationBuilder.RenameTable(
                name: "cbi_customers",
                newName: "cbi_Customers");

            migrationBuilder.RenameIndex(
                name: "IX_wf_priceproposals_Status",
                table: "wf_PriceProposals",
                newName: "IX_wf_PriceProposals_Status");

            migrationBuilder.RenameIndex(
                name: "IX_wf_priceproposals_ProductVariantId",
                table: "wf_PriceProposals",
                newName: "IX_wf_PriceProposals_ProductVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_wf_priceproposals_ProductId",
                table: "wf_PriceProposals",
                newName: "IX_wf_PriceProposals_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_wf_priceproposals_CreatedAtUtc",
                table: "wf_PriceProposals",
                newName: "IX_wf_PriceProposals_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_wf_inventoryproposals_Status",
                table: "wf_InventoryProposals",
                newName: "IX_wf_InventoryProposals_Status");

            migrationBuilder.RenameIndex(
                name: "IX_wf_inventoryproposals_ProductVariantId",
                table: "wf_InventoryProposals",
                newName: "IX_wf_InventoryProposals_ProductVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_wf_inventoryproposals_ProductId",
                table: "wf_InventoryProposals",
                newName: "IX_wf_InventoryProposals_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_wf_inventoryproposals_CreatedAtUtc",
                table: "wf_InventoryProposals",
                newName: "IX_wf_InventoryProposals_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_wf_approvalauditlogs_EntityType_EntityId",
                table: "wf_ApprovalAuditLogs",
                newName: "IX_wf_ApprovalAuditLogs_EntityType_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_wf_approvalauditlogs_CreatedAtUtc",
                table: "wf_ApprovalAuditLogs",
                newName: "IX_wf_ApprovalAuditLogs_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_sync_outboxmessages_Status",
                table: "sync_OutboxMessages",
                newName: "IX_sync_OutboxMessages_Status");

            migrationBuilder.RenameIndex(
                name: "IX_sync_outboxmessages_CreatedAtUtc",
                table: "sync_OutboxMessages",
                newName: "IX_sync_OutboxMessages_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_sync_outboxmessages_AggregateType_AggregateId",
                table: "sync_OutboxMessages",
                newName: "IX_sync_OutboxMessages_AggregateType_AggregateId");

            migrationBuilder.RenameIndex(
                name: "IX_sup_woocommercesynclogs_Operation_Status",
                table: "sup_WooCommerceSyncLogs",
                newName: "IX_sup_WooCommerceSyncLogs_Operation_Status");

            migrationBuilder.RenameIndex(
                name: "IX_sup_woocommercesynclogs_CreatedAtUtc",
                table: "sup_WooCommerceSyncLogs",
                newName: "IX_sup_WooCommerceSyncLogs_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_sup_gatewaypaymentreceipts_WooCommerceOrderId",
                table: "sup_GatewayPaymentReceipts",
                newName: "IX_sup_GatewayPaymentReceipts_WooCommerceOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_sup_gatewaypaymentreceipts_TransactionId",
                table: "sup_GatewayPaymentReceipts",
                newName: "IX_sup_GatewayPaymentReceipts_TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_sup_gatewaypaymentreceipts_IdempotencyKey",
                table: "sup_GatewayPaymentReceipts",
                newName: "IX_sup_GatewayPaymentReceipts_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "IX_sec_aspnetuserroles_RoleId",
                table: "sec_AspNetUserRoles",
                newName: "IX_sec_AspNetUserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_sec_aspnetuserlogins_UserId",
                table: "sec_AspNetUserLogins",
                newName: "IX_sec_AspNetUserLogins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_sec_aspnetuserclaims_UserId",
                table: "sec_AspNetUserClaims",
                newName: "IX_sec_AspNetUserClaims_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_sec_aspnetroleclaims_RoleId",
                table: "sec_AspNetRoleClaims",
                newName: "IX_sec_AspNetRoleClaims_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_pay_manualpaymentreceipts_WooCommerceOrderSnapshotId",
                table: "pay_ManualPaymentReceipts",
                newName: "IX_pay_ManualPaymentReceipts_WooCommerceOrderSnapshotId");

            migrationBuilder.RenameIndex(
                name: "IX_pay_manualpaymentreceipts_WooCommerceOrderId",
                table: "pay_ManualPaymentReceipts",
                newName: "IX_pay_ManualPaymentReceipts_WooCommerceOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_pay_manualpaymentreceipts_ReceiptNumber",
                table: "pay_ManualPaymentReceipts",
                newName: "IX_pay_ManualPaymentReceipts_ReceiptNumber");

            migrationBuilder.RenameIndex(
                name: "IX_pay_barookpaymentsessions_WooCommerceOrderSnapshotId",
                table: "pay_BarookPaymentSessions",
                newName: "IX_pay_BarookPaymentSessions_WooCommerceOrderSnapshotId");

            migrationBuilder.RenameIndex(
                name: "IX_pay_barookpaymentsessions_WooCommerceOrderId",
                table: "pay_BarookPaymentSessions",
                newName: "IX_pay_BarookPaymentSessions_WooCommerceOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_pay_barookpaymentsessions_Token",
                table: "pay_BarookPaymentSessions",
                newName: "IX_pay_BarookPaymentSessions_Token");

            migrationBuilder.RenameIndex(
                name: "IX_pay_barookpaymentsessions_ExternalCode",
                table: "pay_BarookPaymentSessions",
                newName: "IX_pay_BarookPaymentSessions_ExternalCode");

            migrationBuilder.RenameIndex(
                name: "IX_pay_barookpaymentsessions_BarookStatus",
                table: "pay_BarookPaymentSessions",
                newName: "IX_pay_BarookPaymentSessions_BarookStatus");

            migrationBuilder.RenameIndex(
                name: "IX_ord_woocommerceordersnapshots_WooCreatedAtUtc",
                table: "ord_WooCommerceOrderSnapshots",
                newName: "IX_ord_WooCommerceOrderSnapshots_WooCreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_ord_woocommerceordersnapshots_WooCommerceStatus",
                table: "ord_WooCommerceOrderSnapshots",
                newName: "IX_ord_WooCommerceOrderSnapshots_WooCommerceStatus");

            migrationBuilder.RenameIndex(
                name: "IX_ord_woocommerceordersnapshots_WooCommerceOrderNumber",
                table: "ord_WooCommerceOrderSnapshots",
                newName: "IX_ord_WooCommerceOrderSnapshots_WooCommerceOrderNumber");

            migrationBuilder.RenameIndex(
                name: "IX_ord_woocommerceordersnapshots_WooCommerceOrderId",
                table: "ord_WooCommerceOrderSnapshots",
                newName: "IX_ord_WooCommerceOrderSnapshots_WooCommerceOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ord_woocommerceordersnapshots_PaymentStatus",
                table: "ord_WooCommerceOrderSnapshots",
                newName: "IX_ord_WooCommerceOrderSnapshots_PaymentStatus");

            migrationBuilder.RenameIndex(
                name: "IX_ord_woocommerceordersnapshots_InternalStatus",
                table: "ord_WooCommerceOrderSnapshots",
                newName: "IX_ord_WooCommerceOrderSnapshots_InternalStatus");

            migrationBuilder.RenameIndex(
                name: "IX_ord_woocommerceordersnapshots_CustomerPhone",
                table: "ord_WooCommerceOrderSnapshots",
                newName: "IX_ord_WooCommerceOrderSnapshots_CustomerPhone");

            migrationBuilder.RenameIndex(
                name: "IX_ord_woocommerceordersnapshots_CustomerNationalCode",
                table: "ord_WooCommerceOrderSnapshots",
                newName: "IX_ord_WooCommerceOrderSnapshots_CustomerNationalCode");

            migrationBuilder.RenameIndex(
                name: "IX_ord_woocommerceorderitemsnapshots_WooCommerceVariationId",
                table: "ord_WooCommerceOrderItemSnapshots",
                newName: "IX_ord_WooCommerceOrderItemSnapshots_WooCommerceVariationId");

            migrationBuilder.RenameIndex(
                name: "IX_ord_woocommerceorderitemsnapshots_WooCommerceProductId",
                table: "ord_WooCommerceOrderItemSnapshots",
                newName: "IX_ord_WooCommerceOrderItemSnapshots_WooCommerceProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ord_woocommerceorderitemsnapshots_WooCommerceOrderSnapshotId~",
                table: "ord_WooCommerceOrderItemSnapshots",
                newName: "IX_ord_WooCommerceOrderItemSnapshots_WooCommerceOrderSnapshotId~");

            migrationBuilder.RenameIndex(
                name: "IX_khb_productwoocontrolprofiles_WooSyncStatus",
                table: "khb_ProductWooControlProfiles",
                newName: "IX_khb_ProductWooControlProfiles_WooSyncStatus");

            migrationBuilder.RenameIndex(
                name: "IX_khb_productwoocontrolprofiles_ProductId",
                table: "khb_ProductWooControlProfiles",
                newName: "IX_khb_ProductWooControlProfiles_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_khb_productwoocontrolprofiles_PriceCheckStatus",
                table: "khb_ProductWooControlProfiles",
                newName: "IX_khb_ProductWooControlProfiles_PriceCheckStatus");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_productvariants_WooCommerceProductId_WooCommerceVariatio~",
                table: "gnr_ProductVariants",
                newName: "IX_gnr_ProductVariants_WooCommerceProductId_WooCommerceVariatio~");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_productvariants_Sku",
                table: "gnr_ProductVariants",
                newName: "IX_gnr_ProductVariants_Sku");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_productvariants_ProductId_Name",
                table: "gnr_ProductVariants",
                newName: "IX_gnr_ProductVariants_ProductId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_producttags_Slug",
                table: "gnr_ProductTags",
                newName: "IX_gnr_ProductTags_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_productspecvalues_SpecDefinitionId",
                table: "gnr_ProductSpecValues",
                newName: "IX_gnr_ProductSpecValues_SpecDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_productspecvalues_ProductId_SpecDefinitionId",
                table: "gnr_ProductSpecValues",
                newName: "IX_gnr_ProductSpecValues_ProductId_SpecDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_productspecdefinitions_Slug",
                table: "gnr_ProductSpecDefinitions",
                newName: "IX_gnr_ProductSpecDefinitions_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_products_WooCommerceProductId",
                table: "gnr_Products",
                newName: "IX_gnr_Products_WooCommerceProductId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_products_Slug",
                table: "gnr_Products",
                newName: "IX_gnr_Products_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_products_Sku",
                table: "gnr_Products",
                newName: "IX_gnr_Products_Sku");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_products_CommodityId",
                table: "gnr_Products",
                newName: "IX_gnr_Products_CommodityId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_products_CategoryId",
                table: "gnr_Products",
                newName: "IX_gnr_Products_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_products_BrandId",
                table: "gnr_Products",
                newName: "IX_gnr_Products_BrandId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_productproducttags_ProductTagId",
                table: "gnr_ProductProductTags",
                newName: "IX_gnr_ProductProductTags_ProductTagId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_commodities_WooCommerceCommodityId",
                table: "gnr_Commodities",
                newName: "IX_gnr_Commodities_WooCommerceCommodityId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_commodities_Slug",
                table: "gnr_Commodities",
                newName: "IX_gnr_Commodities_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_categories_WooCommerceCategoryId",
                table: "gnr_Categories",
                newName: "IX_gnr_Categories_WooCommerceCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_categories_Slug",
                table: "gnr_Categories",
                newName: "IX_gnr_Categories_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_brands_WooCommerceBrandId",
                table: "gnr_Brands",
                newName: "IX_gnr_Brands_WooCommerceBrandId");

            migrationBuilder.RenameIndex(
                name: "IX_gnr_brands_Slug",
                table: "gnr_Brands",
                newName: "IX_gnr_Brands_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_com_orders_WooCommerceOrderId",
                table: "com_Orders",
                newName: "IX_com_Orders_WooCommerceOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_com_orders_CustomerId",
                table: "com_Orders",
                newName: "IX_com_Orders_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_com_orders_CreatedAt",
                table: "com_Orders",
                newName: "IX_com_Orders_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_com_orderitems_ProductVariantId",
                table: "com_OrderItems",
                newName: "IX_com_OrderItems_ProductVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_com_orderitems_ProductId",
                table: "com_OrderItems",
                newName: "IX_com_OrderItems_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_com_orderitems_OrderId",
                table: "com_OrderItems",
                newName: "IX_com_OrderItems_OrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_wf_PriceProposals",
                table: "wf_PriceProposals",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_wf_InventoryProposals",
                table: "wf_InventoryProposals",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_wf_ApprovalAuditLogs",
                table: "wf_ApprovalAuditLogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sync_OutboxMessages",
                table: "sync_OutboxMessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sup_WooCommerceSyncLogs",
                table: "sup_WooCommerceSyncLogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sup_GatewayPaymentReceipts",
                table: "sup_GatewayPaymentReceipts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sec_AspNetUserTokens",
                table: "sec_AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_sec_AspNetUsers",
                table: "sec_AspNetUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sec_AspNetUserRoles",
                table: "sec_AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_sec_AspNetUserLogins",
                table: "sec_AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_sec_AspNetUserClaims",
                table: "sec_AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sec_AspNetRoles",
                table: "sec_AspNetRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sec_AspNetRoleClaims",
                table: "sec_AspNetRoleClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_pay_ManualPaymentReceipts",
                table: "pay_ManualPaymentReceipts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_pay_BarookPaymentSessions",
                table: "pay_BarookPaymentSessions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ord_WooCommerceOrderSnapshots",
                table: "ord_WooCommerceOrderSnapshots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ord_WooCommerceOrderItemSnapshots",
                table: "ord_WooCommerceOrderItemSnapshots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_khb_ProductWooControlProfiles",
                table: "khb_ProductWooControlProfiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_ProductVariants",
                table: "gnr_ProductVariants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_ProductTags",
                table: "gnr_ProductTags",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_ProductSpecValues",
                table: "gnr_ProductSpecValues",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_ProductSpecDefinitions",
                table: "gnr_ProductSpecDefinitions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_Products",
                table: "gnr_Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_ProductProductTags",
                table: "gnr_ProductProductTags",
                columns: new[] { "ProductId", "ProductTagId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_Commodities",
                table: "gnr_Commodities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_Categories",
                table: "gnr_Categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gnr_Brands",
                table: "gnr_Brands",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_com_Orders",
                table: "com_Orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_com_OrderItems",
                table: "com_OrderItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cbi_Customers",
                table: "cbi_Customers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_com_OrderItems_com_Orders_OrderId",
                table: "com_OrderItems",
                column: "OrderId",
                principalTable: "com_Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_com_OrderItems_gnr_ProductVariants_ProductVariantId",
                table: "com_OrderItems",
                column: "ProductVariantId",
                principalTable: "gnr_ProductVariants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_com_OrderItems_gnr_Products_ProductId",
                table: "com_OrderItems",
                column: "ProductId",
                principalTable: "gnr_Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_com_Orders_cbi_Customers_CustomerId",
                table: "com_Orders",
                column: "CustomerId",
                principalTable: "cbi_Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_ProductProductTags_gnr_ProductTags_ProductTagId",
                table: "gnr_ProductProductTags",
                column: "ProductTagId",
                principalTable: "gnr_ProductTags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_ProductProductTags_gnr_Products_ProductId",
                table: "gnr_ProductProductTags",
                column: "ProductId",
                principalTable: "gnr_Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_Products_gnr_Brands_BrandId",
                table: "gnr_Products",
                column: "BrandId",
                principalTable: "gnr_Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_Products_gnr_Categories_CategoryId",
                table: "gnr_Products",
                column: "CategoryId",
                principalTable: "gnr_Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_Products_gnr_Commodities_CommodityId",
                table: "gnr_Products",
                column: "CommodityId",
                principalTable: "gnr_Commodities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_ProductSpecValues_gnr_ProductSpecDefinitions_SpecDefinit~",
                table: "gnr_ProductSpecValues",
                column: "SpecDefinitionId",
                principalTable: "gnr_ProductSpecDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_ProductSpecValues_gnr_Products_ProductId",
                table: "gnr_ProductSpecValues",
                column: "ProductId",
                principalTable: "gnr_Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_gnr_ProductVariants_gnr_Products_ProductId",
                table: "gnr_ProductVariants",
                column: "ProductId",
                principalTable: "gnr_Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_khb_ProductWooControlProfiles_gnr_Products_ProductId",
                table: "khb_ProductWooControlProfiles",
                column: "ProductId",
                principalTable: "gnr_Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ord_WooCommerceOrderItemSnapshots_ord_WooCommerceOrderSnapsh~",
                table: "ord_WooCommerceOrderItemSnapshots",
                column: "WooCommerceOrderSnapshotId",
                principalTable: "ord_WooCommerceOrderSnapshots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_pay_BarookPaymentSessions_ord_WooCommerceOrderSnapshots_WooC~",
                table: "pay_BarookPaymentSessions",
                column: "WooCommerceOrderSnapshotId",
                principalTable: "ord_WooCommerceOrderSnapshots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_pay_ManualPaymentReceipts_ord_WooCommerceOrderSnapshots_WooC~",
                table: "pay_ManualPaymentReceipts",
                column: "WooCommerceOrderSnapshotId",
                principalTable: "ord_WooCommerceOrderSnapshots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sec_AspNetRoleClaims_sec_AspNetRoles_RoleId",
                table: "sec_AspNetRoleClaims",
                column: "RoleId",
                principalTable: "sec_AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sec_AspNetUserClaims_sec_AspNetUsers_UserId",
                table: "sec_AspNetUserClaims",
                column: "UserId",
                principalTable: "sec_AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sec_AspNetUserLogins_sec_AspNetUsers_UserId",
                table: "sec_AspNetUserLogins",
                column: "UserId",
                principalTable: "sec_AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sec_AspNetUserRoles_sec_AspNetRoles_RoleId",
                table: "sec_AspNetUserRoles",
                column: "RoleId",
                principalTable: "sec_AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sec_AspNetUserRoles_sec_AspNetUsers_UserId",
                table: "sec_AspNetUserRoles",
                column: "UserId",
                principalTable: "sec_AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sec_AspNetUserTokens_sec_AspNetUsers_UserId",
                table: "sec_AspNetUserTokens",
                column: "UserId",
                principalTable: "sec_AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_wf_InventoryProposals_gnr_ProductVariants_ProductVariantId",
                table: "wf_InventoryProposals",
                column: "ProductVariantId",
                principalTable: "gnr_ProductVariants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_wf_InventoryProposals_gnr_Products_ProductId",
                table: "wf_InventoryProposals",
                column: "ProductId",
                principalTable: "gnr_Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_wf_PriceProposals_gnr_ProductVariants_ProductVariantId",
                table: "wf_PriceProposals",
                column: "ProductVariantId",
                principalTable: "gnr_ProductVariants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_wf_PriceProposals_gnr_Products_ProductId",
                table: "wf_PriceProposals",
                column: "ProductId",
                principalTable: "gnr_Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
#endif
            // Forward-only migration: rollback is intentionally a no-op to preserve local data.
        }
    }
}
