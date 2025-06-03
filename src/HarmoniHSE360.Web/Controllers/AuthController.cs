using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using HarmoniHSE360.Application.Features.Authentication.Commands;
using HarmoniHSE360.Application.Features.Authentication.DTOs;
using HarmoniHSE360.Application.Common.Interfaces;
using System.Security.Authentication;

namespace HarmoniHSE360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IJwtTokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        IJwtTokenService tokenService,
        ICurrentUserService currentUserService,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _tokenService = tokenService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request?.Email ?? "null");
            
            if (request == null)
            {
                _logger.LogWarning("Login request is null");
                return BadRequest(new { message = "Invalid request data" });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login model state invalid: {@ModelState}", ModelState);
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            }

            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password,
                RememberMe = request.RememberMe
            };

            var result = await _mediator.Send(command);
            
            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            
            return Ok(result);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning("Login failed for {Email}: {Message}", request.Email, ex.Message);
            return Unauthorized(new { message = "Invalid email or password" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for {Email}", request.Email);
            return BadRequest(new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Refresh JWT token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _tokenService.RefreshTokenAsync(request.Token, request.RefreshToken);
            
            return Ok(new LoginResponse
            {
                Token = result.Token,
                RefreshToken = result.RefreshToken,
                ExpiresAt = result.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token refresh failed");
            return Unauthorized(new { message = "Invalid token" });
        }
    }

    /// <summary>
    /// Logout user and revoke token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(token))
            {
                await _tokenService.RevokeTokenAsync(token);
            }

            _logger.LogInformation("User {Email} logged out", _currentUserService.Email);
            
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout error for user {Email}", _currentUserService.Email);
            return BadRequest(new { message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
    {
        try
        {
            if (!_currentUserService.IsAuthenticated)
            {
                return Unauthorized();
            }

            // For demo purposes, return basic user info from current user service
            // In production, you might want to fetch fresh data from database
            var userProfile = new UserProfileDto
            {
                Id = _currentUserService.UserId,
                Email = _currentUserService.Email,
                Name = _currentUserService.Name,
                Roles = _currentUserService.Roles.ToList()
            };

            return Ok(userProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get current user error");
            return BadRequest(new { message = "An error occurred while fetching user profile" });
        }
    }

    /// <summary>
    /// Validate current token
    /// </summary>
    [HttpPost("validate")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult ValidateToken()
    {
        return Ok(new { valid = true, userId = _currentUserService.UserId });
    }

    /// <summary>
    /// Get demo user credentials for testing
    /// </summary>
    [HttpGet("demo-users")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetDemoUsers()
    {
        var demoUsers = new[]
        {
            new { email = "admin@bsj.sch.id", password = "Admin123!", role = "Admin", name = "System Administrator" },
            new { email = "hse.manager@bsj.sch.id", password = "HSE123!", role = "HSE Manager", name = "HSE Manager" },
            new { email = "john.doe@bsj.sch.id", password = "Employee123!", role = "Employee", name = "John Doe" },
            new { email = "jane.smith@bsj.sch.id", password = "Employee123!", role = "Employee", name = "Jane Smith" }
        };

        return Ok(new { 
            message = "Demo user credentials for testing",
            users = demoUsers 
        });
    }
}