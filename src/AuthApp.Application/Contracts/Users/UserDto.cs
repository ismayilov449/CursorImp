namespace AuthApp.Application.Contracts.Users;

public sealed class UserDto
{
    public Guid Id { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public IReadOnlyCollection<string> Permissions { get; init; } = Array.Empty<string>();
}
