namespace AuthApp.Application.Contracts.Permissions;

public sealed class PermissionDto
{
    public Guid Id { get; init; }
    public required string Key { get; init; }
    public required string Name { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}
