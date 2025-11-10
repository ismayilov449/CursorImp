using System.Security.Authentication;
using AuthApp.Application.Common;
using AuthApp.Application.Contracts.Auth;
using AuthApp.Application.Contracts.Users;
using AuthApp.Application.Interfaces;
using AuthApp.Application.Options;
using AuthApp.Domain.Entities;
using AuthApp.Domain.Permissions;
using AuthApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthApp.Infrastructure.Identity;

public sealed class AuthService : IAuthService
{
    private readonly AuthDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUserPermissionService _userPermissionService;
    private readonly IPermissionManager _permissionManager;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        AuthDbContext context,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService,
        IUserPermissionService userPermissionService,
        IPermissionManager permissionManager,
        IOptions<JwtOptions> jwtOptions)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _userPermissionService = userPermissionService;
        _permissionManager = permissionManager;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);

        var normalizedEmail = NormalizeEmail(request.Email);

        var emailExists = await _context.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException("Email address is already registered.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLowerInvariant(),
            NormalizedEmail = normalizedEmail,
            FirstName = request.FirstName?.Trim() ?? string.Empty,
            LastName = request.LastName?.Trim() ?? string.Empty,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var userCount = await _context.Users.CountAsync(cancellationToken);
        var defaultPermissions = userCount == 1
            ? PermissionCatalog.All
            : new[] { PermissionCatalog.ViewUsers };

        await _userPermissionService.AssignPermissionsAsync(user.Id, defaultPermissions, cancellationToken);

        await RemoveExpiredRefreshTokensAsync(user.Id, cancellationToken);

        var permissions = await _permissionManager.GetUserPermissionsAsync(user.Id, cancellationToken);

        var (accessToken, accessExpiresAt) = _tokenService.GenerateAccessToken(user, permissions);
        var refreshLifetime = TimeSpan.FromDays(_jwtOptions.RefreshTokenDays);
        var (refreshId, refreshToken, refreshExpiresAt, refreshHash, refreshSalt) = _tokenService.CreateRefreshToken(ipAddress, refreshLifetime);

        await SaveRefreshTokenAsync(user.Id, refreshId, refreshHash, refreshSalt, refreshExpiresAt, ipAddress, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return BuildAuthResponse(user, permissions, accessToken, accessExpiresAt, refreshToken, refreshExpiresAt);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);

        var normalizedEmail = NormalizeEmail(request.Email);

        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken)
            ?? throw new AuthenticationException("Invalid email or password.");

        if (!user.IsActive)
        {
            throw new AuthenticationException("User account is disabled.");
        }

        var passwordVerification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordVerification == PasswordVerificationResult.Failed)
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        await RemoveExpiredRefreshTokensAsync(user.Id, cancellationToken);

        var permissions = await _permissionManager.GetUserPermissionsAsync(user.Id, cancellationToken);
        var (accessToken, accessExpiresAt) = _tokenService.GenerateAccessToken(user, permissions);
        var refreshLifetime = TimeSpan.FromDays(_jwtOptions.RefreshTokenDays);
        var (refreshId, refreshToken, refreshExpiresAt, refreshHash, refreshSalt) = _tokenService.CreateRefreshToken(ipAddress, refreshLifetime);

        await SaveRefreshTokenAsync(user.Id, refreshId, refreshHash, refreshSalt, refreshExpiresAt, ipAddress, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return BuildAuthResponse(user, permissions, accessToken, accessExpiresAt, refreshToken, refreshExpiresAt);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        if (!TryExtractRefreshTokenId(refreshToken, out var tokenId))
        {
            throw new AuthenticationException("Invalid refresh token.");
        }

        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserPermissions)
            .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(rt => rt.Id == tokenId, cancellationToken)
            ?? throw new AuthenticationException("Refresh token not found.");

        if (!_tokenService.ValidateRefreshToken(refreshToken, storedToken))
        {
            storedToken.RevokedAtUtc = DateTime.UtcNow;
            storedToken.RevokedByIp = ipAddress;
            await _context.SaveChangesAsync(cancellationToken);

            throw new AuthenticationException("Invalid refresh token.");
        }

        if (!storedToken.IsActive)
        {
            throw new AuthenticationException("Refresh token is no longer active.");
        }

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        storedToken.RevokedByIp = ipAddress;

        var user = storedToken.User ?? throw new AuthenticationException("Refresh token user not found.");

        await RemoveExpiredRefreshTokensAsync(user.Id, cancellationToken);

        var permissions = await _permissionManager.GetUserPermissionsAsync(user.Id, cancellationToken);
        var (accessToken, accessExpiresAt) = _tokenService.GenerateAccessToken(user, permissions);
        var refreshLifetime = TimeSpan.FromDays(_jwtOptions.RefreshTokenDays);
        var (newRefreshId, newRefreshToken, newRefreshExpiresAt, refreshHash, refreshSalt) = _tokenService.CreateRefreshToken(ipAddress, refreshLifetime);

        await SaveRefreshTokenAsync(user.Id, newRefreshId, refreshHash, refreshSalt, newRefreshExpiresAt, ipAddress, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return BuildAuthResponse(user, permissions, accessToken, accessExpiresAt, newRefreshToken, newRefreshExpiresAt);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserPermissions)
            .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return user is null ? null : MapToDto(user);
    }

    public async Task<IReadOnlyCollection<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserPermissions)
            .ThenInclude(up => up.Permission)
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        return users.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await _context.Users.LongCountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var users = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserPermissions)
            .ThenInclude(up => up.Permission)
            .OrderBy(u => u.Email)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDto>
        {
            Items = users.Select(MapToDto).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    private Task SaveRefreshTokenAsync(Guid userId, Guid tokenId, string hash, string salt, DateTime expiresAt, string ipAddress, CancellationToken cancellationToken)
    {
        var refreshToken = new RefreshToken
        {
            Id = tokenId,
            UserId = userId,
            TokenHash = hash,
            TokenSalt = salt,
            ExpiresAtUtc = expiresAt,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByIp = ipAddress ?? string.Empty
        };

        return _context.RefreshTokens.AddAsync(refreshToken, cancellationToken).AsTask();
    }

    private async Task RemoveExpiredRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.ExpiresAtUtc < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (expiredTokens.Count == 0)
        {
            return;
        }

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static bool TryExtractRefreshTokenId(string refreshToken, out Guid tokenId)
    {
        tokenId = Guid.Empty;
        var segments = refreshToken.Split('.', 2);
        if (segments.Length != 2)
        {
            return false;
        }

        return Guid.TryParseExact(segments[0], "N", out tokenId);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private static AuthResponse BuildAuthResponse(User user, IReadOnlyCollection<string> permissions, string accessToken, DateTime accessExpiresAt, string refreshToken, DateTime refreshExpiresAt)
    {
        return new AuthResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = accessExpiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = refreshExpiresAt,
            Permissions = permissions,
            User = new AuthResponse.UserSummary
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            }
        };
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc,
            UpdatedAtUtc = user.UpdatedAtUtc,
            Permissions = user.UserPermissions
                .Select(up => up.Permission.Key)
                .OrderBy(p => p)
                .ToList()
        };
    }
}
