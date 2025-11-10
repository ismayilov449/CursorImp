using AuthApp.Application.Contracts.Permissions;

namespace AuthApp.Application.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlyCollection<PermissionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PermissionDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<PermissionDto> CreateAsync(string key, string name, string description, CancellationToken cancellationToken = default);
}
