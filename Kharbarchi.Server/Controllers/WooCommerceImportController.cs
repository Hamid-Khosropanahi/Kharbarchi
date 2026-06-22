using Kharbarchi.Shared.Contracts;
using Kharbarchi.Server.Security;
using Kharbarchi.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/admin/woocommerce-import")]
[EnableRateLimiting("admin")]
[Produces("application/json")]
public sealed class WooCommerceImportController : ControllerBase
{
    private readonly WooCommerceImportService _importService;

    public WooCommerceImportController(WooCommerceImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("products")]
    [Authorize(Policy = AuthorizationPolicyNames.ProductImportWrite)]
    public async Task<ActionResult<ImportResult>> ImportProducts([FromBody] ImportWooCommerceProductsRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _importService.ImportProductsAsync(request, cancellationToken));
    }

    [HttpPost("orders")]
    [Authorize(Policy = AuthorizationPolicyNames.OrdersImportWrite)]
    public async Task<ActionResult<ImportResult>> ImportOrders([FromBody] ImportWooCommerceOrdersRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _importService.ImportOrdersAsync(request, cancellationToken));
    }
}
