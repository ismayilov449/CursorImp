using AuthApp.Api.Security;
using AuthApp.Application.Contracts.Permissions;
using AuthApp.Application.Interfaces;
using AuthApp.Domain.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId:guid}/permissions")]
public sealed class UserPermissionsController(
    IUserPermissionService userPermissionService,
    IPermissionManager permissionManager,
    ILogger<UserPermissionsController> logger) : ControllerBase
{
    [HttpGet]
    [RequirePermission(PermissionCatalog.ViewUsers)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var permissions = await permissionManager.GetUserPermissionsAsync(userId, cancellationToken);
        return Ok(permissions);
    }

    [HttpPost]
    [RequirePermission(PermissionCatalog.ManagePermissions)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignAsync(Guid userId, [FromBody] UpdateUserPermissionsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await userPermissionService.AssignPermissionsAsync(userId, request.Permissions, cancellationToken);
            var permissions = await permissionManager.GetUserPermissionsAsync(userId, cancellationToken);
            return Ok(permissions);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "User {UserId} not found.", userId);
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "User Not Found",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Failed to assign permissions to user {UserId}.", userId);
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Permissions",
                Detail = ex.Message
            });
        }
    }

    [HttpDelete]
    [RequirePermission(PermissionCatalog.ManagePermissions)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveAsync(Guid userId, [FromBody] UpdateUserPermissionsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await userPermissionService.RemovePermissionsAsync(userId, request.Permissions, cancellationToken);
            var permissions = await permissionManager.GetUserPermissionsAsync(userId, cancellationToken);
            return Ok(permissions);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "User {UserId} not found.", userId);
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "User Not Found",
                Detail = ex.Message
            });
        }
    }
}
