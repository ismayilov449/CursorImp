using AuthApp.Application.Contracts.Permissions;
using AuthApp.Application.Interfaces;
using AuthApp.Domain.Entities;
using AuthApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Infrastructure.Permissions;

public sealed class PermissionService(AuthDbContext context) : IPermissionService
{
    public async Task<IReadOnlyCollection<PermissionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await context.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Key)
            .ToListAsync(cancellationToken);

        return permissions.Select(MapToDto).ToList();
    }

    public async Task<PermissionDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var normalized = key.Trim().ToLowerInvariant();

        var permission = await context.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Key == normalized, cancellationToken);

        return permission is null ? null : MapToDto(permission);
    }

    public async Task<PermissionDto> CreateAsync(string key, string name, string description, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var normalizedKey = key.Trim().ToLowerInvariant();

        var exists = await context.Permissions.AnyAsync(p => p.Key == normalizedKey, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Permission '{normalizedKey}' already exists.");
        }

        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Key = normalizedKey,
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            CreatedAtUtc = DateTime.UtcNow
        };

        await context.Permissions.AddAsync(permission, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return MapToDto(permission);
    }

    private static PermissionDto MapToDto(Permission permission) =>
        new()
        {
            Id = permission.Id,
            Key = permission.Key,
            Name = permission.Name,
            Description = permission.Description,
            CreatedAtUtc = permission.CreatedAtUtc
        };
}
