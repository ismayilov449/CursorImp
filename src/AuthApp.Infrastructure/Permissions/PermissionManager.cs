using AuthApp.Application.Interfaces;
using AuthApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Infrastructure.Permissions;

public sealed class PermissionManager(AuthDbContext context) : IPermissionManager
{
    public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        var normalized = permission.Trim().ToLowerInvariant();

        return await context.UserPermissions
            .AsNoTracking()
            .AnyAsync(up => up.UserId == userId && up.Permission.Key == normalized, cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var permissions = await context.UserPermissions
            .AsNoTracking()
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission.Key)
            .ToListAsync(cancellationToken);

        return permissions;
    }
}
