using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthApp.Application.Interfaces;
using AuthApp.Domain.Constants;

namespace AuthApp.Api.Security;

public sealed class PermissionMiddleware(RequestDelegate next, ILogger<PermissionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<PermissionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context, IPermissionManager permissionManager)
    {
        ArgumentNullException.ThrowIfNull(context);

        var endpoint = context.GetEndpoint();
        if (endpoint is null)
        {
            await _next(context);
            return;
        }

        var permissionAttributes = endpoint.Metadata
            .GetOrderedMetadata<RequirePermissionAttribute>()
            ?.SelectMany(attr => attr.Permissions)
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct()
            .ToArray();

        if (permissionAttributes is null || permissionAttributes.Length == 0)
        {
            await _next(context);
            return;
        }

        if (context.User?.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Authentication is required.", context.RequestAborted);
            return;
        }

        if (!TryGetUserId(context.User, out var userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid user identity.", context.RequestAborted);
            return;
        }

        foreach (var requiredPermission in permissionAttributes)
        {
            var hasClaim = context.User.Claims.Any(c =>
                c.Type == AppClaimTypes.Permission && string.Equals(c.Value, requiredPermission, StringComparison.OrdinalIgnoreCase));

            if (hasClaim)
            {
                continue;
            }

            var hasPermission = await permissionManager.HasPermissionAsync(userId, requiredPermission, context.RequestAborted);
            if (!hasPermission)
            {
                _logger.LogWarning("User {UserId} does not have required permission {Permission} for {Path}.", userId, requiredPermission, context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("You do not have permission to perform this action.", context.RequestAborted);
                return;
            }
        }

        await _next(context);
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId)
    {
        userId = Guid.Empty;

        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirst(JwtRegisteredClaimNames.Sub);

        return claim is not null && Guid.TryParse(claim.Value, out userId);
    }
}
