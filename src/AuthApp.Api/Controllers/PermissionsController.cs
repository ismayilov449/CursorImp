using AuthApp.Api.Security;
using AuthApp.Application.Contracts.Permissions;
using AuthApp.Application.Interfaces;
using AuthApp.Domain.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class PermissionsController(IPermissionService permissionService, ILogger<PermissionsController> logger) : ControllerBase
{
    [HttpGet]
    [RequirePermission(PermissionCatalog.ViewPermissions)]
    [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var permissions = await permissionService.GetAllAsync(cancellationToken);
        return Ok(permissions);
    }

    [HttpGet("{key}")]
    [RequirePermission(PermissionCatalog.ViewPermissions)]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByKeyAsync(string key, CancellationToken cancellationToken)
    {
        var permission = await permissionService.GetByKeyAsync(key, cancellationToken);
        return permission is null ? NotFound() : Ok(permission);
    }

    [HttpPost]
    [RequirePermission(PermissionCatalog.ManagePermissions)]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAsync([FromBody] CreatePermissionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var permission = await permissionService.CreateAsync(request.Key, request.Name, request.Description, cancellationToken);
            return CreatedAtAction(nameof(GetByKeyAsync), new { key = permission.Key }, permission);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Failed to create permission {Key}.", request.Key);
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Permission Already Exists",
                Detail = ex.Message
            });
        }
    }
}
