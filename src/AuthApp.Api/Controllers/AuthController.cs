using System.Security.Authentication;
using AuthApp.Api.Security;
using AuthApp.Application.Contracts.Auth;
using AuthApp.Application.Interfaces;
using AuthApp.Domain.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthApp.Application.Contracts.Users;

namespace AuthApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var response = await authService.RegisterAsync(request, ipAddress, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Failed to register user with email {Email}.", request.Email);
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Registration Failed",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during registration.");
            return StatusCode(StatusCodes.Status500InternalServerError, BuildProblem("An unexpected error occurred."));
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var response = await authService.LoginAsync(request, ipAddress, cancellationToken);
            return Ok(response);
        }
        catch (AuthenticationException ex)
        {
            logger.LogWarning(ex, "Failed login attempt for {Email}.", request.Email);
            return Unauthorized(BuildProblem("Invalid credentials.", StatusCodes.Status401Unauthorized));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during login.");
            return StatusCode(StatusCodes.Status500InternalServerError, BuildProblem("An unexpected error occurred."));
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var response = await authService.RefreshTokenAsync(request.RefreshToken, ipAddress, cancellationToken);
            return Ok(response);
        }
        catch (AuthenticationException ex)
        {
            logger.LogWarning(ex, "Refresh token failed.");
            return Unauthorized(BuildProblem("Invalid refresh token.", StatusCodes.Status401Unauthorized));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error refreshing token.");
            return StatusCode(StatusCodes.Status500InternalServerError, BuildProblem("An unexpected error occurred."));
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [RequirePermission(PermissionCatalog.ViewUsers)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await authService.GetByIdAsync(id, cancellationToken);
        return user is null
            ? NotFound()
            : Ok(user);
    }

    [HttpGet]
    [Authorize]
    [RequirePermission(PermissionCatalog.ViewUsers)]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await authService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("paged")]
    [Authorize]
    [RequirePermission(PermissionCatalog.ViewUsers)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPagedAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var pagedResult = await authService.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        return Ok(pagedResult);
    }

    private static ProblemDetails BuildProblem(string detail, int statusCode = StatusCodes.Status400BadRequest) =>
        new()
        {
            Status = statusCode,
            Title = "Request Failed",
            Detail = detail
        };
}
