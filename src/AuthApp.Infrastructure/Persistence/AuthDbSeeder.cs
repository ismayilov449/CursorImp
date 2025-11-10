using AuthApp.Domain.Entities;
using AuthApp.Domain.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthApp.Infrastructure.Persistence;

public sealed class AuthDbSeeder(AuthDbContext context, ILogger<AuthDbSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        if (!await context.Permissions.AnyAsync(cancellationToken))
        {
            var permissions = PermissionCatalog.All.Select(key => new Permission
            {
                Key = key,
                Name = key.Split('.').Last().Replace('-', ' ').ToUpperInvariant(),
                Description = $"Allows {key.Replace('.', ' ')}"
            }).ToList();

            await context.Permissions.AddRangeAsync(permissions, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded {Count} permissions.", permissions.Count);
        }
    }
}
