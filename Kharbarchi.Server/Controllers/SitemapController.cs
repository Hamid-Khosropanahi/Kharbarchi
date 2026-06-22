using System.Security;
using System.Text;
using Kharbarchi.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[AllowAnonymous]
public sealed class SitemapController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public SitemapController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetSitemap(CancellationToken cancellationToken = default)
    {
        var siteUrl = (_configuration["Site:PublicUrl"] ?? "https://kharbarchi.com").TrimEnd('/');

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsAvailable)
            .OrderBy(p => p.Id)
            .Select(p => new { p.Slug })
            .ToListAsync(cancellationToken);

        var sitemap = new StringBuilder();
        sitemap.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sitemap.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        AppendUrl(sitemap, siteUrl, "/", "1.0");
        AppendUrl(sitemap, siteUrl, "/products", "0.9");

        foreach (var product in products)
        {
            AppendUrl(sitemap, siteUrl, $"/products/{Uri.EscapeDataString(product.Slug)}", "0.8");
        }

        sitemap.AppendLine("</urlset>");

        return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
    }

    private static void AppendUrl(StringBuilder sitemap, string siteUrl, string path, string priority)
    {
        var location = SecurityElement.Escape($"{siteUrl}{path}") ?? string.Empty;
        sitemap.AppendLine("  <url>");
        sitemap.AppendLine($"    <loc>{location}</loc>");
        sitemap.AppendLine($"    <priority>{priority}</priority>");
        sitemap.AppendLine("  </url>");
    }
}
