using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Application.Features.Authentication.DTOs;
using HarmoniHSE360.Application.Features.Authentication.Commands;
using System.Security.Authentication;

namespace HarmoniHSE360.Application.Features.Authentication.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _tokenService;
    private readonly IPasswordHashService _passwordService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IJwtTokenService tokenService,
        IPasswordHashService passwordService,
        ILogger<LoginCommandHandler> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Login attempt failed for email: {Email} - User not found", request.Email);
            throw new AuthenticationException("Invalid email or password");
        }

        // Verify password using hashed password
        var isValidPassword = _passwordService.VerifyPassword(request.Password, user.PasswordHash);

        if (!isValidPassword)
        {
            _logger.LogWarning("Login attempt failed for email: {Email} - Invalid password", request.Email);
            throw new AuthenticationException("Invalid email or password");
        }

        // Extract roles and permissions
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.Permissions)
            .Select(p => p.Name)
            .Distinct()
            .ToList();

        // Generate JWT token
        var tokenResult = await _tokenService.GenerateTokenAsync(user.Id, user.Email, user.Name, roles);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        return new LoginResponse
        {
            Token = tokenResult.Token,
            RefreshToken = tokenResult.RefreshToken,
            ExpiresAt = tokenResult.ExpiresAt,
            User = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                EmployeeId = user.EmployeeId,
                Department = user.Department,
                Position = user.Position,
                Roles = roles,
                Permissions = permissions
            }
        };
    }

}