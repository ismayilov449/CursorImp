using AuthApp.Application.Common;
using AuthApp.Application.Contracts.Auth;
using AuthApp.Application.Contracts.Users;

namespace AuthApp.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<UserDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
