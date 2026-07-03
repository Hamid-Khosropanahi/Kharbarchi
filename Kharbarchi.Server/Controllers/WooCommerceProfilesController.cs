using Kharbarchi.Server.Security;
using Kharbarchi.Server.Services;
using Kharbarchi.Shared.Contracts.WooCommerce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/admin/woocommerce-profiles")]
[Authorize(Policy = AuthorizationPolicyNames.ProductImportWrite)]
public sealed class WooCommerceProfilesController : ControllerBase
{
    private readonly WooCommerceProfileService _profiles;

    public WooCommerceProfilesController(WooCommerceProfileService profiles)
    {
        _profiles = profiles;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WooConnectionProfileDto>>> GetProfiles(CancellationToken cancellationToken) =>
        Ok(await _profiles.GetProfilesAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<WooConnectionProfileDto>> Save(
        [FromBody] WooConnectionProfileUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _profiles.SaveAsync(request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{profileId:int}/test")]
    public async Task<ActionResult<WooConnectionProfileTestResultDto>> Test(
        int profileId,
        CancellationToken cancellationToken)
    {
        var connection = await _profiles.GetConnectionAsync(profileId, cancellationToken);
        using var client = new ProfileWooCommerceClient(connection);
        var result = await client.TestAsync(cancellationToken);
        await _profiles.RecordTestAsync(profileId, result, cancellationToken);
        return result.Success ? Ok(result) : StatusCode(StatusCodes.Status503ServiceUnavailable, result);
    }
}
