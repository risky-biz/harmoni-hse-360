using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Harmoni360.Application.Features.Authentication.Commands;
using Harmoni360.Application.Features.Authentication.DTOs;
using Harmoni360.Application.Common.Interfaces;
using System.Security.Authentication;

namespace Harmoni360.Web.Controllers;

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
    public Task<ActionResult<UserProfileDto>> GetCurrentUser()
    {
        try
        {
            if (!_currentUserService.IsAuthenticated)
            {
                return Task.FromResult<ActionResult<UserProfileDto>>(Unauthorized());
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

            return Task.FromResult<ActionResult<UserProfileDto>>(Ok(userProfile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get current user error");
            return Task.FromResult<ActionResult<UserProfileDto>>(BadRequest(new { message = "An error occurred while fetching user profile" }));
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
            // System administration users
            new { email = "superadmin@harmoni360.com", password = "SuperAdmin123!", role = "Super Admin", name = "Super Administrator" },
            new { email = "developer@harmoni360.com", password = "Developer123!", role = "Developer", name = "System Developer" },
            new { email = "admin@harmoni360.com", password = "Admin123!", role = "Admin", name = "System Administrator" },
            
            // HSE (Health, Safety, Environment) specialized managers
            new { email = "incident.manager@harmoni360.com", password = "IncidentMgr123!", role = "Incident Manager", name = "Incident Manager" },
            new { email = "risk.manager@harmoni360.com", password = "RiskMgr123!", role = "Risk Manager", name = "Risk Manager" },
            new { email = "ppe.manager@harmoni360.com", password = "PPEMgr123!", role = "PPE Manager", name = "PPE Manager" },
            new { email = "health.monitor@harmoni360.com", password = "HealthMon123!", role = "Health Monitor", name = "Health Monitor" },
            
            // Security domain specialists - NEW for HSSE expansion
            new { email = "security.manager@harmoni360.com", password = "SecurityMgr123!", role = "Security Manager", name = "Security Manager" },
            new { email = "security.officer@harmoni360.com", password = "SecurityOfc123!", role = "Security Officer", name = "Security Officer" },
            new { email = "compliance.officer@harmoni360.com", password = "ComplianceOfc123!", role = "Compliance Officer", name = "Compliance Officer" },
            
            // Work Permit Approval Specialists - NEW for work permit workflow
            new { email = "safety.officer@harmoni360.com", password = "SafetyOfc123!", role = "Safety Officer", name = "Safety Officer" },
            new { email = "department.head@harmoni360.com", password = "DeptHead123!", role = "Department Head", name = "Department Head" },
            new { email = "hotwork.specialist@harmoni360.com", password = "HotWork123!", role = "Hot Work Specialist", name = "Hot Work Specialist" },
            new { email = "confinedspace.specialist@harmoni360.com", password = "ConfinedSpace123!", role = "Confined Space Specialist", name = "Confined Space Specialist" },
            new { email = "electrical.supervisor@harmoni360.com", password = "ElecSup123!", role = "Electrical Supervisor", name = "Electrical Supervisor" },
            new { email = "specialwork.specialist@harmoni360.com", password = "SpecialWork123!", role = "Special Work Specialist", name = "Special Work Specialist" },
            new { email = "hse.manager@harmoni360.com", password = "HSEMgr123!", role = "HSE Manager", name = "HSE Manager" },
            
            // General access roles
            new { email = "reporter@harmoni360.com", password = "Reporter123!", role = "Reporter", name = "Safety Reporter" },
            new { email = "viewer@harmoni360.com", password = "Viewer123!", role = "Viewer", name = "Safety Viewer" },
            
            // Legacy compatibility users
            new { email = "john.doe@bsj.sch.id", password = "Employee123!", role = "Reporter", name = "John Doe" },
            new { email = "jane.smith@bsj.sch.id", password = "Employee123!", role = "Viewer", name = "Jane Smith" }
        };

        return Ok(new
        {
            message = "Demo user credentials for testing",
            users = demoUsers
        });
    }
}