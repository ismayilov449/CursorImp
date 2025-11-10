using AuthApp.Domain.Entities;

namespace AuthApp.Application.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user, IReadOnlyCollection<string> permissions);
    (Guid TokenId, string Token, DateTime ExpiresAtUtc, string Hash, string Salt) CreateRefreshToken(string ipAddress, TimeSpan lifetime);
    bool ValidateRefreshToken(string providedToken, RefreshToken storedToken);
}
