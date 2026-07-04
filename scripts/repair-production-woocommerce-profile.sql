-- Production-only, non-destructive WooCommerce profile repair.
-- This script never reads, prints, inserts, or changes WooCommerce secrets.
-- If no usable Production profile exists, application startup creates one
-- from WooCommerce__ConsumerKey and WooCommerce__ConsumerSecret.

USE `Kharbarchi_erp`;

START TRANSACTION;

UPDATE `khb_woocommerce_connection_profiles`
SET
    `IsActive` = 0,
    `UpdatedAtUtc` = UTC_TIMESTAMP(6)
WHERE `EnvironmentType` <> 'Production'
   OR `BaseUrl` <> 'https://www.Kharbarchi.ir/'
   OR `VerifySsl` <> 1;

UPDATE `khb_woocommerce_connection_profiles`
SET
    `BaseUrl` = 'https://www.Kharbarchi.ir/',
    `VerifySsl` = 1,
    `UpdatedAtUtc` = UTC_TIMESTAMP(6)
WHERE `EnvironmentType` = 'Production';

SET @khb_production_profile_id = (
    SELECT `Id`
    FROM `khb_woocommerce_connection_profiles`
    WHERE `EnvironmentType` = 'Production'
      AND `ConsumerKey` <> ''
      AND `ProtectedConsumerSecret` <> ''
    ORDER BY `IsActive` DESC, `UpdatedAtUtc` DESC, `Id` ASC
    LIMIT 1
);

UPDATE `khb_woocommerce_connection_profiles`
SET
    `IsActive` = IF(`Id` = @khb_production_profile_id, 1, 0),
    `UpdatedAtUtc` = UTC_TIMESTAMP(6)
WHERE `EnvironmentType` = 'Production';

COMMIT;

SELECT
    `Id`,
    `ProfileName`,
    `EnvironmentType`,
    `BaseUrl`,
    `VerifySsl`,
    `IsActive`
FROM `khb_woocommerce_connection_profiles`
ORDER BY `IsActive` DESC, `Id`;
