namespace AuthApp.Application.Contracts.Auth;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}
