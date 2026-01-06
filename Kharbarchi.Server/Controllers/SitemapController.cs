using System.Text;
using Kharbarchi.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers;

[ApiController]
public class SitemapController : ControllerBase
{
    private readonly AppDbContext _context;

    public SitemapController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> GetSitemap()
    {
        var products = await _context.Products.ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
        sb.AppendLine(@"<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">");

        sb.AppendLine("<url><loc>https://kharbarchi.com/</loc></url>");
        sb.AppendLine("<url><loc>https://kharbarchi.com/products</loc></url>");

        foreach (var p in products)
        {
            sb.AppendLine($"<url><loc>https://kharbarchi.com/products/{p.Slug}</loc></url>");
        }

        sb.AppendLine("</urlset>");

        return Content(sb.ToString(), "application/xml");
    }
}