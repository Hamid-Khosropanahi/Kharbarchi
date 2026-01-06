using Kharbarchi.Server.Models;
using Kharbarchi.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
	public AppDbContext(DbContextOptions<AppDbContext> options)
		: base(options) { }

	public DbSet<Product> Products => Set<Product>();
	public DbSet<Category> Categories => Set<Category>();
	public DbSet<Order> Orders { get; set; }
	public DbSet<OrderItem> OrderItems { get; set; }
	public DbSet<Customer> Customers { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder); // important: keeps Identity tables

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
				Price = 150000,
				IsAvailable = true,
				CategoryId = 1,
				ImageUrl = "images/products/pinto-beans.jpg",
				StockQuantity = 100
			}
		);
	}
}