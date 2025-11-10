using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthApp.Application.Interfaces;
using AuthApp.Application.Options;
using AuthApp.Domain.Constants;
using AuthApp.Domain.Entities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthApp.Infrastructure.Identity;

public sealed class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _options = options.Value;
    private readonly byte[] _signingKey = Encoding.UTF8.GetBytes(options.Value.SigningKey);

    public (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user, IReadOnlyCollection<string> permissions)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(ClaimTypes.Email, user.Email)
        };

        claims.AddRange(permissions.Select(permission => new Claim(AppClaimTypes.Permission, permission)));

        var credentials = new SigningCredentials(new SymmetricSecurityKey(_signingKey), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.WriteToken(jwt);

        return (token, expiresAt);
    }

    public (Guid TokenId, string Token, DateTime ExpiresAtUtc, string Hash, string Salt) CreateRefreshToken(string ipAddress, TimeSpan lifetime)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var tokenId = Guid.NewGuid();
        var randomToken = Convert.ToBase64String(randomBytes);
        var token = $"{tokenId:N}.{randomToken}";

        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);
        var hash = HashToken(token, salt);

        var expiresAt = DateTime.UtcNow.Add(lifetime);

        return (tokenId, token, expiresAt, hash, salt);
    }

    public bool ValidateRefreshToken(string providedToken, RefreshToken storedToken)
    {
        if (storedToken.RevokedAtUtc is not null || DateTime.UtcNow > storedToken.ExpiresAtUtc)
        {
            return false;
        }

        var hash = HashToken(providedToken, storedToken.TokenSalt);
        return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(hash), Convert.FromBase64String(storedToken.TokenHash));
    }

    private static string HashToken(string token, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);

        var hashedBytes = KeyDerivation.Pbkdf2(
            password: token,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100_000,
            numBytesRequested: 32);

        return Convert.ToBase64String(hashedBytes);
    }
}
