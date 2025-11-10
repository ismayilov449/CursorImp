namespace AuthApp.Application.Contracts.Auth;

public sealed class AuthResponse
{
    public required string AccessToken { get; init; }
    public required DateTime AccessTokenExpiresAtUtc { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime RefreshTokenExpiresAtUtc { get; init; }
    public required UserSummary User { get; init; }
    public IReadOnlyCollection<string> Permissions { get; init; } = Array.Empty<string>();

    public sealed class UserSummary
    {
        public required Guid Id { get; init; }
        public required string Email { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
    }
}
