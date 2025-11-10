namespace AuthApp.Application.Interfaces;

public interface IUserPermissionService
{
    Task AssignPermissionsAsync(Guid userId, IEnumerable<string> permissionKeys, CancellationToken cancellationToken = default);
    Task RemovePermissionsAsync(Guid userId, IEnumerable<string> permissionKeys, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
