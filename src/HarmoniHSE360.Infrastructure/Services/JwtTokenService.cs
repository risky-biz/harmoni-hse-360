using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Application.Features.Authentication.DTOs;

namespace HarmoniHSE360.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _tokenExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _secretKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        _issuer = _configuration["Jwt:Issuer"] ?? "HarmoniHSE360";
        _audience = _configuration["Jwt:Audience"] ?? "HarmoniHSE360Users";
        _tokenExpirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");
        _refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
    }

    public async Task<TokenResult> GenerateTokenAsync(int userId, string email, string name, IList<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, name),
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_tokenExpirationMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        var refreshToken = GenerateRefreshToken();

        _logger.LogInformation("JWT token generated for user {UserId}", userId);

        return new TokenResult
        {
            Token = tokenString,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt
        };
    }

    public async Task<Application.Features.Authentication.DTOs.TokenValidationResult> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return new Application.Features.Authentication.DTOs.TokenValidationResult { IsValid = false };
            }

            return new Application.Features.Authentication.DTOs.TokenValidationResult
            {
                IsValid = true,
                UserId = userId,
                Email = emailClaim ?? string.Empty,
                Roles = roles
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return new Application.Features.Authentication.DTOs.TokenValidationResult { IsValid = false };
        }
    }

    public async Task<TokenResult> RefreshTokenAsync(string token, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(token);
        if (principal == null)
        {
            throw new SecurityTokenException("Invalid token");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;
        var nameClaim = principal.FindFirst(ClaimTypes.Name)?.Value;
        var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new SecurityTokenException("Invalid token claims");
        }

        // Here you would normally validate the refresh token against your database
        // For demo purposes, we'll just generate a new token

        return await GenerateTokenAsync(userId, emailClaim ?? string.Empty, nameClaim ?? string.Empty, roles);
    }

    public async Task RevokeTokenAsync(string token)
    {
        // In a production environment, you would store revoked tokens in a database or cache
        // For demo purposes, we'll just log the revocation
        _logger.LogInformation("Token revoked");
        await Task.CompletedTask;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // Don't validate lifetime for refresh
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}