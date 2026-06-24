USE kharbarchi_support;

ALTER TABLE khb_imported_woocommerce_records
MODIFY COLUMN ImportedAtUtc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);

ALTER TABLE khb_imported_woocommerce_records
MODIFY COLUMN CreatedAtUtc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);

ALTER TABLE khb_imported_woocommerce_records
MODIFY COLUMN ExternalId VARCHAR(191) NULL;

ALTER TABLE khb_imported_woocommerce_records
MODIFY COLUMN SourceUrl VARCHAR(1000) NULL;

ALTER TABLE khb_imported_woocommerce_records
MODIFY COLUMN Name VARCHAR(500) NULL;

ALTER TABLE khb_imported_woocommerce_records
MODIFY COLUMN Status VARCHAR(100) NULL;

-- Keep the unique index. Repeated imports are handled by ON DUPLICATE KEY UPDATE.
