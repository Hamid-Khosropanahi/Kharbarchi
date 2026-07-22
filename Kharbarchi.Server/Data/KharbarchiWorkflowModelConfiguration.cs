using Kharbarchi.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Data;

internal static class KharbarchiWorkflowModelConfiguration
{
    public static void ConfigureKharbarchiWorkflow(this ModelBuilder modelBuilder)
    {
        ConfigureAllProduct(modelBuilder);
        ConfigureWorkflowTables(modelBuilder);
        ConfigureJobsAndProfiles(modelBuilder);
    }

    private static void ConfigureAllProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AllProductWithProcess>(entity =>
        {
            entity.ToTable("all_product_with_process");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SourceRowHash).IsUnique().HasDatabaseName("UX_All_Product_With_Process_SourceRowHash");
            entity.HasIndex(x => x.ProductName).HasDatabaseName("IX_All_Product_With_Process_ProductName");
            entity.HasIndex(x => x.MainProductName).HasDatabaseName("IX_All_Product_With_Process_MainProductName");
            entity.HasIndex(x => x.Sku).HasDatabaseName("IX_All_Product_With_Process_SKU");
            entity.Property(x => x.SourceRowHash).HasColumnType("char(64)").IsRequired();
            entity.Property(x => x.Sku).HasColumnName("SKU").HasMaxLength(191);
            SetMaxLengths(entity,
                (nameof(AllProductWithProcess.ImportBatchId), 64),
                (nameof(AllProductWithProcess.MainProductName), 500),
                (nameof(AllProductWithProcess.MainProductSlug), 500),
                (nameof(AllProductWithProcess.GroupName), 500),
                (nameof(AllProductWithProcess.CategoryName), 500),
                (nameof(AllProductWithProcess.CategorySlug), 500),
                (nameof(AllProductWithProcess.ProductName), 700),
                (nameof(AllProductWithProcess.ProductEnglishName), 700),
                (nameof(AllProductWithProcess.ProductSlug), 700),
                (nameof(AllProductWithProcess.BrandName), 300),
                (nameof(AllProductWithProcess.BrandEnglishName), 300),
                (nameof(AllProductWithProcess.PackageName), 300),
                (nameof(AllProductWithProcess.Status), 100),
                (nameof(AllProductWithProcess.PackageOne), 300));
            ConfigureCommonProductText(entity);
            entity.Property(x => x.RawJson).HasColumnType("longtext");
            entity.Property(x => x.ShortDescription).HasColumnType("longtext");
            entity.Property(x => x.FullDescription).HasColumnType("longtext");
            entity.Property(x => x.ImageUrl).HasColumnType("longtext");
            entity.Property(x => x.GalleryJson).HasColumnType("longtext");
            SetPrecision(entity);
            SetTimestampDefaults(entity);
        });
    }

    private static void ConfigureWorkflowTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KhbProductMainGroup>(entity =>
        {
            entity.ToTable("khb_product_main_groups");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.MainProductSlug).IsUnique().HasDatabaseName("UX_khb_product_main_groups_slug");
            entity.Property(x => x.Description).HasColumnType("longtext");
            entity.Property(x => x.ImageUrl).HasColumnType("longtext");
            SetMaxLengths(entity,
                (nameof(KhbProductMainGroup.MainProductName), 500),
                (nameof(KhbProductMainGroup.MainProductSlug), 500),
                (nameof(KhbProductMainGroup.CategoryName), 500),
                (nameof(KhbProductMainGroup.EnTaxonomic), 500),
                (nameof(KhbProductMainGroup.CategorySlug), 500),
                (nameof(KhbProductMainGroup.SourceKey), 500),
                (nameof(KhbProductMainGroup.Name), 500));
            SetTimestampDefaults(entity);
        });

        modelBuilder.Entity<KhbSaleProduct>(entity =>
        {
            entity.ToTable("khb_sale_products");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SourceRowHash).IsUnique().HasDatabaseName("UX_khb_sale_products_hash");
            entity.HasIndex(x => x.WooProductId).HasDatabaseName("IX_khb_sale_products_woo");
            entity.HasIndex(x => x.Sku).HasDatabaseName("IX_khb_sale_products_sku");
            entity.HasIndex(x => x.ProductName).HasDatabaseName("IX_khb_sale_products_name");
            entity.HasIndex(x => x.ProductSlug).HasDatabaseName("IX_khb_sale_products_slug");
            entity.Property(x => x.SourceRowHash).HasColumnType("char(64)").IsRequired();
            entity.Property(x => x.Sku).HasColumnName("SKU").HasMaxLength(191);
            entity.Property(x => x.Status).HasMaxLength(100).HasDefaultValue("draft").IsRequired();
            ConfigureCommonProductText(entity);
            entity.Property(x => x.RawJson).HasColumnType("longtext");
            entity.Property(x => x.ShortDescription).HasColumnType("longtext");
            entity.Property(x => x.FullDescription).HasColumnType("longtext");
            entity.Property(x => x.ImageUrl).HasColumnType("longtext");
            entity.Property(x => x.GalleryJson).HasColumnType("longtext");
            SetMaxLengths(entity,
                (nameof(KhbSaleProduct.ProductName), 700),
                (nameof(KhbSaleProduct.ProductEnglishName), 700),
                (nameof(KhbSaleProduct.ProductSlug), 700),
                (nameof(KhbSaleProduct.BrandName), 300),
                (nameof(KhbSaleProduct.BrandEnglishName), 300),
                (nameof(KhbSaleProduct.PackageName), 300),
                (nameof(KhbSaleProduct.PackagingGroup), 50),
                (nameof(KhbSaleProduct.PackageCode), 50),
                (nameof(KhbSaleProduct.SaleMode), 80),
                (nameof(KhbSaleProduct.PriceCalculationBasis), 80));
            SetPrecision(entity);
            SetTimestampDefaults(entity);
        });

        modelBuilder.Entity<KhbSourceProduct>(entity =>
        {
            entity.ToTable("khb_source_product");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SourceKey).IsUnique().HasDatabaseName("UX_KHB_Source_Product_SourceKey");
            entity.Property(x => x.SourceKey).HasColumnType("char(64)").IsRequired();
            entity.Property(x => x.RawJson).HasColumnType("longtext");
            entity.Property(x => x.UnitWeightKg).HasPrecision(18, 6);
            entity.Property(x => x.KgCashPrice).HasPrecision(18, 2);
            entity.Property(x => x.KgCreditPrice).HasPrecision(18, 2);
            SetMaxLengths(entity,
                (nameof(KhbSourceProduct.ProductName), 700),
                (nameof(KhbSourceProduct.ProductEnglishName), 700),
                (nameof(KhbSourceProduct.MainProductName), 500),
                (nameof(KhbSourceProduct.CategoryName), 500),
                (nameof(KhbSourceProduct.CategorySlug), 500),
                (nameof(KhbSourceProduct.BrandName), 300),
                (nameof(KhbSourceProduct.BrandEnglishName), 300),
                (nameof(KhbSourceProduct.PackageOne), 300));
            SetTimestampDefaults(entity);
        });

        modelBuilder.Entity<KhbCategoryMap>(entity =>
        {
            entity.ToTable("khb_category_map");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SourceKey).IsUnique().HasDatabaseName("UX_KHB_Category_Map_SourceKey");
            SetMaxLengths(entity,
                (nameof(KhbCategoryMap.SourceKey), 500),
                (nameof(KhbCategoryMap.CategoryName), 500),
                (nameof(KhbCategoryMap.CategorySlug), 500));
            SetTimestampDefaults(entity);
        });

        modelBuilder.Entity<KhbCommodity>(entity =>
        {
            entity.ToTable("khb_commodity");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SourceKey).IsUnique().HasDatabaseName("UX_KHB_Commodity_SourceKey");
            SetMaxLengths(entity,
                (nameof(KhbCommodity.SourceKey), 500),
                (nameof(KhbCommodity.CommodityName), 500),
                (nameof(KhbCommodity.CommoditySlug), 500));
            SetTimestampDefaults(entity);
        });

        modelBuilder.Entity<KhbPackageType>(entity =>
        {
            entity.ToTable("khb_package_type");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SourceKey).IsUnique().HasDatabaseName("UX_KHB_Package_Type_SourceKey");
            entity.HasIndex(x => x.WooPackageId).HasDatabaseName("IX_khb_package_type_WooPackageId");
            entity.Property(x => x.UnitWeightKg).HasPrecision(18, 6);
            entity.Property(x => x.PackagingPricePerPack).HasPrecision(18, 2);
            SetMaxLengths(entity,
                (nameof(KhbPackageType.SourceKey), 500),
                (nameof(KhbPackageType.PackageGroup), 50),
                (nameof(KhbPackageType.PackageCode), 50),
                (nameof(KhbPackageType.PackageTitle), 300));
            SetTimestampDefaults(entity);
        });

        modelBuilder.Entity<KhbProductFinal>(entity =>
        {
            entity.ToTable("khb_product_final");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SourceKey).IsUnique().HasDatabaseName("UX_KHB_Product_Final_SourceKey");
            entity.HasIndex(x => x.Sku).HasDatabaseName("IX_KHB_Product_Final_SKU");
            entity.HasIndex(x => x.ProductSlug).HasDatabaseName("IX_khb_product_final_ProductSlug");
            entity.HasIndex(x => x.WooProductId).HasDatabaseName("IX_khb_product_final_WooProductId");
            entity.Property(x => x.SourceKey).HasColumnType("char(64)").IsRequired();
            entity.Property(x => x.Sku).HasColumnName("SKU").HasMaxLength(191);
            entity.Property(x => x.WooPayloadJson).HasColumnType("longtext");
            SetMaxLengths(entity,
                (nameof(KhbProductFinal.CategorySourceKey), 500),
                (nameof(KhbProductFinal.CommoditySourceKey), 500),
                (nameof(KhbProductFinal.PackageSourceKey), 500),
                (nameof(KhbProductFinal.ProductName), 700),
                (nameof(KhbProductFinal.ProductEnglishName), 700),
                (nameof(KhbProductFinal.ProductSlug), 700),
                (nameof(KhbProductFinal.PackageGroup), 50),
                (nameof(KhbProductFinal.PackageCode), 50),
                (nameof(KhbProductFinal.Status), 100),
                (nameof(KhbProductFinal.CatalogVisibility), 50),
                (nameof(KhbProductFinal.BrandName), 300),
                (nameof(KhbProductFinal.BrandEnglishName), 300),
                (nameof(KhbProductFinal.PackageTitle), 300),
                (nameof(KhbProductFinal.ImageTag), 300),
                (nameof(KhbProductFinal.SaleMode), 80),
                (nameof(KhbProductFinal.PriceCalculationBasis), 80));
            ConfigureCommonProductText(entity);
            SetPrecision(entity);
            SetTimestampDefaults(entity);
        });

        modelBuilder.Entity<KhbProductUpdateQueue>(entity =>
        {
            entity.ToTable("khb_product_update_queue");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SourceKey).IsUnique().HasDatabaseName("UX_KHB_Product_Update_Queue_SourceKey");
            entity.HasIndex(x => x.QueueStatus).HasDatabaseName("IX_KHB_Product_Update_Queue_Status");
            entity.HasIndex(x => x.JobId).HasDatabaseName("IX_khb_product_update_queue_JobId");
            entity.Property(x => x.SourceKey).HasColumnType("char(64)").IsRequired();
            entity.Property(x => x.Sku).HasColumnName("SKU").HasMaxLength(191);
            entity.Property(x => x.WooPayloadJson).HasColumnType("longtext");
            entity.Property(x => x.LastError).HasColumnType("longtext");
            entity.Property(x => x.JobId).HasColumnType("char(36)");
            SetMaxLengths(entity,
                (nameof(KhbProductUpdateQueue.EntityType), 80),
                (nameof(KhbProductUpdateQueue.QueueStatus), 50),
                (nameof(KhbProductUpdateQueue.ActionType), 50),
                (nameof(KhbProductUpdateQueue.ProductSlug), 700));
            SetTimestampDefaults(entity);
        });

        modelBuilder.Entity<KhbProductPriceHistory>(entity =>
        {
            entity.ToTable("khb_product_price_history");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ProductSourceKey, x.ProductType, x.PriceType, x.IsCurrent })
                .HasDatabaseName("IX_KHB_Product_Price_History_Product");
            entity.HasIndex(x => x.Sku).HasDatabaseName("IX_KHB_Product_Price_History_SKU");
            entity.HasIndex(x => new { x.ValidFromUtc, x.ValidToUtc }).HasDatabaseName("IX_KHB_Product_Price_History_Date");
            entity.Property(x => x.ProductSourceKey).HasColumnType("char(64)").IsRequired();
            entity.Property(x => x.Sku).HasColumnName("SKU").HasMaxLength(191);
            entity.Property(x => x.PriceAmount).HasPrecision(18, 2);
            SetMaxLengths(entity,
                (nameof(KhbProductPriceHistory.ProductName), 700),
                (nameof(KhbProductPriceHistory.ProductType), 80),
                (nameof(KhbProductPriceHistory.PackageGroup), 50),
                (nameof(KhbProductPriceHistory.PackageCode), 50),
                (nameof(KhbProductPriceHistory.PriceType), 80),
                (nameof(KhbProductPriceHistory.CurrencyCode), 20));
            SetTimestampDefaults(entity);
        });

        modelBuilder.Entity<KhbProductChangeLog>(entity =>
        {
            entity.ToTable("khb_product_change_log");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ProductId).HasDatabaseName("IX_khb_product_change_log_ProductId");
            entity.Property(x => x.Payload).HasColumnType("longtext");
            SetMaxLengths(entity,
                (nameof(KhbProductChangeLog.ChangeType), 100),
                (nameof(KhbProductChangeLog.Summary), 1000));
            entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        modelBuilder.Entity<KhbImportedWooCommerceRecord>(entity =>
        {
            entity.ToTable("khb_imported_woocommerce_records");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.SourceType, x.ExternalId })
                .IsUnique()
                .HasDatabaseName("UX_khb_imported_woocommerce_records_Source_External");
            entity.Property(x => x.RawJson).HasColumnType("longtext").IsRequired();
            entity.Property(x => x.SourceUrl).HasColumnType("text");
            SetMaxLengths(entity,
                (nameof(KhbImportedWooCommerceRecord.SourceType), 64),
                (nameof(KhbImportedWooCommerceRecord.ExternalId), 191),
                (nameof(KhbImportedWooCommerceRecord.Slug), 255),
                (nameof(KhbImportedWooCommerceRecord.Title), 512),
                (nameof(KhbImportedWooCommerceRecord.Name), 512),
                (nameof(KhbImportedWooCommerceRecord.Status), 128));
            entity.Property(x => x.ImportedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureJobsAndProfiles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KhbWorkflowJob>(entity =>
        {
            entity.ToTable("khb_workflow_jobs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.JobId).IsUnique();
            entity.HasIndex(x => new { x.Type, x.CreatedAtUtc });
            entity.Property(x => x.JobId).HasColumnType("char(36)");
            entity.Property(x => x.Message).HasMaxLength(2000);
            entity.Property(x => x.CreatedBy).HasMaxLength(256);
            SetMaxLengths(entity,
                (nameof(KhbWorkflowJob.Type), 50),
                (nameof(KhbWorkflowJob.Status), 50),
                (nameof(KhbWorkflowJob.CurrentStep), 160));
            SetTimestampDefaults(entity);
        });

        modelBuilder.Entity<KhbWorkflowJobLog>(entity =>
        {
            entity.ToTable("khb_workflow_job_logs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.JobId, x.CreatedAtUtc });
            entity.Property(x => x.JobId).HasColumnType("char(36)");
            entity.Property(x => x.Message).HasMaxLength(4000);
            entity.Property(x => x.RequestUrl).HasMaxLength(2000);
            entity.Property(x => x.ResponseBodySummary).HasMaxLength(4000);
            SetMaxLengths(entity,
                (nameof(KhbWorkflowJobLog.StepName), 160),
                (nameof(KhbWorkflowJobLog.EntityType), 100),
                (nameof(KhbWorkflowJobLog.EntityId), 191),
                (nameof(KhbWorkflowJobLog.Sku), 191),
                (nameof(KhbWorkflowJobLog.Status), 50));
            entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.HasOne(x => x.Job)
                .WithMany(x => x.Logs)
                .HasForeignKey(x => x.WorkflowJobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WooCommerceConnectionProfile>(entity =>
        {
            entity.ToTable("khb_woocommerce_connection_profiles");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ProfileName).IsUnique();
            entity.HasIndex(x => new { x.EnvironmentType, x.IsActive });
            entity.Property(x => x.ProtectedConsumerSecret).HasColumnType("longtext").IsRequired();
            entity.Property(x => x.ProtectedWordPressApplicationPassword).HasColumnType("longtext");
            entity.Property(x => x.LastTestMessage).HasMaxLength(2000);
            SetMaxLengths(entity,
                (nameof(WooCommerceConnectionProfile.ProfileName), 160),
                (nameof(WooCommerceConnectionProfile.EnvironmentType), 20),
                (nameof(WooCommerceConnectionProfile.BaseUrl), 1000),
                (nameof(WooCommerceConnectionProfile.ConsumerKey), 255),
                (nameof(WooCommerceConnectionProfile.WordPressUsername), 255),
                (nameof(WooCommerceConnectionProfile.ApiVersion), 40));
            SetTimestampDefaults(entity);
        });
    }

    private static void ConfigureCommonProductText<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity)
        where TEntity : class
    {
        entity.Property("ProductName").HasMaxLength(700);
        entity.Property("ProductEnglishName").HasMaxLength(700);
        entity.Property("ProductSlug").HasMaxLength(700);
    }

    private static void SetPrecision<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity)
        where TEntity : class
    {
        foreach (var propertyName in new[]
                 {
                     "UnitWeight", "UnitWeightKg", "BulkWeightKg", "MinPurchaseKg"
                 })
        {
            if (entity.Metadata.FindProperty(propertyName) is not null)
            {
                entity.Property(propertyName).HasPrecision(18, 6);
            }
        }

        foreach (var propertyName in new[]
                 {
                     "PackagingPricePerPack", "SalePriceCash", "SalePriceInstallment",
                     "PurchasePriceCash", "PurchasePriceInstallment", "KgPriceCash",
                     "KgPriceInstallment", "KgCashPrice", "KgCreditPrice", "SaleCashPrice",
                     "SaleCreditPrice", "BuyCashPrice", "BuyCreditPrice"
                 })
        {
            if (entity.Metadata.FindProperty(propertyName) is not null)
            {
                entity.Property(propertyName).HasPrecision(18, 2);
            }
        }
    }

    private static void SetMaxLengths<TEntity>(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity,
        params (string PropertyName, int MaxLength)[] properties)
        where TEntity : class
    {
        foreach (var property in properties)
        {
            entity.Property(property.PropertyName).HasMaxLength(property.MaxLength);
        }
    }

    private static void SetTimestampDefaults<TEntity>(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity)
        where TEntity : class
    {
        if (entity.Metadata.FindProperty("CreatedAtUtc") is not null)
        {
            entity.Property("CreatedAtUtc").HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        }

        if (entity.Metadata.FindProperty("UpdatedAtUtc") is not null)
        {
            entity.Property("UpdatedAtUtc").HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        }
    }
}
