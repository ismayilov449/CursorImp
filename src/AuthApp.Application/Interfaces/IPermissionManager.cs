namespace AuthApp.Application.Interfaces;

public interface IPermissionManager
{
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
