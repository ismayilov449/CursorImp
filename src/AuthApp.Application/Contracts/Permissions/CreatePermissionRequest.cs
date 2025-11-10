namespace AuthApp.Application.Contracts.Permissions;

public sealed class CreatePermissionRequest
{
    public string Key { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
