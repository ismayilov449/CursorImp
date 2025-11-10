namespace AuthApp.Application.Contracts.Permissions;

public sealed class UpdateUserPermissionsRequest
{
    public IReadOnlyCollection<string> Permissions { get; init; } = Array.Empty<string>();
}
