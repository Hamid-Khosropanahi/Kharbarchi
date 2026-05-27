using Kharbarchi.Server.Models;
using Kharbarchi.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
	public AppDbContext(DbContextOptions<AppDbContext> options): base(options) { }

	public DbSet<Product> Products => Set<Product>();
	public DbSet<Category> Categories => Set<Category>();
	public DbSet<Order> Orders { get; set; }
	public DbSet<OrderItem> OrderItems { get; set; }
	public DbSet<Customer> Customers { get; set; }

    // در فایل AppDbContext.cs
    public DbSet<ProductVariant> ProductVariants { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // important: keeps Identity tables

        modelBuilder.Entity<Product>()
          .HasMany(p => p.Variants)
          .WithOne(v => v.Product)
          .HasForeignKey(v => v.ProductId)
          .OnDelete(DeleteBehavior.Cascade); // اگر محصول پاک شد، وزن‌ها هم پاک شوند


        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "لوبیا", Slug = "beans" },
            new Category { Id = 2, Name = "عدس", Slug = "lentils" },
            new Category { Id = 3, Name = "نخود", Slug = "chickpeas" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "لوبیا چیتی ممتاز",
                Slug = "premium-pinto-beans",
                Description = "لوبیا چیتی تازه و باکیفیت صادراتی.",
                IsAvailable = true,
                CategoryId = 1,
                ImageUrl = "images/products/pinto-beans.jpg"
            }
        );
        // 1. انتقال جداول Identity به اسکیمای "identity"
        modelBuilder.Entity<IdentityUser>().ToTable("AspNetUsers", "sec");
        modelBuilder.Entity<IdentityRole>().ToTable("AspNetRoles", "sec");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles", "sec");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims", "sec");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins", "sec");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims", "sec");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens", "sec");

        // 2. انتقال جداول فروشگاه به اسکیمای "store"
        modelBuilder.Entity<Category>().ToTable("Categories", "GNR");
        modelBuilder.Entity<Customer>().ToTable("Customers", "CBI");
        modelBuilder.Entity<Order>().ToTable("Orders", "COM");
        modelBuilder.Entity<OrderItem>().ToTable("OrderItems", "COM");
        modelBuilder.Entity<Product>().ToTable("Products", "GNR");
        modelBuilder.Entity<ProductVariant>().ToTable("ProductVariants", "GNR");

        string adminRoleId = "f517b79d-1fc4-4800-bcb8-ee0ca67dce1e";
        string customerRoleId = "6240e185-5c3a-410b-99d3-9767571fdf24"; // The old "User" ID

        // 2. Hardcoded static GUIDs for the new roles (generated once)
        string sellerRoleId = "db563459-d897-400f-87d2-747d86f2b236";
        string adminSellerRoleId = "9a4918f4-604a-4e2b-be24-5d5d852ec6a2";

        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = null
            },
            new IdentityRole
            {
                Id = customerRoleId,
                Name = "Customer",           // Changed from User to Customer
                NormalizedName = "CUSTOMER",
                ConcurrencyStamp = null     // Kept null to match your existing database
            },
            new IdentityRole
            {
                Id = sellerRoleId,
                Name = "Seller",
                NormalizedName = "SELLER",
                ConcurrencyStamp = "a71f00a4-3221-4d32-8418-df8db3748c0f" // Hardcoded string
            },
            new IdentityRole
            {
                Id = adminSellerRoleId,
                Name = "AdminSeller",
                NormalizedName = "ADMINSELLER",
                ConcurrencyStamp = "e98e4f5a-5bc4-4845-8b38-dbb4eb5b7964" // Hardcoded string
            }
            );
         


    }
}