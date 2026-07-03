START TRANSACTION;

SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_AspNetUsers'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_aspnetusers'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `sec_AspNetUsers` TO `sec_aspnetusers`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_AspNetRoles'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_aspnetroles'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `sec_AspNetRoles` TO `sec_aspnetroles`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_AspNetUserRoles'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_aspnetuserroles'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `sec_AspNetUserRoles` TO `sec_aspnetuserroles`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_AspNetUserClaims'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_aspnetuserclaims'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `sec_AspNetUserClaims` TO `sec_aspnetuserclaims`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_AspNetUserLogins'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_aspnetuserlogins'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `sec_AspNetUserLogins` TO `sec_aspnetuserlogins`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_AspNetRoleClaims'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_aspnetroleclaims'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `sec_AspNetRoleClaims` TO `sec_aspnetroleclaims`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_AspNetUserTokens'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sec_aspnetusertokens'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `sec_AspNetUserTokens` TO `sec_aspnetusertokens`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_Brands'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_brands'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `gnr_Brands` TO `gnr_brands`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_Categories'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_categories'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `gnr_Categories` TO `gnr_categories`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_Commodities'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_commodities'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `gnr_Commodities` TO `gnr_commodities`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_Products'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_products'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `gnr_Products` TO `gnr_products`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_ProductVariants'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_productvariants'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `gnr_ProductVariants` TO `gnr_productvariants`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_ProductTags'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_producttags'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `gnr_ProductTags` TO `gnr_producttags`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_ProductProductTags'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_productproducttags'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `gnr_ProductProductTags` TO `gnr_productproducttags`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_ProductSpecDefinitions'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_productspecdefinitions'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `gnr_ProductSpecDefinitions` TO `gnr_productspecdefinitions`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_ProductSpecValues'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'gnr_productspecvalues'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `gnr_ProductSpecValues` TO `gnr_productspecvalues`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'cbi_Customers'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'cbi_customers'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `cbi_Customers` TO `cbi_customers`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'com_Orders'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'com_orders'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `com_Orders` TO `com_orders`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'com_OrderItems'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'com_orderitems'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `com_OrderItems` TO `com_orderitems`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'wf_PriceProposals'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'wf_priceproposals'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `wf_PriceProposals` TO `wf_priceproposals`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'wf_InventoryProposals'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'wf_inventoryproposals'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `wf_InventoryProposals` TO `wf_inventoryproposals`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'wf_ApprovalAuditLogs'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'wf_approvalauditlogs'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `wf_ApprovalAuditLogs` TO `wf_approvalauditlogs`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sync_OutboxMessages'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sync_outboxmessages'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `sync_OutboxMessages` TO `sync_outboxmessages`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sup_WooCommerceSyncLogs'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sup_woocommercesynclogs'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `sup_WooCommerceSyncLogs` TO `sup_woocommercesynclogs`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sup_GatewayPaymentReceipts'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'sup_gatewaypaymentreceipts'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `sup_GatewayPaymentReceipts` TO `sup_gatewaypaymentreceipts`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'ord_WooCommerceOrderSnapshots'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'ord_woocommerceordersnapshots'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `ord_WooCommerceOrderSnapshots` TO `ord_woocommerceordersnapshots`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'ord_WooCommerceOrderItemSnapshots'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'ord_woocommerceorderitemsnapshots'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `ord_WooCommerceOrderItemSnapshots` TO `ord_woocommerceorderitemsnapshots`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'pay_BarookPaymentSessions'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'pay_barookpaymentsessions'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `pay_BarookPaymentSessions` TO `pay_barookpaymentsessions`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'pay_ManualPaymentReceipts'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'pay_manualpaymentreceipts'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `pay_ManualPaymentReceipts` TO `pay_manualpaymentreceipts`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_ProductWooControlProfiles'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_productwoocontrolprofiles'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `khb_ProductWooControlProfiles` TO `khb_productwoocontrolprofiles`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'All_Product_With_Process'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'all_product_with_process'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `All_Product_With_Process` TO `all_product_with_process`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'KHB_Source_Product'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `KHB_Source_Product` TO `khb_source_product`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'KHB_Category_Map'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_category_map'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `KHB_Category_Map` TO `khb_category_map`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'KHB_Commodity'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_commodity'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `KHB_Commodity` TO `khb_commodity`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'KHB_Package_Type'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_package_type'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `KHB_Package_Type` TO `khb_package_type`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'KHB_Product_Final'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `KHB_Product_Final` TO `khb_product_final`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'KHB_Product_Update_Queue'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `KHB_Product_Update_Queue` TO `khb_product_update_queue`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_legacy_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'KHB_Product_Price_History'
);
SET @khb_canonical_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_price_history'
);
SET @khb_sql = IF(
  @khb_legacy_exists = 1 AND @khb_canonical_exists = 0,
  'RENAME TABLE `KHB_Product_Price_History` TO `khb_product_price_history`',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


CREATE TABLE IF NOT EXISTS `all_product_with_process` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ImportBatchId` VARCHAR(64) NULL,
  `SourceRowNumber` INT NULL,
  `SourceRowHash` CHAR(64) NOT NULL,
  `RawJson` LONGTEXT NULL,
  `MainProductName` VARCHAR(500) NULL,
  `MainProductSlug` VARCHAR(500) NULL,
  `GroupName` VARCHAR(500) NULL,
  `CategoryName` VARCHAR(500) NULL,
  `CategorySlug` VARCHAR(500) NULL,
  `ProductName` VARCHAR(700) NULL,
  `ProductEnglishName` VARCHAR(700) NULL,
  `ProductSlug` VARCHAR(700) NULL,
  `SKU` VARCHAR(191) NULL,
  `BrandName` VARCHAR(300) NULL,
  `BrandEnglishName` VARCHAR(300) NULL,
  `PackageName` VARCHAR(300) NULL,
  `UnitWeight` DECIMAL(18,6) NULL,
  `PacksPerCarton` INT NULL,
  `CartonQuantity` INT NULL,
  `PackagingPricePerPack` DECIMAL(18,2) NULL,
  `SalePriceCash` DECIMAL(18,2) NULL,
  `SalePriceInstallment` DECIMAL(18,2) NULL,
  `PurchasePriceCash` DECIMAL(18,2) NULL,
  `PurchasePriceInstallment` DECIMAL(18,2) NULL,
  `ShortDescription` LONGTEXT NULL,
  `FullDescription` LONGTEXT NULL,
  `ImageUrl` LONGTEXT NULL,
  `GalleryJson` LONGTEXT NULL,
  `Status` VARCHAR(100) NULL,
  `WooProductId` BIGINT NULL,
  `HaveOtherPackage` TINYINT(1) NULL,
  `PackageOne` VARCHAR(300) NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_All_Product_With_Process_SourceRowHash` (`SourceRowHash`),
  KEY `IX_All_Product_With_Process_ProductName` (`ProductName`(191)),
  KEY `IX_All_Product_With_Process_MainProductName` (`MainProductName`(191)),
  KEY `IX_All_Product_With_Process_SKU` (`SKU`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `khb_product_main_groups` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `MainProductName` VARCHAR(500) NULL,
  `MainProductSlug` VARCHAR(500) NULL,
  `CategoryName` VARCHAR(500) NULL,
  `EnTaxonomic` VARCHAR(500) NULL,
  `CategorySlug` VARCHAR(500) NULL,
  `Description` LONGTEXT NULL,
  `ImageUrl` LONGTEXT NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `SourceKey` VARCHAR(500) NULL,
  `Name` VARCHAR(500) NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_khb_product_main_groups_slug` (`MainProductSlug`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `khb_sale_products` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `MainGroupId` BIGINT NULL,
  `SourceRowHash` CHAR(64) NOT NULL,
  `WooProductId` BIGINT NULL,
  `ProductName` VARCHAR(700) NULL,
  `ProductEnglishName` VARCHAR(700) NULL,
  `ProductSlug` VARCHAR(700) NULL,
  `SKU` VARCHAR(191) NULL,
  `BrandName` VARCHAR(300) NULL,
  `BrandEnglishName` VARCHAR(300) NULL,
  `PackageName` VARCHAR(300) NULL,
  `PackagingGroup` VARCHAR(50) NULL,
  `PackageCode` VARCHAR(50) NULL,
  `UnitWeight` DECIMAL(18,6) NULL,
  `PacksPerCarton` INT NULL,
  `CartonQuantity` INT NULL,
  `PackagingPricePerPack` DECIMAL(18,2) NULL,
  `KgPriceCash` DECIMAL(18,2) NULL,
  `KgPriceInstallment` DECIMAL(18,2) NULL,
  `SalePriceCash` DECIMAL(18,2) NULL,
  `SalePriceInstallment` DECIMAL(18,2) NULL,
  `PurchasePriceCash` DECIMAL(18,2) NULL,
  `PurchasePriceInstallment` DECIMAL(18,2) NULL,
  `ShortDescription` LONGTEXT NULL,
  `FullDescription` LONGTEXT NULL,
  `ImageUrl` LONGTEXT NULL,
  `GalleryJson` LONGTEXT NULL,
  `Status` VARCHAR(100) NOT NULL DEFAULT 'draft',
  `RawJson` LONGTEXT NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `SaleMode` VARCHAR(80) NULL,
  `PriceCalculationBasis` VARCHAR(80) NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_khb_sale_products_hash` (`SourceRowHash`),
  KEY `IX_khb_sale_products_woo` (`WooProductId`),
  KEY `IX_khb_sale_products_sku` (`SKU`),
  KEY `IX_khb_sale_products_name` (`ProductName`(191))
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `khb_source_product` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `SourceKey` CHAR(64) NOT NULL,
  `SourceRowId` BIGINT NULL,
  `ProductName` VARCHAR(700) NULL,
  `ProductEnglishName` VARCHAR(700) NULL,
  `MainProductName` VARCHAR(500) NULL,
  `CategoryName` VARCHAR(500) NULL,
  `CategorySlug` VARCHAR(500) NULL,
  `BrandName` VARCHAR(300) NULL,
  `BrandEnglishName` VARCHAR(300) NULL,
  `PackageOne` VARCHAR(300) NULL,
  `UnitWeightKg` DECIMAL(18,6) NULL,
  `KgCashPrice` DECIMAL(18,2) NULL,
  `KgCreditPrice` DECIMAL(18,2) NULL,
  `RawJson` LONGTEXT NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Source_Product_SourceKey` (`SourceKey`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `khb_category_map` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `SourceKey` VARCHAR(500) NOT NULL,
  `CategoryName` VARCHAR(500) NULL,
  `CategorySlug` VARCHAR(500) NULL,
  `WooCategoryId` BIGINT NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Category_Map_SourceKey` (`SourceKey`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `khb_commodity` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `SourceKey` VARCHAR(500) NOT NULL,
  `CommodityName` VARCHAR(500) NULL,
  `CommoditySlug` VARCHAR(500) NULL,
  `WooCommodityId` BIGINT NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Commodity_SourceKey` (`SourceKey`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `khb_package_type` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `SourceKey` VARCHAR(500) NOT NULL,
  `PackageGroup` VARCHAR(50) NULL,
  `PackageCode` VARCHAR(50) NULL,
  `PackageTitle` VARCHAR(300) NULL,
  `UnitWeightKg` DECIMAL(18,6) NULL,
  `PacksPerCarton` INT NULL,
  `PackagingPricePerPack` DECIMAL(18,2) NULL,
  `WooPackageId` BIGINT NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Package_Type_SourceKey` (`SourceKey`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `khb_product_final` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `SourceKey` CHAR(64) NOT NULL,
  `MainGroupId` BIGINT NULL,
  `CategorySourceKey` VARCHAR(500) NULL,
  `CommoditySourceKey` VARCHAR(500) NULL,
  `PackageSourceKey` VARCHAR(500) NULL,
  `ProductName` VARCHAR(700) NULL,
  `ProductEnglishName` VARCHAR(700) NULL,
  `ProductSlug` VARCHAR(700) NULL,
  `WooProductId` BIGINT NULL,
  `SKU` VARCHAR(191) NULL,
  `PackageGroup` VARCHAR(50) NULL,
  `PackageCode` VARCHAR(50) NULL,
  `UnitWeightKg` DECIMAL(18,6) NULL,
  `PacksPerCarton` INT NULL,
  `PackagingPricePerPack` DECIMAL(18,2) NULL,
  `KgCashPrice` DECIMAL(18,2) NULL,
  `KgCreditPrice` DECIMAL(18,2) NULL,
  `SaleCashPrice` DECIMAL(18,2) NULL,
  `SaleCreditPrice` DECIMAL(18,2) NULL,
  `BuyCashPrice` DECIMAL(18,2) NULL,
  `BuyCreditPrice` DECIMAL(18,2) NULL,
  `Status` VARCHAR(100) NULL,
  `CatalogVisibility` VARCHAR(50) NULL,
  `WooPayloadJson` LONGTEXT NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `BrandName` VARCHAR(300) NULL,
  `BrandEnglishName` VARCHAR(300) NULL,
  `PackageTitle` VARCHAR(300) NULL,
  `BulkWeightKg` DECIMAL(18,6) NULL,
  `MinPurchaseKg` DECIMAL(18,6) NULL,
  `ImageTag` VARCHAR(300) NULL,
  `SaleMode` VARCHAR(80) NULL,
  `PriceCalculationBasis` VARCHAR(80) NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Product_Final_SourceKey` (`SourceKey`),
  KEY `IX_KHB_Product_Final_SKU` (`SKU`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `khb_product_update_queue` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `SourceKey` CHAR(64) NOT NULL,
  `EntityType` VARCHAR(80) NOT NULL DEFAULT 'product',
  `QueueStatus` VARCHAR(50) NOT NULL DEFAULT 'pending',
  `ActionType` VARCHAR(50) NOT NULL DEFAULT 'upsert',
  `SKU` VARCHAR(191) NULL,
  `ProductSlug` VARCHAR(700) NULL,
  `WooProductId` BIGINT NULL,
  `WooPayloadJson` LONGTEXT NULL,
  `LastError` LONGTEXT NULL,
  `JobId` CHAR(36) NULL,
  `TryCount` INT NOT NULL DEFAULT 0,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_KHB_Product_Update_Queue_SourceKey` (`SourceKey`),
  KEY `IX_KHB_Product_Update_Queue_Status` (`QueueStatus`),
  KEY `IX_khb_product_update_queue_JobId` (`JobId`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `khb_product_price_history` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ProductSourceKey` CHAR(64) NOT NULL,
  `ProductName` VARCHAR(700) NULL,
  `SKU` VARCHAR(191) NULL,
  `ProductType` VARCHAR(80) NOT NULL,
  `PackageGroup` VARCHAR(50) NULL,
  `PackageCode` VARCHAR(50) NULL,
  `PriceType` VARCHAR(80) NOT NULL,
  `PriceAmount` DECIMAL(18,2) NOT NULL,
  `CurrencyCode` VARCHAR(20) NOT NULL DEFAULT 'TOMAN',
  `ValidFromUtc` DATETIME(6) NOT NULL,
  `ValidToUtc` DATETIME(6) NULL,
  `IsCurrent` TINYINT(1) NOT NULL DEFAULT 1,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `IX_KHB_Product_Price_History_Product` (`ProductSourceKey`,`ProductType`,`PriceType`,`IsCurrent`),
  KEY `IX_KHB_Product_Price_History_SKU` (`SKU`),
  KEY `IX_KHB_Product_Price_History_Date` (`ValidFromUtc`,`ValidToUtc`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `khb_product_change_log` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ProductId` BIGINT NOT NULL,
  `ChangeType` VARCHAR(100) NOT NULL,
  `Summary` VARCHAR(1000) NULL,
  `Payload` LONGTEXT NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `IX_khb_product_change_log_ProductId` (`ProductId`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `khb_imported_woocommerce_records` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `SourceType` VARCHAR(64) NOT NULL,
  `ExternalId` VARCHAR(191) NULL,
  `Slug` VARCHAR(255) NULL,
  `Title` VARCHAR(512) NULL,
  `RawJson` LONGTEXT NOT NULL,
  `ImportedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `SourceUrl` TEXT NULL,
  `Name` VARCHAR(512) NULL,
  `Status` VARCHAR(128) NULL,
  `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `khb_workflow_jobs` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `JobId` CHAR(36) NOT NULL,
  `Type` VARCHAR(50) NOT NULL,
  `Status` VARCHAR(50) NOT NULL DEFAULT 'pending',
  `CurrentStep` VARCHAR(160) NOT NULL DEFAULT 'Pending',
  `TotalItems` INT NOT NULL DEFAULT 0,
  `ProcessedItems` INT NOT NULL DEFAULT 0,
  `SuccessCount` INT NOT NULL DEFAULT 0,
  `ErrorCount` INT NOT NULL DEFAULT 0,
  `DraftCount` INT NOT NULL DEFAULT 0,
  `SkippedCount` INT NOT NULL DEFAULT 0,
  `PendingCount` INT NOT NULL DEFAULT 0,
  `ProgressPercent` INT NOT NULL DEFAULT 0,
  `Message` VARCHAR(2000) NULL,
  `CreatedBy` VARCHAR(256) NULL,
  `StartedAtUtc` DATETIME(6) NULL,
  `FinishedAtUtc` DATETIME(6) NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_khb_workflow_jobs_JobId` (`JobId`),
  KEY `IX_khb_workflow_jobs_Type_CreatedAtUtc` (`Type`,`CreatedAtUtc`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `khb_workflow_job_logs` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `WorkflowJobId` BIGINT NOT NULL,
  `JobId` CHAR(36) NOT NULL,
  `StepName` VARCHAR(160) NOT NULL,
  `EntityType` VARCHAR(100) NULL,
  `EntityId` VARCHAR(191) NULL,
  `SKU` VARCHAR(191) NULL,
  `Status` VARCHAR(50) NOT NULL,
  `Message` VARCHAR(4000) NULL,
  `RequestUrl` VARCHAR(2000) NULL,
  `ResponseCode` INT NULL,
  `ResponseBodySummary` VARCHAR(4000) NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `IX_khb_workflow_job_logs_JobId_CreatedAtUtc` (`JobId`,`CreatedAtUtc`),
  KEY `IX_khb_workflow_job_logs_WorkflowJobId` (`WorkflowJobId`),
  CONSTRAINT `FK_khb_workflow_job_logs_khb_workflow_jobs_WorkflowJobId`
    FOREIGN KEY (`WorkflowJobId`) REFERENCES `khb_workflow_jobs` (`Id`) ON DELETE CASCADE
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `khb_woocommerce_connection_profiles` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `ProfileName` VARCHAR(160) NOT NULL,
  `EnvironmentType` VARCHAR(20) NOT NULL,
  `BaseUrl` VARCHAR(1000) NOT NULL,
  `ConsumerKey` VARCHAR(255) NOT NULL,
  `ProtectedConsumerSecret` LONGTEXT NOT NULL,
  `ApiVersion` VARCHAR(40) NOT NULL DEFAULT 'wc/v3',
  `VerifySsl` TINYINT(1) NOT NULL DEFAULT 1,
  `TimeoutSeconds` INT NOT NULL DEFAULT 30,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 0,
  `LastTestedAtUtc` DATETIME(6) NULL,
  `LastTestSucceeded` TINYINT(1) NULL,
  `LastTestMessage` VARCHAR(2000) NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_khb_woocommerce_connection_profiles_ProfileName` (`ProfileName`),
  KEY `IX_khb_woocommerce_connection_profiles_EnvironmentType_IsActive` (`EnvironmentType`,`IsActive`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_category_map'
    AND BINARY COLUMN_NAME = BINARY 'CategoryName'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_category_map` ADD COLUMN `CategoryName` VARCHAR(500) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_commodity'
    AND BINARY COLUMN_NAME = BINARY 'CommodityName'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_commodity` ADD COLUMN `CommodityName` VARCHAR(500) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_package_type'
    AND BINARY COLUMN_NAME = BINARY 'PackageTitle'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_package_type` ADD COLUMN `PackageTitle` VARCHAR(300) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_package_type'
    AND BINARY COLUMN_NAME = BINARY 'PackagingPricePerPack'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_package_type` ADD COLUMN `PackagingPricePerPack` DECIMAL(18,2) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_package_type'
    AND BINARY COLUMN_NAME = BINARY 'WooPackageId'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_package_type` ADD COLUMN `WooPackageId` BIGINT NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'ProductName'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_source_product` ADD COLUMN `ProductName` VARCHAR(700) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'ProductEnglishName'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_source_product` ADD COLUMN `ProductEnglishName` VARCHAR(700) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'MainProductName'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_source_product` ADD COLUMN `MainProductName` VARCHAR(500) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'CategoryName'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_source_product` ADD COLUMN `CategoryName` VARCHAR(500) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'BrandName'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_source_product` ADD COLUMN `BrandName` VARCHAR(300) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'BrandEnglishName'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_source_product` ADD COLUMN `BrandEnglishName` VARCHAR(300) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'UnitWeightKg'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_source_product` ADD COLUMN `UnitWeightKg` DECIMAL(18,6) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'KgCashPrice'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_source_product` ADD COLUMN `KgCashPrice` DECIMAL(18,2) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'KgCreditPrice'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_source_product` ADD COLUMN `KgCreditPrice` DECIMAL(18,2) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'MainGroupId'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `MainGroupId` BIGINT NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'CategorySourceKey'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `CategorySourceKey` VARCHAR(500) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'CommoditySourceKey'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `CommoditySourceKey` VARCHAR(500) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'PackageSourceKey'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `PackageSourceKey` VARCHAR(500) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'ProductName'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `ProductName` VARCHAR(700) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'ProductEnglishName'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `ProductEnglishName` VARCHAR(700) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'BrandName'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `BrandName` VARCHAR(300) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'BrandEnglishName'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `BrandEnglishName` VARCHAR(300) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'PackageTitle'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `PackageTitle` VARCHAR(300) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'PackagingPricePerPack'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `PackagingPricePerPack` DECIMAL(18,2) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'KgCashPrice'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `KgCashPrice` DECIMAL(18,2) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'KgCreditPrice'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `KgCreditPrice` DECIMAL(18,2) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'SaleCashPrice'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `SaleCashPrice` DECIMAL(18,2) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'SaleCreditPrice'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `SaleCreditPrice` DECIMAL(18,2) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'BuyCashPrice'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `BuyCashPrice` DECIMAL(18,2) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'BuyCreditPrice'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `BuyCreditPrice` DECIMAL(18,2) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'CatalogVisibility'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `CatalogVisibility` VARCHAR(50) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'WooPayloadJson'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `WooPayloadJson` LONGTEXT NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'BulkWeightKg'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `BulkWeightKg` DECIMAL(18,6) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'MinPurchaseKg'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `MinPurchaseKg` DECIMAL(18,6) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'ImageTag'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `ImageTag` VARCHAR(300) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'SaleMode'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `SaleMode` VARCHAR(80) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'PriceCalculationBasis'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_final` ADD COLUMN `PriceCalculationBasis` VARCHAR(80) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY COLUMN_NAME = BINARY 'QueueStatus'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_update_queue` ADD COLUMN `QueueStatus` VARCHAR(50) NOT NULL DEFAULT ''pending''',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY COLUMN_NAME = BINARY 'ActionType'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_update_queue` ADD COLUMN `ActionType` VARCHAR(50) NOT NULL DEFAULT ''upsert''',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY COLUMN_NAME = BINARY 'SKU'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_update_queue` ADD COLUMN `SKU` VARCHAR(191) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY COLUMN_NAME = BINARY 'ProductSlug'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_update_queue` ADD COLUMN `ProductSlug` VARCHAR(700) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY COLUMN_NAME = BINARY 'WooProductId'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_update_queue` ADD COLUMN `WooProductId` BIGINT NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY COLUMN_NAME = BINARY 'WooPayloadJson'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_update_queue` ADD COLUMN `WooPayloadJson` LONGTEXT NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY COLUMN_NAME = BINARY 'JobId'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_update_queue` ADD COLUMN `JobId` CHAR(36) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY COLUMN_NAME = BINARY 'TryCount'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_product_update_queue` ADD COLUMN `TryCount` INT NOT NULL DEFAULT 0',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_imported_woocommerce_records'
    AND BINARY COLUMN_NAME = BINARY 'Slug'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_imported_woocommerce_records` ADD COLUMN `Slug` VARCHAR(255) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_column_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND BINARY TABLE_NAME = BINARY 'khb_imported_woocommerce_records'
    AND BINARY COLUMN_NAME = BINARY 'Title'
);
SET @khb_sql = IF(
  @khb_column_exists = 0,
  'ALTER TABLE `khb_imported_woocommerce_records` ADD COLUMN `Title` VARCHAR(512) NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_category_map'
    AND BINARY COLUMN_NAME = BINARY 'CategoryName'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_category_map'
    AND BINARY COLUMN_NAME = BINARY 'CategoryNameFa'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_category_map` SET `CategoryName` = `CategoryNameFa` WHERE `CategoryName` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_commodity'
    AND BINARY COLUMN_NAME = BINARY 'CommodityName'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_commodity'
    AND BINARY COLUMN_NAME = BINARY 'CommodityNameFa'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_commodity` SET `CommodityName` = `CommodityNameFa` WHERE `CommodityName` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_package_type'
    AND BINARY COLUMN_NAME = BINARY 'PackageTitle'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_package_type'
    AND BINARY COLUMN_NAME = BINARY 'PackageNameFa'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_package_type` SET `PackageTitle` = `PackageNameFa` WHERE `PackageTitle` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_package_type'
    AND BINARY COLUMN_NAME = BINARY 'PackagingPricePerPack'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_package_type'
    AND BINARY COLUMN_NAME = BINARY 'PackagingPricePerPackToman'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_package_type` SET `PackagingPricePerPack` = `PackagingPricePerPackToman` WHERE `PackagingPricePerPack` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'ProductName'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'ProductNameFa'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_source_product` SET `ProductName` = `ProductNameFa` WHERE `ProductName` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'ProductEnglishName'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'ProductNameEn'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_source_product` SET `ProductEnglishName` = `ProductNameEn` WHERE `ProductEnglishName` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'MainProductName'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'MainProductNameFa'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_source_product` SET `MainProductName` = `MainProductNameFa` WHERE `MainProductName` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'CategoryName'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'CategoryNameFa'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_source_product` SET `CategoryName` = `CategoryNameFa` WHERE `CategoryName` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'BrandName'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'BrandNameFa'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_source_product` SET `BrandName` = `BrandNameFa` WHERE `BrandName` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'BrandEnglishName'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'BrandNameEn'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_source_product` SET `BrandEnglishName` = `BrandNameEn` WHERE `BrandEnglishName` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'UnitWeightKg'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'PackageOneKg'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_source_product` SET `UnitWeightKg` = `PackageOneKg` WHERE `UnitWeightKg` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'KgCashPrice'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'KgCashPriceToman'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_source_product` SET `KgCashPrice` = `KgCashPriceToman` WHERE `KgCashPrice` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'KgCreditPrice'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_source_product'
    AND BINARY COLUMN_NAME = BINARY 'KgCreditPriceToman'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_source_product` SET `KgCreditPrice` = `KgCreditPriceToman` WHERE `KgCreditPrice` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'ProductName'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'ProductNameFa'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_final` SET `ProductName` = `ProductNameFa` WHERE `ProductName` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'ProductEnglishName'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'ProductNameEn'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_final` SET `ProductEnglishName` = `ProductNameEn` WHERE `ProductEnglishName` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'BrandName'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'BrandNameFa'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_final` SET `BrandName` = `BrandNameFa` WHERE `BrandName` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'BrandEnglishName'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'BrandNameEn'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_final` SET `BrandEnglishName` = `BrandNameEn` WHERE `BrandEnglishName` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'PackageTitle'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'PackageNameFa'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_final` SET `PackageTitle` = `PackageNameFa` WHERE `PackageTitle` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'PackagingPricePerPack'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'PackagingPricePerPackToman'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_final` SET `PackagingPricePerPack` = `PackagingPricePerPackToman` WHERE `PackagingPricePerPack` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'KgCashPrice'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'KgCashPriceToman'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_final` SET `KgCashPrice` = `KgCashPriceToman` WHERE `KgCashPrice` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'KgCreditPrice'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'KgCreditPriceToman'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_final` SET `KgCreditPrice` = `KgCreditPriceToman` WHERE `KgCreditPrice` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'SaleCashPrice'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'SalePriceCashToman'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_final` SET `SaleCashPrice` = `SalePriceCashToman` WHERE `SaleCashPrice` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'SaleCreditPrice'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'SalePriceCreditToman'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_final` SET `SaleCreditPrice` = `SalePriceCreditToman` WHERE `SaleCreditPrice` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'BuyCashPrice'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'PurchasePriceCashToman'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_final` SET `BuyCashPrice` = `PurchasePriceCashToman` WHERE `BuyCashPrice` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'BuyCreditPrice'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY COLUMN_NAME = BINARY 'PurchasePriceCreditToman'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_final` SET `BuyCreditPrice` = `PurchasePriceCreditToman` WHERE `BuyCreditPrice` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY COLUMN_NAME = BINARY 'QueueStatus'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY COLUMN_NAME = BINARY 'SyncStatus'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_update_queue` SET `QueueStatus` = `SyncStatus` WHERE `QueueStatus` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY COLUMN_NAME = BINARY 'WooPayloadJson'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY COLUMN_NAME = BINARY 'PayloadJson'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_update_queue` SET `WooPayloadJson` = `PayloadJson` WHERE `WooPayloadJson` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_target_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY COLUMN_NAME = BINARY 'WooProductId'
);
SET @khb_source_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY COLUMN_NAME = BINARY 'WooObjectId'
);
SET @khb_sql = IF(
  @khb_target_exists = 1 AND @khb_source_exists = 1,
  'UPDATE `khb_product_update_queue` SET `WooProductId` = `WooObjectId` WHERE `WooProductId` IS NULL',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_index_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_package_type'
    AND BINARY INDEX_NAME = BINARY 'IX_khb_package_type_WooPackageId'
);
SET @khb_sql = IF(
  @khb_index_exists = 0,
  'ALTER TABLE `khb_package_type` ADD INDEX `IX_khb_package_type_WooPackageId` (`WooPackageId`)',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_index_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY INDEX_NAME = BINARY 'IX_khb_product_final_ProductSlug'
);
SET @khb_sql = IF(
  @khb_index_exists = 0,
  'ALTER TABLE `khb_product_final` ADD INDEX `IX_khb_product_final_ProductSlug` (`ProductSlug`(191))',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_index_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_final'
    AND BINARY INDEX_NAME = BINARY 'IX_khb_product_final_WooProductId'
);
SET @khb_sql = IF(
  @khb_index_exists = 0,
  'ALTER TABLE `khb_product_final` ADD INDEX `IX_khb_product_final_WooProductId` (`WooProductId`)',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_index_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_product_update_queue'
    AND BINARY INDEX_NAME = BINARY 'IX_khb_product_update_queue_JobId'
);
SET @khb_sql = IF(
  @khb_index_exists = 0,
  'ALTER TABLE `khb_product_update_queue` ADD INDEX `IX_khb_product_update_queue_JobId` (`JobId`)',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;


SET @khb_index_exists = (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE() AND BINARY TABLE_NAME = BINARY 'khb_imported_woocommerce_records'
    AND BINARY INDEX_NAME = BINARY 'UX_khb_imported_woocommerce_records_Source_External'
);
SET @khb_duplicate_count = (
  SELECT COUNT(*) FROM (
    SELECT 1 FROM `khb_imported_woocommerce_records`
    WHERE `ExternalId` IS NOT NULL
    GROUP BY `SourceType`, `ExternalId`
    HAVING COUNT(*) > 1
  ) khb_duplicates
);
SET @khb_sql = IF(
  @khb_index_exists = 0 AND @khb_duplicate_count = 0,
  'ALTER TABLE `khb_imported_woocommerce_records` ADD UNIQUE INDEX `UX_khb_imported_woocommerce_records_Source_External` (`SourceType`, `ExternalId`)',
  'SELECT 1'
);
PREPARE khb_stmt FROM @khb_sql;
EXECUTE khb_stmt;
DEALLOCATE PREPARE khb_stmt;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260703221056_AlignCanonicalKharbarchiWorkflow20260704', '10.0.9');

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260703221623_FinalizeCanonicalKharbarchiModel20260704', '10.0.9');

COMMIT;

