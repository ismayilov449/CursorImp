using AuthApp.Application.Interfaces;
using AuthApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Infrastructure.Permissions;

public sealed class UserPermissionService(AuthDbContext context) : IUserPermissionService
{
    public async Task AssignPermissionsAsync(Guid userId, IEnumerable<string> permissionKeys, CancellationToken cancellationToken = default)
    {
        var keys = Normalize(permissionKeys);
        if (keys.Length == 0)
        {
            return;
        }

        var user = await context.Users
            .Include(u => u.UserPermissions)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        var permissions = await context.Permissions
            .Where(p => keys.Contains(p.Key))
            .ToListAsync(cancellationToken);

        var missing = keys.Except(permissions.Select(p => p.Key), StringComparer.OrdinalIgnoreCase).ToList();
        if (missing.Count > 0)
        {
            throw new InvalidOperationException($"Permissions not found: {string.Join(", ", missing)}");
        }

        var existingKeys = user.UserPermissions
            .Select(up => up.Permission.Key.ToLowerInvariant())
            .ToHashSet();

        foreach (var permission in permissions)
        {
            if (existingKeys.Contains(permission.Key))
            {
                continue;
            }

            user.UserPermissions.Add(new Domain.Entities.UserPermission
            {
                UserId = user.Id,
                PermissionId = permission.Id
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePermissionsAsync(Guid userId, IEnumerable<string> permissionKeys, CancellationToken cancellationToken = default)
    {
        var keys = Normalize(permissionKeys);
        if (keys.Length == 0)
        {
            return;
        }

        var userPermissions = await context.UserPermissions
            .Where(up => up.UserId == userId && keys.Contains(up.Permission.Key))
            .ToListAsync(cancellationToken);

        if (userPermissions.Count == 0)
        {
            return;
        }

        context.UserPermissions.RemoveRange(userPermissions);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var permissions = await context.UserPermissions
            .AsNoTracking()
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission.Key)
            .ToListAsync(cancellationToken);

        return permissions;
    }

    private static string[] Normalize(IEnumerable<string> permissionKeys) =>
        permissionKeys?
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Select(k => k.Trim().ToLowerInvariant())
            .Distinct()
            .ToArray() ?? Array.Empty<string>();
}
